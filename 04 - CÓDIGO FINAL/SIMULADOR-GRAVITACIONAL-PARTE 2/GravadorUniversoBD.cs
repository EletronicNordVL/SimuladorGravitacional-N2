using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using TrabalhoN2;

namespace SimuladorGravitacional_N2;

public class GravadorUniversoBD
{
    private int idSimulacaoAtual;
    private Dictionary<Corpo, int> mapaIdCorpos =
        new Dictionary<Corpo, int>();

    public void IniciarSimulacao(
        string nomeSimulacao,
        List<Corpo> corpos,
        int qtdIteracoes,
        double tempoEntreIteracoes)
    {
        mapaIdCorpos.Clear();

        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        {
            using (MySqlTransaction transacao = conexao.BeginTransaction())
            {
                try
                {
                    string sqlSimulacao = @"INSERT INTO Simulacao (nome, data_criacao, qtd_corpos, qtd_iteracoes, tempo_entre_iteracoes)
                    VALUES (@nome, NOW(), @qtdCorpos, @qtdIteracoes, @tempoEntreIteracoes);";

                    using (MySqlCommand cmd = new MySqlCommand(sqlSimulacao, conexao, transacao))
                    {
                        cmd.Parameters.AddWithValue("@nome", nomeSimulacao);
                        cmd.Parameters.AddWithValue("@qtdCorpos", corpos.Count);
                        cmd.Parameters.AddWithValue("@qtdIteracoes", qtdIteracoes);
                        cmd.Parameters.AddWithValue("@tempoEntreIteracoes", tempoEntreIteracoes);
                        cmd.ExecuteNonQuery();

                        this.idSimulacaoAtual = (int)cmd.LastInsertedId;
                    }

                    string sqlCorpo = @"INSERT INTO Corpo (id_simulacao, nome, massa, densidade) 
                    VALUES (@idSimulacao, @nome, @massa, @densidade);";

                    foreach (var corpo in corpos)
                    {
                        using (MySqlCommand cmdCorpo = new MySqlCommand(sqlCorpo, conexao, transacao))
                        {
                            cmdCorpo.Parameters.AddWithValue("@idSimulacao", this.idSimulacaoAtual);
                            cmdCorpo.Parameters.AddWithValue("@nome", corpo.Nome);
                            cmdCorpo.Parameters.AddWithValue("@massa", corpo.Massa);
                            cmdCorpo.Parameters.AddWithValue("@densidade", corpo.Densidade);
                            cmdCorpo.ExecuteNonQuery();

                            int idCorpoGerado = (int)cmdCorpo.LastInsertedId;
                            mapaIdCorpos[corpo] = idCorpoGerado;
                        }
                    }

                    transacao.Commit();
                    Console.WriteLine($"[BD] Simulação '{nomeSimulacao}' criada com Sucesso! (ID: {idSimulacaoAtual})");
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                    Console.WriteLine($"[ERRO BD] Falha ao iniciar simulação: {ex.Message}");
                    throw;
                }
            }
        }
    }

    public class ResumoSimulacao
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; }
        public int QuantidadeCorpos { get; set; }
        public int QuantidadeIteracoes { get; set; }
        public double TempoEntreIteracoes { get; set; }

        public ResumoSimulacao()
        {
            Nome = string.Empty;
        }
    }

    public List<Corpo> CarregarPasso(
    int idSimulacao,
    int numPasso)
    {
        List<Corpo> corpos = new List<Corpo>();

        string sql = @"
        SELECT c.nome, c.massa, c.densidade,
               ec.pos_x, ec.pos_y, ec.vel_x, ec.vel_y,
               ec.forca_x, ec.forca_y,
               ec.aceleracao_x, ec.aceleracao_y
        FROM EstadoCorpo ec
        INNER JOIN Corpo c ON c.id_corpo = ec.corpo_id
        WHERE c.id_simulacao = @idSimulacao
          AND ec.num_passo = @numPasso
        ORDER BY c.id_corpo;";

        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        {
            cmd.Parameters.AddWithValue("@idSimulacao", idSimulacao);
            cmd.Parameters.AddWithValue("@numPasso", numPasso);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Corpo corpo = new Corpo(
                        reader.GetString("nome"),
                        reader.GetDouble("massa"),
                        reader.GetDouble("densidade"),
                        reader.GetDouble("pos_x"),
                        reader.GetDouble("pos_y"),
                        reader.GetDouble("vel_x"),
                        reader.GetDouble("vel_y"));

                    corpo.ForcaX = reader.GetDouble("forca_x");
                    corpo.ForcaY = reader.GetDouble("forca_y");
                    corpo.AceleracaoX =
                        reader.GetDouble("aceleracao_x");
                    corpo.AceleracaoY =
                        reader.GetDouble("aceleracao_y");

                    corpos.Add(corpo);
                }
            }
        }

        return corpos;
    }

    public void GravarPasso(int numPasso, double tempoDecorrido, List<Corpo> corpos)
    {
        if (idSimulacaoAtual == 0)
        {
            throw new InvalidOperationException(
                "Inicie uma simulação antes de gravar os passos.");
        }

        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        {
            using (MySqlTransaction transacao = conexao.BeginTransaction())
            {
                try
                {
                    string sqlEstado = @"INSERT INTO EstadoCorpo
         (corpo_id, num_passo, tempo_decorrido, pos_x, pos_y, vel_x, vel_y, forca_x, forca_y, aceleracao_x, aceleracao_y)
         VALUES
         (@corpoId, @numPasso, @tempoDecorrido, @posX, @posY, @velX, @velY, @forcaX, @forcaY, @aceleracaoX, @aceleracaoY);";

                    foreach (var corpo in corpos)
                    {
                        using (MySqlCommand cmd = new MySqlCommand(sqlEstado, conexao, transacao))
                        {
                            int idCorpo;

                            if (!mapaIdCorpos.TryGetValue(corpo, out idCorpo))
                            {
                                throw new InvalidOperationException(
                                    "Um dos corpos não pertence à simulação iniciada.");
                            }

                            cmd.Parameters.AddWithValue("@corpoId", idCorpo);
                            cmd.Parameters.AddWithValue("@numPasso", numPasso);
                            cmd.Parameters.AddWithValue("@tempoDecorrido", tempoDecorrido);
                            cmd.Parameters.AddWithValue("@posX", corpo.PosX);
                            cmd.Parameters.AddWithValue("@posY", corpo.PosY);
                            cmd.Parameters.AddWithValue("@velX", corpo.VelX);
                            cmd.Parameters.AddWithValue("@velY", corpo.VelY);
                            cmd.Parameters.AddWithValue("@forcaX", corpo.ForcaX);
                            cmd.Parameters.AddWithValue("@forcaY", corpo.ForcaY);
                            cmd.Parameters.AddWithValue("@aceleracaoX", corpo.AceleracaoX);
                            cmd.Parameters.AddWithValue("@aceleracaoY", corpo.AceleracaoY);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                    Console.WriteLine($"[ERRO BD] Falha ao gravar o passo {numPasso}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}

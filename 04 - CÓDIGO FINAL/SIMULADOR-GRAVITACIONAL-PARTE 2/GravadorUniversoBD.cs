using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TrabalhoN2;

namespace SimuladorGravitacional_N2;

public class GravadorUniversoBD : IDisposable
{
    private int idSimulacaoAtual;
    private Dictionary<Corpo, int> mapaIdCorpos =
        new Dictionary<Corpo, int>();

    // Linhas de EstadoCorpo enviadas ao banco em cada INSERT.
    // De 500 a 1000 equilibra poucas idas ao banco e pacotes pequenos.
    private const int LinhasPorBloco = 500;

    // Conexão mantida aberta durante a simulação (evita abrir uma por passo).
    private MySqlConnection? conexaoGravacao;

    // Linhas (cópias dos valores dos corpos) esperando para serem gravadas.
    private readonly List<LinhaEstado> bufferLinhas = new List<LinhaEstado>();

    // Marca que um bloco falhou: o FinalizarSimulacao não tenta gravar de novo.
    private bool erroNaGravacao;

    // Mede SOMENTE o tempo escrevendo no banco, para o relatório separar
    // tempo de cálculo e tempo de gravação.
    private readonly Stopwatch cronometroGravacao = new Stopwatch();
    public TimeSpan TempoTotalGravacao => cronometroGravacao.Elapsed;

    // "Foto" do corpo em um passo. É uma cópia dos valores porque o objeto
    // Corpo muda na iteração seguinte, antes de a linha ser gravada.
    private readonly struct LinhaEstado
    {
        public readonly int CorpoId;
        public readonly int NumPasso;
        public readonly double TempoDecorrido, PosX, PosY, VelX, VelY;
        public readonly double ForcaX, ForcaY, AceleracaoX, AceleracaoY;

        public LinhaEstado(int corpoId, int numPasso, double tempoDecorrido, Corpo c)
        {
            CorpoId = corpoId;
            NumPasso = numPasso;
            TempoDecorrido = tempoDecorrido;
            PosX = c.PosX; PosY = c.PosY;
            VelX = c.VelX; VelY = c.VelY;
            ForcaX = c.ForcaX; ForcaY = c.ForcaY;
            AceleracaoX = c.AceleracaoX; AceleracaoY = c.AceleracaoY;
        }
    }

    public int IniciarSimulacao(
        string nomeSimulacao,
        List<Corpo> corpos,
        int qtdIteracoes,
        double tempoEntreIteracoes)
    {
        mapaIdCorpos.Clear();
        bufferLinhas.Clear();
        erroNaGravacao = false;
        cronometroGravacao.Reset();

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
                    cronometroGravacao.Stop();
                    Console.WriteLine(
                        $"[BD] Simulação '{nomeSimulacao}' criada com Sucesso! " +
                        $"(ID: {idSimulacaoAtual})");

                    return idSimulacaoAtual;
                }
                catch (Exception ex)
                {
                    cronometroGravacao.Stop();
                    transacao.Rollback();
                    mapaIdCorpos.Clear();
                    idSimulacaoAtual = 0;
                    Console.WriteLine($"[ERRO BD] Falha ao iniciar simulação: {ex.Message}");
                    throw;
                }
            }
        }
    }

    public int ExcluirTodasSimulacoes()
    {
        string sql = "DELETE FROM Simulacao;";

        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        {
            int linhas = cmd.ExecuteNonQuery();
            LimparEstadoDaSimulacaoAtual();      // <<< NOVO
            return linhas;
        }
    }

    public bool ExcluirSimulacao(int idSimulacao)
    {
        string sql = @"
        DELETE FROM Simulacao
        WHERE id_simulacao = @idSimulacao;";

        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        {
            cmd.Parameters.AddWithValue("@idSimulacao", idSimulacao);

            int linhasAfetadas = cmd.ExecuteNonQuery();

            if (idSimulacao == idSimulacaoAtual)
            {
                LimparEstadoDaSimulacaoAtual();
            }

            return linhasAfetadas > 0;
        }
    }

    private void LimparEstadoDaSimulacaoAtual()
    {
        mapaIdCorpos.Clear();
        bufferLinhas.Clear();
        idSimulacaoAtual = 0;
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

    public List<ResumoSimulacao> ListarSimulacoes()
    {
        List<ResumoSimulacao> simulacoes =
            new List<ResumoSimulacao>();

        string sql = @"
        SELECT id_simulacao, nome, data_criacao,
               qtd_corpos, qtd_iteracoes, tempo_entre_iteracoes
        FROM Simulacao
        ORDER BY data_criacao DESC, id_simulacao DESC;";

        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                ResumoSimulacao simulacao =
                    new ResumoSimulacao();

                simulacao.Id =
                    reader.GetInt32("id_simulacao");
                simulacao.Nome =
                    reader.GetString("nome");
                simulacao.DataCriacao =
                    reader.GetDateTime("data_criacao");
                simulacao.QuantidadeCorpos =
                    reader.GetInt32("qtd_corpos");
                simulacao.QuantidadeIteracoes =
                    reader.GetInt32("qtd_iteracoes");
                simulacao.TempoEntreIteracoes =
                    reader.GetDouble("tempo_entre_iteracoes");

                simulacoes.Add(simulacao);
            }
        }

        return simulacoes;
    }

    /* Método para gravar o estado de todos os corpos em um determinado passo da simulação */
    public void GravarPasso(int numPasso, double tempoDecorrido, List<Corpo> corpos)
    {
        if (idSimulacaoAtual == 0)
        {
            throw new InvalidOperationException(
                "Inicie uma simulação antes de gravar os passos.");
        }

        foreach (var corpo in corpos)
        {
            if (!mapaIdCorpos.TryGetValue(corpo, out int idCorpo))
            {
                throw new InvalidOperationException(
                    "Um dos corpos não pertence à simulação iniciada.");
            }

            bufferLinhas.Add(new LinhaEstado(idCorpo, numPasso, tempoDecorrido, corpo));
        }

        if (bufferLinhas.Count >= LinhasPorBloco)
        {
            DescarregarBuffer();
        }
    }

    /* Grava no banco todas as linhas acumuladas, com UM INSERT de várias linhas
     * dentro de uma transação (ou grava o bloco inteiro, ou nada dele). */
    private void DescarregarBuffer()
    {
        if (bufferLinhas.Count == 0)
        {
            return;
        }

        cronometroGravacao.Start();
        try
        {
            /* Abre a conexão só na primeira vez; depois ela é reaproveitada. */
            conexaoGravacao ??= ConexaoBD.ObterConexao();

            using (MySqlTransaction transacao = conexaoGravacao.BeginTransaction())
            {
                try
                {
                    StringBuilder sql = new StringBuilder(
                        "INSERT INTO EstadoCorpo (corpo_id, num_passo, tempo_decorrido, " +
                        "pos_x, pos_y, vel_x, vel_y, forca_x, forca_y, aceleracao_x, aceleracao_y) VALUES ");

                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conexaoGravacao;
                        cmd.Transaction = transacao;

                        /* Cada linha ganha parâmetros com sufixo próprio (@c0, @n0, @c1, ...) */
                        for (int i = 0; i < bufferLinhas.Count; i++)
                        {
                            LinhaEstado l = bufferLinhas[i];

                            if (i > 0) sql.Append(',');
                            sql.Append($"(@c{i},@n{i},@t{i},@px{i},@py{i},@vx{i},@vy{i},@fx{i},@fy{i},@ax{i},@ay{i})");

                            cmd.Parameters.AddWithValue($"@c{i}", l.CorpoId);
                            cmd.Parameters.AddWithValue($"@n{i}", l.NumPasso);
                            cmd.Parameters.AddWithValue($"@t{i}", l.TempoDecorrido);
                            cmd.Parameters.AddWithValue($"@px{i}", l.PosX);
                            cmd.Parameters.AddWithValue($"@py{i}", l.PosY);
                            cmd.Parameters.AddWithValue($"@vx{i}", l.VelX);
                            cmd.Parameters.AddWithValue($"@vy{i}", l.VelY);
                            cmd.Parameters.AddWithValue($"@fx{i}", l.ForcaX);
                            cmd.Parameters.AddWithValue($"@fy{i}", l.ForcaY);
                            cmd.Parameters.AddWithValue($"@ax{i}", l.AceleracaoX);
                            cmd.Parameters.AddWithValue($"@ay{i}", l.AceleracaoY);
                        }

                        cmd.CommandText = sql.ToString();
                        cmd.ExecuteNonQuery();
                    }

                    transacao.Commit();
                    bufferLinhas.Clear();
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                    Console.WriteLine($"[ERRO BD] Falha ao gravar um bloco de passos: {ex.Message}");
                    throw;
                }
            }
        }
        catch (Exception)
        {
            erroNaGravacao = true;
            throw;
        }
        finally
        {
            cronometroGravacao.Stop();
        }
    }

    /* Deve ser chamado ao fim da simulação: grava o último bloco (quase sempre menor
     * que LinhasPorBloco) e fecha a conexão. Sem isso, as últimas linhas ficariam só
     * na memória e o replay as perderia. */
    public void FinalizarSimulacao()
    {
        try
        {
            if (!erroNaGravacao)
            {
                DescarregarBuffer();
            }
        }
        finally
        {
            FecharConexao();
            bufferLinhas.Clear();
        }
    }

    private void FecharConexao()
    {
        conexaoGravacao?.Dispose();
        conexaoGravacao = null;
    }

    /* Rede de segurança caso o FinalizarSimulacao não seja chamado. */
    public void Dispose()
    {
        FecharConexao();
        bufferLinhas.Clear();
    }
}

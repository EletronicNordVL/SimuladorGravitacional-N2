using Google.Protobuf.Collections;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TrabalhoN2;

namespace SimuladorGravitacional_N2;

public class GravadorUniversoBD
{
    private int idSimulacaoAtual;
    private Dictionary<Corpo, int> mapaIdCorpos = new Dictionary<Corpo, int>();

    public void IniciarSimulacao(string nomeSimulacao, List<Corpo> corpos, int qtdIteracoes, double tempoEntreIteracoes)
    {
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

    public void GravarPasso(int numPasso, double tempoDecorrido, List<Corpo> corpos)
    {
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
                            cmd.Parameters.AddWithValue("@corpoId", mapaIdCorpos[corpo]);
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


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

    /* Grava várias linhas de uma vez, em um único INSERT, para reduzir
     * overhead de comunicação com o banco. */
    private const int LinhasPorBloco = 500;

    /* Conexão aberta para gravar os blocos de linhas. É aberta na primeira
     * vez que o DescarregarBuffer é chamado, e fechada no FinalizarSimulacao. */
    private MySqlConnection? conexaoGravacao;

    /* Buffer de linhas a gravar. Cada linha é uma "foto" do estado de um corpo
     * em um passo da simulação. */
    private readonly List<LinhaEstado> bufferLinhas = new List<LinhaEstado>();

    /* Indica se houve algum erro na gravação. 
     * Se houver, o FinalizarSimulacao não tenta gravar o que sobrou no buffer, 
     * porque provavelmente vai falhar de novo. */
    private bool erroNaGravacao;

    /* Cronômetro para medir o tempo total gasto na gravação dos passos da simulação. */
    private readonly Stopwatch cronometroGravacao = new Stopwatch();
    public TimeSpan TempoTotalGravacao => cronometroGravacao.Elapsed;

    /* Estrutura para armazenar o estado de um corpo em um passo da simulação. */
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

    /* Inicia uma nova simulação no banco de dados, criando registros para a simulação e seus corpos.
     * Retorna o ID da simulação criada. */
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

        /* Abre uma conexão com o banco de dados e inicia uma 
         * transação para garantir que todas as operações sejam atômicas. */
        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        {
            using (MySqlTransaction transacao = conexao.BeginTransaction())
            {
                try
                {
                    /* Insere um registro na tabela Simulacao com as informações fornecidas, 
                     * incluindo o nome da simulação, quantidade de corpos, quantidade de iterações 
                     * e tempo entre iterações. */
                    string sqlSimulacao = @"INSERT INTO Simulacao (nome, data_criacao, qtd_corpos, qtd_iteracoes, tempo_entre_iteracoes)
                    VALUES (@nome, NOW(), @qtdCorpos, @qtdIteracoes, @tempoEntreIteracoes);";

                    using (MySqlCommand cmd = new MySqlCommand(sqlSimulacao, conexao, transacao))
                    {
                        /* Adiciona os parâmetros necessários para a inserção da simulação no banco de dados, 
                         * incluindo o nome da simulação, quantidade de corpos, quantidade de iterações 
                         * e tempo entre iterações. */
                        cmd.Parameters.AddWithValue("@nome", nomeSimulacao);
                        cmd.Parameters.AddWithValue("@qtdCorpos", corpos.Count);
                        cmd.Parameters.AddWithValue("@qtdIteracoes", qtdIteracoes);
                        cmd.Parameters.AddWithValue("@tempoEntreIteracoes", tempoEntreIteracoes);
                        cmd.ExecuteNonQuery();

                        this.idSimulacaoAtual = (int)cmd.LastInsertedId;
                    }

                    /* Cria registros para cada corpo na tabela Corpo, associando-os à simulação atual. 
                     * O ID gerado para cada corpo é armazenado no mapa de corpos para referência futura. */
                    string sqlCorpo = @"INSERT INTO Corpo (id_simulacao, nome, massa, densidade) 
                    VALUES (@idSimulacao, @nome, @massa, @densidade);";

                    foreach (var corpo in corpos)
                    {
                        /* Para cada corpo, cria um registro no banco de dados e armazena o ID 
                         * gerado no mapa de corpos. */
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

                    /* Se todas as operações foram bem-sucedidas, a transação é confirmada, 
                     * o cronômetro de gravação é parado e o ID da simulação atual é retornado. */
                    transacao.Commit();
                    cronometroGravacao.Stop();
                    Console.WriteLine(
                        $"[BD] Simulação '{nomeSimulacao}' criada com Sucesso! " +
                        $"(ID: {idSimulacaoAtual})");

                    return idSimulacaoAtual;
                }
                catch (Exception ex)
                {
                    /* Em caso de erro, a transação é revertida, 
                     * o mapa de corpos é limpo e o ID da simulação atual é redefinido para 0. */
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

    /* Exclui todas as simulações do banco de dados. Retorna o número de linhas afetadas. */
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

    /* Exclui uma simulação específica do banco de dados. Retorna true se a exclusão foi bem-sucedida, false caso contrário. */
    public bool ExcluirSimulacao(int idSimulacao)
    {
        string sql = @"
        DELETE FROM Simulacao
        WHERE id_simulacao = @idSimulacao;";

        /* Abre uma conexão com o banco de dados e executa a exclusão da simulação especificada. 
         * Se a simulação excluída for a simulação atual, o estado da simulação atual é limpo. */
        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        {
            cmd.Parameters.AddWithValue("@idSimulacao", idSimulacao);

            int linhasAfetadas = cmd.ExecuteNonQuery();

            /* Se a simulação excluída for a simulação atual, limpa o estado da simulação atual. */
            if (idSimulacao == idSimulacaoAtual)
            {
                LimparEstadoDaSimulacaoAtual();
            }

            return linhasAfetadas > 0;
        }
    }

    /* Limpa o estado da simulação atual, incluindo o mapa de corpos e o buffer de linhas. */
    private void LimparEstadoDaSimulacaoAtual()
    {
        /* Limpa o mapa de corpos e o buffer de linhas, e redefine o ID da simulação atual para 0. */
        mapaIdCorpos.Clear();
        bufferLinhas.Clear();
        idSimulacaoAtual = 0;
    }

    /* Classe para armazenar o resumo de uma simulação, incluindo informações como ID, nome, 
     * data de criação, quantidade de corpos, quantidade de iterações e tempo entre iterações. */
    public class ResumoSimulacao
    {
        /* Propriedade para armazenar o ID da simulação. */
        public int Id { get; set; }
        /* Propriedade para armazenar o nome da simulação. */
        public string Nome { get; set; }
        /* Propriedade para armazenar a data de criação da simulação. */
        public DateTime DataCriacao { get; set; }
        /* Propriedade para armazenar a quantidade de corpos na simulação. */
        public int QuantidadeCorpos { get; set; }
        /* Propriedade para armazenar a quantidade de iterações na simulação. */
        public int QuantidadeIteracoes { get; set; }
        /* Propriedade para armazenar o tempo entre iterações na simulação. */
        public double TempoEntreIteracoes { get; set; }

        public ResumoSimulacao()
        {
            Nome = string.Empty;
        }
    }

    /* Carrega o estado de todos os corpos em um determinado passo da simulação. 
     * Retorna uma lista de objetos Corpo representando o estado dos corpos nesse passo. */
    public List<Corpo> CarregarPasso(
    int idSimulacao,
    int numPasso)
    {
        List<Corpo> corpos = new List<Corpo>();

        /* Consulta do MySQL para obter o estado dos corpos em um determinado passo da simulação. 
         * A consulta seleciona informações como nome, massa, densidade, posição, velocidade, 
         * força e aceleração. */
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

        /* Abre uma conexão com o banco de dados e executa a consulta para obter o estado dos corpos. 
         * Os resultados são lidos e armazenados em uma lista de objetos Corpo. */
        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        {
            cmd.Parameters.AddWithValue("@idSimulacao", idSimulacao);
            cmd.Parameters.AddWithValue("@numPasso", numPasso);

            /* Executa a consulta e lê os resultados usando um MySqlDataReader. 
             * Para cada linha retornada, cria um objeto Corpo com 
             * as informações do estado do corpo. */
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    /* Cria um objeto Corpo com as informações obtidas do banco de dados. 
                     * O construtor do Corpo recebe os valores de nome, massa, densidade, posição e velocidade. 
                     * Em seguida, os valores de força e aceleração são atribuídos 
                     * às propriedades correspondentes. */
                    Corpo corpo = new Corpo(
                        reader.GetString("nome"),
                        reader.GetDouble("massa"),
                        reader.GetDouble("densidade"),
                        reader.GetDouble("pos_x"),
                        reader.GetDouble("pos_y"),
                        reader.GetDouble("vel_x"),
                        reader.GetDouble("vel_y"));

                    /* Atribui os valores de força e aceleração obtidos do banco de dados 
                     * às propriedades correspondentes do objeto Corpo. */
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

    /* Lista todas as simulações armazenadas no banco de dados, 
     * retornando uma lista de objetos ResumoSimulacao contendo 
     * informações resumidas sobre cada simulação. */
    public List<ResumoSimulacao> ListarSimulacoes()
    {
        List<ResumoSimulacao> simulacoes =
            new List<ResumoSimulacao>();

        /* Consulta SQL para selecionar informações resumidas de todas as simulações, 
         * ordenadas por data de criação e ID da simulação em ordem decrescente. */
        string sql = @"
        SELECT id_simulacao, nome, data_criacao,
               qtd_corpos, qtd_iteracoes, tempo_entre_iteracoes
        FROM Simulacao
        ORDER BY data_criacao DESC, id_simulacao DESC;";

        /* Abre uma conexão com o banco de dados, executa a consulta e lê os resultados usando um 
         * MySqlDataReader. Para cada linha retornada, cria um objeto ResumoSimulacao 
         * com as informações da simulação. */
        using (MySqlConnection conexao = ConexaoBD.ObterConexao())
        using (MySqlCommand cmd = new MySqlCommand(sql, conexao))
        using (MySqlDataReader reader = cmd.ExecuteReader())
        {
            /* Lê os resultados da consulta e cria objetos ResumoSimulacao para cada simulação encontrada. 
             * Cada objeto é preenchido com informações como ID, nome, data de criação, quantidade de corpos, 
             * quantidade de iterações e tempo entre iterações. */
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

        /* Retorna a lista de simulações encontradas no banco de dados. */
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

        /* Inicia o cronômetro para medir o tempo gasto na gravação do bloco de linhas. */
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

                    /* Cria um comando MySQL para executar a inserção em lote. Cada linha do 
                     * buffer é adicionada como um conjunto de parâmetros com sufixos únicos 
                     * para evitar conflitos. */
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

    /* Fecha a conexão com o banco de dados, se estiver aberta. */
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

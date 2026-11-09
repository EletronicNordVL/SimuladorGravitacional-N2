using System;
using MySql.Data.MySqlClient;

/* O namespace SimuladorGravitacional_N2 contém classes relacionadas à simulação gravitacional, 
 * incluindo a classe ConexaoBD para gerenciar a conexão com o banco de dados MySQL. */
namespace SimuladorGravitacional_N2;

/* Essa classe fornece métodos para gerenciar a conexão com o banco de dados MySQL. */
public static class ConexaoBD
{
    /* Obtém a string de conexão do banco de dados a partir da variável de ambiente "SIMULADOR_DB_CONNECTION". */
    private static readonly string stringConexao =
        /* Tenta obter a variável de ambiente "SIMULADOR_DB_CONNECTION". Se não estiver definida, usa uma string vazia. */
        Environment.GetEnvironmentVariable("SIMULADOR_DB_CONNECTION") ?? "";

    /* Método para obter uma conexão aberta com o banco de dados MySQL. */
    public static MySqlConnection ObterConexao()
    {
        /* Verifica se a string de conexão está vazia. Se estiver, 
         * lança uma exceção informando que a variável de conexão não foi configurada. */
        if (string.IsNullOrWhiteSpace(stringConexao))
        {
            /* Lança uma exceção informando que a variável de conexão com o MySQL não foi configurada. */
            throw new InvalidOperationException(
                "A variável de conexão com o MySQL não foi configurada.");
        }

        /* Cria uma nova instância de MySqlConnection usando a string de conexão obtida. */
        MySqlConnection conexao = new MySqlConnection(stringConexao);

        /* Tenta abrir a conexão com o banco de dados. Se ocorrer algum erro, 
         * captura a exceção, exibe uma mensagem de erro e lança a exceção novamente. */
        try
        {
            /* Tenta abrir a conexão com o banco de dados MySQL. */
            conexao.Open();
            return conexao;
        }
        /* Captura qualquer exceção que ocorra ao tentar abrir a conexão. */
        catch (Exception ex)
        {
            /* Exibe uma mensagem de erro no console informando que houve uma falha ao conectar ao MySQL, 
             * incluindo a mensagem da exceção capturada. */
            Console.WriteLine($"[ERRO BD] Falha ao conectar ao MySQL: {ex.Message}");
            /* Descarta a conexão aberta (se houver) e lança a exceção novamente para que o chamador possa lidar com ela. */
            conexao.Dispose();
            throw;
        }
    }
}

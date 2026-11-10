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
            "A variável de ambiente SIMULADOR_DB_CONNECTION não está configurada. " +
            "Defina-a com a string de conexão do MySQL e reinicie o Visual Studio " +
            "(ou o terminal) para que o programa enxergue o novo valor.");
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
        catch
        {
            /* Descarta a conexão e relança a exceção original. Quem exibe o erro ao
             * usuário é o Program.cs (try/catch do menu), para a mensagem não repetir. */
            conexao.Dispose();
            throw;
        }
    }
}

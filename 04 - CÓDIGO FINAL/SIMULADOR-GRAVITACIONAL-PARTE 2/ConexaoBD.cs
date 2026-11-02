
using System;
using MySql.Data.MySqlClient;

namespace SimuladorGravitacional_N2;

public static class ConexaoBD
{
    private static readonly string stringConexao =
        Environment.GetEnvironmentVariable("SIMULADOR_DB_CONNECTION") ?? "";

    public static MySqlConnection ObterConexao()
    {
        if (string.IsNullOrWhiteSpace(stringConexao))
        {
            throw new InvalidOperationException(
                "A variável de conexão com o MySQL não foi configurada.");
        }

        MySqlConnection conexao = new MySqlConnection(stringConexao);

        try
        {
            conexao.Open();
            return conexao;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO BD] Falha ao conectar ao MySQL: {ex.Message}");
            conexao.Dispose();
            throw;
        }
    }
}
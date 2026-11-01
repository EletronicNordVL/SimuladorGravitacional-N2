using System;
using MySql.Data.MySqlClient;

namespace SimuladorGravitacional_N2;

public static class ConexaoBD
{
    private static string stringConexao = 
        "Server=localhost;" +
        "Database=simulador_gravitacional;" +
        "Uid=root;" +
        "Pwd=;";

    public static MySqlConnection ObterConexao()
    {
        MySqlConnection conexao = new MySqlConnection(stringConexao);

        try
        {
            conexao.Open();
            return conexao;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO BD] Falha ao conectar ao MySQL: {ex.Message}");
            throw;
      }
    }
  }
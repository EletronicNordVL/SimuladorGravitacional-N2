using System;
using System.IO;
using System.Globalization;
namespace TrabalhoN1;

/* Essa é a classe abstrata para gravar os dados do universo em um arquivo txt. */
public abstract class GravadorUniverso
{
    /* Essa função salva os dados do universo em um arquivo txt. */
    public abstract void Salvar(Universo universo, string caminho);

    /* Essa função tem a função de ler os dados do arquivo txt e cria um objeto Universo com os corpos carregados. */
    public abstract Universo Carregar(string caminho);
}

/* Essa é a classe concreta que implementa a gravação e leitura dos dados do universo em um arquivo txt. */
public class GravadorArquivoTexto : GravadorUniverso
{
    /* Essa função salva os dados do universo em um arquivo txt. */
    public override void Salvar(Universo universo, string caminho)
    {
        using StreamWriter arquivo = new StreamWriter(caminho);

        arquivo.WriteLine(
            $"{universo.Corpos.Count};" +
            $"{universo.QuantidadeIteracoes};" +
            universo.TempoEntreIteracoes.ToString("R", CultureInfo.InvariantCulture)
        );

        /* Salvar os dados de cada corpo no arquivo. */
        foreach (Corpo corpo in universo.Corpos)
        {
            arquivo.WriteLine(
                $"{corpo.Nome};" +
                corpo.Massa.ToString("R", CultureInfo.InvariantCulture) + ";" +
                corpo.Densidade.ToString("R", CultureInfo.InvariantCulture) + ";" +
                corpo.PosX.ToString("R", CultureInfo.InvariantCulture) + ";" +
                corpo.PosY.ToString("R", CultureInfo.InvariantCulture) + ";" +
                corpo.VelX.ToString("R", CultureInfo.InvariantCulture) + ";" +
                corpo.VelY.ToString("R", CultureInfo.InvariantCulture)
            );
        }
    }

    /* Essa função lê os dados do arquivo txt e cria um objeto Universo com os corpos carregados. */
    public override Universo Carregar(string caminho)
    {
        /* Abrir o arquivo para leitura. */
        using StreamReader arquivo = new StreamReader(caminho);

        /* Ler a primeira linha do arquivo, que contém a quantidade de corpos, quantidade de iterações e tempo entre iterações. */
        string primeiraLinha = arquivo.ReadLine()!;

        /* Separar os dados da primeira linha em um array de strings. */
        string[] dados = primeiraLinha.Split(';');

        /* Ler a quantidade de corpos, quantidade de iterações e tempo entre iterações do arquivo. */
        int quantidadeCorpos = int.Parse(dados[0]);
        int quantidadeIteracoes = int.Parse(dados[1]);
        double tempoEntreIteracoes = LerNumero(dados[2]);

        Universo universo = new Universo();

        /* Atribuir a quantidade de iterações e o tempo entre iterações ao objeto */
        universo.QuantidadeIteracoes = quantidadeIteracoes;
        universo.TempoEntreIteracoes = tempoEntreIteracoes;

        /* Ler os corpos do arquivo e adicioná-los ao universo. */
        for (int i = 0; i < quantidadeCorpos; i++)
        {
            string linhaCorpo = arquivo.ReadLine()!;

            string[] dadosCorpo = linhaCorpo.Split(';');

            /* Ler os dados do corpo do arquivo. */
            string nome = dadosCorpo[0];
            double massa = LerNumero(dadosCorpo[1]);
            double densidade = LerNumero(dadosCorpo[2]);
            double posX = LerNumero(dadosCorpo[3]);
            double posY = LerNumero(dadosCorpo[4]);
            double velX = LerNumero(dadosCorpo[5]);
            double velY = LerNumero(dadosCorpo[6]);

            /* Criar um novo corpo com os dados lidos do arquivo e adicioná-lo ao universo. */
            Corpo corpo = new Corpo(
                nome,
                massa,
                densidade,
                posX,
                posY,
                velX,
                velY
            );

            /* Adicionar o corpo ao universo. */
            universo.AdicionarCorpo(corpo);
        }

        return universo;
    }

    /* Lê tanto arquivos antigos com vírgula quanto arquivos invariantes da interface. */
    private static double LerNumero(string valor)
    {
        return double.Parse(valor.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}

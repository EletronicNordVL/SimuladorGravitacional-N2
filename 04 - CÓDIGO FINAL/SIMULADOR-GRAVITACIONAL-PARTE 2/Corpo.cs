using System;
namespace TrabalhoN2;

/* Essa classe representa um corpo no universo, com propriedades como 
 * nome, massa, densidade, posição, velocidade, força e aceleração. */
public class Corpo
{
    /* Atributo que define o máximo de densidade para evitar valores irrealistas. */
    private const double DensidadeMaxima = 1e18;

    /* Campos privados para armazenar os valores das propriedades do corpo. */
    private string nome = string.Empty;

    /* A massa do corpo, em quilogramas (kg) */
    private double massa;

    /* A densidade do corpo, em quilogramas por metro cúbico (kg/m³) */
    private double densidade;

    /* Propriedades públicas para acessar e modificar os valores do corpo. */
    public string Nome
    {
        /* Obtém o nome do corpo. */
        get => nome;
        /* Define o nome do corpo, validando se não é nulo, vazio ou contém apenas espaços em branco. */
        set
        {
            /* Valida se o nome fornecido é nulo, vazio ou contém apenas espaços em branco. */
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "O nome do corpo não pode ser vazio.");
            /* Se o nome for válido, atribui o valor ao campo privado "nome". */
            nome = value;
        }
    }

    /* É medida em quilogramas (kg) e representa a quantidade de matéria contida no corpo. */
    public double Massa
    {
        /* Obtém a massa do corpo. */
        get => massa;
        /* Define a massa do corpo, validando se é maior que zero. */
        set
        {
            /* A massa deve ser maior que zero, caso contrário, lança uma exceção. */
            if (value <= 0)
                throw new ArgumentException(
                    "A massa deve ser maior que zero.");
            /* Se a massa for válida, atribui o valor ao campo privado "massa". */
            massa = value;
            /* Sempre que a massa é alterada, o raio do corpo também precisa ser recalculado. */
            AtualizarRaio();
        }
    }

    /* Densidade do corpo, em quilogramas por metro cúbico (kg/m³) */
    public double Densidade
    {
        /* Obtém a densidade do corpo. */
        get => densidade;
        /* Define a densidade do corpo, validando se é maior que zero e não excede o limite máximo permitido. */
        set
        {
            /* A densidade deve ser maior que zero e não pode exceder o limite máximo definido. */
            if (value <= 0)
            {
                /* Se a densidade for menor ou igual a zero, lança uma exceção informando que 
                 * a densidade deve ser maior que zero. */
                throw new ArgumentException(
                    "A densidade deve ser maior que zero.");
            }

            /* Verifica se a densidade excede o limite máximo permitido. */
            if (value > DensidadeMaxima)
            {
                /* Se a densidade exceder o limite máximo, lança uma exceção informando que 
                 * a densidade excede o limite máximo permitido. */
                throw new ArgumentException(
                    "A densidade excede o limite máximo permitido.");
            }
            /* Se a densidade for válida, atribui o valor ao campo privado "densidade". */
            densidade = value;
            /* Sempre que a densidade é alterada, o raio do corpo também precisa ser recalculado. */
            AtualizarRaio();
        }
    }
    
    public double PosX { get; set; } /* Posição no eixo X, em metros (m) */
    public double PosY { get; set; } /* Posição no eixo Y, em metros (m) */
    public double VelX { get; set; } /* Velocidade no eixo X, em metros por segundo (m/s) */
    public double VelY { get; set; } /* Velocidade no eixo Y, em metros por segundo (m/s) */

    public double ForcaX { get; set; } /* Força resultante no eixo X, em newtons (N) */
    public double ForcaY { get; set; } /* Força resultante no eixo Y, em newtons (N) */

    public double AceleracaoX { get; set; } /* Aceleração no eixo X, em metros por segundo ao quadrado (m/s²) */
    public double AceleracaoY { get; set; } /* Aceleração no eixo Y, em metros por segundo ao quadrado (m/s²) */

    /* Raio do corpo, em metros (m). Depende só de massa e densidade, então é recalculado
     * apenas quando uma das duas muda (nos setters) e guardado neste campo. Assim, ler
     * Raio custa uma leitura de campo em vez de uma raiz cúbica a cada acesso. */
    private double raio;

    /* Propriedade pública para acessar o raio do corpo. */
    public double Raio => raio;

    /* Construtor da classe Corpo. */
    public Corpo(
        string nome, /* Nome do corpo */
        double massa, /* Massa do corpo, em quilogramas (kg) */
        double densidade, /* Densidade do corpo, em quilogramas por metro cúbico (kg/m³) */
        double posX, /* Posição inicial no eixo X, em metros (m) */
        double posY, /* Posição inicial no eixo Y, em metros (m) */
        double velX, /* Velocidade inicial no eixo X, em metros por segundo (m/s) */
        double velY) /* Velocidade inicial no eixo Y, em metros por segundo (m/s) */
    {
        /* Inicializa as propriedades do corpo com os valores fornecidos. */
        Nome = nome; /* Define o nome do corpo, validando se não é vazio ou nulo. */
        Massa = massa; /* Define a massa do corpo, validando se é maior que zero. */
        Densidade = densidade; /* Define a densidade do corpo, validando se é maior que zero e não excede o limite máximo. */
        PosX = posX; /* Define a posição inicial no eixo X. */
        PosY = posY; /* Define a posição inicial no eixo Y. */
        VelX = velX; /* Define a velocidade inicial no eixo X. */
        VelY = velY; /* Define a velocidade inicial no eixo Y. */
    }

    /* Recalcula o raio com base na massa e densidade e o guarda no campo "raio". */
    private void AtualizarRaio()
    {
        /* No construtor, a massa é definida antes da densidade. Enquanto uma das duas
         * ainda for zero não há raio a calcular (e isso evita dividir por zero). */
        if (massa <= 0 || densidade <= 0)
        {
            /* Se a massa ou densidade for menor ou igual a zero, não é possível calcular o raio. */
            return;
        }

        /* A partir da densidade (d = m/v), isolamos o volume: v = m/d */
        double volume = massa / densidade;

        /* Considerando que o corpo é uma esfera, a fórmula do volume é V = (4/3) * PI * r^3. Logo, se isolarmos o raio, temos a raiz cúbica de (3 * V) / (4 * PI) */
        raio = Math.Cbrt((3 * volume) / (4 * Math.PI));
    }
}
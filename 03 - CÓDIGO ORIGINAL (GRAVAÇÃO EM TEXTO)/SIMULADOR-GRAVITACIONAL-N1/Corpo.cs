using System;
namespace TrabalhoN1
{
    /* Essa classe representa um corpo no universo, com propriedades como 
     * nome, massa, densidade, posição, velocidade, força e aceleração. */
    public class Corpo
    {

        /* Define o máximo de densidade para evitar valores irrealistas. */
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
            get => nome;
            set
            {
                /* Valida se o nome fornecido é nulo, vazio ou contém apenas espaços em branco. */
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(
                        "O nome do corpo não pode ser vazio.");

                nome = value;
            }
        }

        /* É medida em quilogramas (kg) e representa a quantidade de matéria contida no corpo. */
        public double Massa
        {
            get => massa;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(
                        "A massa deve ser maior que zero.");

                massa = value;
            }
        }

        /* Densidade do corpo, em quilogramas por metro cúbico (kg/m³) */
        public double Densidade
        {
            get => densidade;
            set
            {
                /* A densidade deve ser maior que zero e não pode exceder o limite máximo definido. */
                if (value <= 0)
                {
                    throw new ArgumentException(
                        "A densidade deve ser maior que zero.");
                }

                /* Verifica se a densidade excede o limite máximo permitido. */
                if (value > DensidadeMaxima)
                {
                    throw new ArgumentException(
                        "A densidade excede o limite máximo permitido.");
                }

                densidade = value;
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

        /* O raio não é armazenado separadamente porque depende */
        /* diretamente da massa e da densidade do corpo. */
        public double Raio => CalcularRaio();

        /* Construtor da classe Corpo. */
        public Corpo(
            string nome,
            double massa,
            double densidade,
            double posX,
            double posY,
            double velX,
            double velY)
        {
            /* Inicializa as propriedades do corpo com os valores fornecidos. */
            Nome = nome;
            Massa = massa;
            Densidade = densidade;
            PosX = posX;
            PosY = posY;
            VelX = velX;
            VelY = velY;
        }

        /* Função privada que calcula o raio do corpo com base na massa e densidade. */
        private double CalcularRaio()
        {
            /* A partir da densidade (d = m/v), isolamos o volume: v = m/d */
            double volume = Massa / Densidade;

            /* Considerando que o corpo é uma esfera, a fórmula do volume é V = (4/3) * PI * r^3. Logo, se isolarmos o raio, temos a raiz cúbica de (3 * V) / (4 * PI) */
            return Math.Cbrt(
                (3 * volume) / (4 * Math.PI)
            );
        }
    }
}
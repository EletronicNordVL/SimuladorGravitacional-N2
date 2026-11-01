using System;
using System.Collections.Generic;
namespace TrabalhoN1;

/* Essa classe representa o universo, contendo uma lista de corpos e métodos para calcular forças, atualizar posições e tratar colisões. */
public class Universo
{
    /* Constante gravitacional utilizada no cálculo da força entre os corpos. */
    private const double G = 6.674184e-11;

    /* Lista de corpos presentes no universo. */
    public List<Corpo> Corpos { get; private set; }

    /* Quantidade de iterações que a simulação irá executar. */
    public int QuantidadeIteracoes { get; set; }

    /* Tempo entre cada iteração da simulação, em segundos. */
    public double TempoEntreIteracoes { get; set; }

    /* Construtor da classe Universo. */
    public Universo()
    {
        Corpos = new List<Corpo>();
    }

    /* Adiciona um corpo ao universo. */
    public void AdicionarCorpo(Corpo corpo)
    {
        Corpos.Add(corpo);
    }

    /* Calcula a distância entre dois corpos. */
    private double CalcularDistancia(Corpo corpo1, Corpo corpo2)
    {
        double deltaX = corpo2.PosX - corpo1.PosX;
        double deltaY = corpo2.PosY - corpo1.PosY;

        /* Utiliza o Teorema de Pitágoras para calcular a distância entre os corpos. */
        return Math.Sqrt(
            deltaX * deltaX +
            deltaY * deltaY
        );
    }

    /* Calcula a força gravitacional entre dois corpos. */
    private double CalcularForcaGravitacional(
        Corpo corpo1,
        Corpo corpo2)
    {
        double distancia = CalcularDistancia(corpo1, corpo2);

        if (distancia == 0)
        {
            return 0;
        }

        return G * corpo1.Massa * corpo2.Massa
               / (distancia * distancia);
    }

    /* Calcula a força gravitacional resultante sobre os corpos. */
    public void CalcularForcas()
    {
        /* Zera as forças da iteração anterior para não acumular com o novo ciclo. */
        foreach (Corpo corpo in Corpos)
        {
            corpo.ForcaX = 0;
            corpo.ForcaY = 0;
        }

        /* Percorre cada par de corpos uma única vez utilizando o (j = i + 1) para otimizar o processamento. */
        for (int i = 0; i < Corpos.Count; i++)
        {
            for (int j = i + 1; j < Corpos.Count; j++)
            {
                Corpo corpo1 = Corpos[i]; /* Corpo 1 é o corpo atual da iteração externa. */
                Corpo corpo2 = Corpos[j]; /* Corpo 2 é o corpo atual da iteração interna. */

                double deltaX = corpo2.PosX - corpo1.PosX; /* Calcula a diferença de posição no eixo X entre os corpos. */
                double deltaY = corpo2.PosY - corpo1.PosY; /* Calcula a diferença de posição no eixo Y entre os corpos. */

                /* Calcula a distância entre os corpos. */
                double distancia = CalcularDistancia(
                    corpo1,
                    corpo2
                );

                if (distancia == 0)
                {
                    continue;
                }

                /* Áqui será aplicada a Lei da Gravitação Universal de Isaac Newton que é o F = G * (m1 * m2) / r² */
                double forca = CalcularForcaGravitacional(
                    corpo1,
                    corpo2
                );

                /* Utilizaremos a decomposição vetorial da força resultante para os eixos X e Y onde utilizaremos os triângulos (seno e cosseno do ângulo) */
                double direcaoX = deltaX / distancia;
                double direcaoY = deltaY / distancia;

                double forcaX = forca * direcaoX;
                double forcaY = forca * direcaoY;

                /* Terceira Lei de Newton (Ação e Reação) onde a força que o corpo 1 exerce sobre o corpo 2 é igual em módulo e oposta em direção á força
                 * que o corpo 2 exerce sobre o corpo 1. */
                corpo1.ForcaX += forcaX;
                corpo1.ForcaY += forcaY;

                corpo2.ForcaX -= forcaX;
                corpo2.ForcaY -= forcaY;
            }
        }
    }

    /* Atualiza velocidade e posição dos corpos em cada iteração utilizando MRUV (Movimento Retilíneo Uniformemente Variado). */
    public void AtualizarPosicoes(double tempoEntreIteracoes)
    {
        foreach (Corpo corpo in Corpos)
        {

            /* Aplica a 2ª Lei de Newton (F = m * a) isolando a aceleração (a = F / m) */
            corpo.AceleracaoX = corpo.ForcaX / corpo.Massa;
            corpo.AceleracaoY = corpo.ForcaY / corpo.Massa;

            /* Equação horária da posição (s = s0 + v0*t + (a*t²)/2) */
            corpo.PosX +=
            corpo.VelX * tempoEntreIteracoes
            + (corpo.AceleracaoX / 2)
            * Math.Pow(tempoEntreIteracoes, 2);

            corpo.PosY +=
                corpo.VelY * tempoEntreIteracoes
                + (corpo.AceleracaoY / 2)
                * Math.Pow(tempoEntreIteracoes, 2);

            /* Equação da velocidade X (v = v0 + a*t) */
            corpo.VelX +=
                corpo.AceleracaoX * tempoEntreIteracoes;

            /* Equação da velocidade Y (v = v0 + a*t) */
            corpo.VelY +=
                corpo.AceleracaoY * tempoEntreIteracoes;
        }
    }

    /* Verifica colisões e recalcula as velocidades quando necessário. */
    public void TratarColisoes()
    {
        for (int i = 0; i < Corpos.Count; i++)
        {
            for (int j = i + 1; j < Corpos.Count; j++)
            {
                /* Obtém os corpos que estão sendo comparados na iteração atual. */
                Corpo corpo1 = Corpos[i];
                Corpo corpo2 = Corpos[j];

                /* Calcula a distância entre os centros dos corpos. */
                double distancia = CalcularDistancia(corpo1, corpo2);

                /* A soma dos raios define o limite para ocorrer uma colisão. */
                double somaRaios = corpo1.Raio + corpo2.Raio;

                if (distancia <= somaRaios)
                {
                    /* Colisão detectada entre os corpos. */
                    Console.WriteLine(
                        $"Colisão detectada entre {corpo1.Nome} e {corpo2.Nome}");

                    /* Calcula a normal da colisão: direção do corpo 1 para o corpo 2. */
                    double normalX;
                    double normalY;

                    if (distancia > 0)
                    {
                        /* Normaliza a direção da colisão para obter a normal. */
                        normalX = (corpo2.PosX - corpo1.PosX) / distancia;
                        normalY = (corpo2.PosY - corpo1.PosY) / distancia;
                    }
                    else
                    {
                        /* Quando os centros coincidem, usa a velocidade relativa para
                         * definir uma direção de separação e evita divisão por zero. */
                        double diferencaVelX = corpo2.VelX - corpo1.VelX;
                        double diferencaVelY = corpo2.VelY - corpo1.VelY;

                        /* Calcula o módulo da velocidade relativa para normalizar a direção. */
                        double moduloVelocidade = Math.Sqrt(
                            diferencaVelX * diferencaVelX +
                            diferencaVelY * diferencaVelY);

                        /* Normaliza a direção da velocidade relativa para obter a normal da colisão. */
                        if (moduloVelocidade > 0)
                        {
                            /* Normaliza a direção da velocidade relativa para obter a normal da colisão. */
                            normalX = diferencaVelX / moduloVelocidade;
                            normalY = diferencaVelY / moduloVelocidade;
                        }
                        else
                        {
                            /* Se a velocidade relativa também for zero, define uma normal arbitrária. */
                            normalX = 1;
                            normalY = 0;
                        }
                    }

                    /* Primeiro resolve fisicamente a sobreposição. E, mesmo quando os corpos já estão se afastando. */
                    double sobreposicao = somaRaios - distancia;

                    if (sobreposicao > 0)
                    {
                        /* Calcula a correção de posição para cada corpo, dividindo a sobreposição pela metade. */
                        double correcao = sobreposicao / 2.0;

                        /* Move os corpos para fora da sobreposição ao longo da normal da colisão. */
                        corpo1.PosX -= correcao * normalX;
                        corpo1.PosY -= correcao * normalY;

                        corpo2.PosX += correcao * normalX;
                        corpo2.PosY += correcao * normalY;
                    }

                    /* Velocidade relativa na direção da normal da colisão. */
                    double velocidadeRelativaX = corpo2.VelX - corpo1.VelX;
                    double velocidadeRelativaY = corpo2.VelY - corpo1.VelY;

                    /* Projeção da velocidade relativa na direção da normal. */
                    double velocidadeNaNormal =
                        velocidadeRelativaX * normalX +
                        velocidadeRelativaY * normalY;

                    /* Se for negativo, os corpos estão se aproximando. Nesse caso aplicamos o impulso da colisão elástica. Se for zero ou positivo, eles já 
                     * estão se afastando,então não aplicamos outro impulso. */
                    if (velocidadeNaNormal < 0)
                    {
                        /* Conservação da quantidade de movimento (Q = m * v).
                         * O cálculo considera uma colisão elástica. */
                        double impulso =
                            -(2 * velocidadeNaNormal) /
                            ((1 / corpo1.Massa) + (1 / corpo2.Massa));

                        double impulsoX = impulso * normalX;
                        double impulsoY = impulso * normalY;

                        /* Aplica o impulso nas componentes X e Y das velocidades. */
                        corpo1.VelX -= impulsoX / corpo1.Massa;
                        corpo1.VelY -= impulsoY / corpo1.Massa;

                        corpo2.VelX += impulsoX / corpo2.Massa;
                        corpo2.VelY += impulsoY / corpo2.Massa;
                    }
                }
            }
        }
    }

    /* Exibe o estado atual dos corpos. */
    public void ExibirEstado()
    {
        foreach (Corpo corpo in Corpos)
        {
            Console.WriteLine(
                $"{corpo.Nome} - " +
                $"Posição: ({corpo.PosX:F4}, {corpo.PosY:F4}) - " +
                $"Velocidade: ({corpo.VelX:F4}, {corpo.VelY:F4})");
        }
    }

    /* Executa as iterações da simulação. */
    public void ExecutarSimulacao(
        int iteracoes,
        double tempoEntreIteracoes)
    {
        for (int i = 1; i <= iteracoes; i++)
        {
            /* Calcula as forças gravitacionais entre os corpos. */
            CalcularForcas();

            AtualizarPosicoes(tempoEntreIteracoes);

            TratarColisoes();

            Console.WriteLine($"Iteração {i}");

            ExibirEstado();

            Console.WriteLine();
        }
    }

    /* Gera corpos aleatórios para a simulação. */
    public void GerarCorposAleatorios(int quantidade)
    {
        Random random = new Random();

        /* Gera corpos com propriedades aleatórias dentro de intervalos definidos. */
        for (int i = 0; i < quantidade; i++)
        {
            /* Gera um nome único para cada corpo. */
            string nome = $"Corpo {i + 1}";
            double massa = random.NextDouble() * (1e25 - 1e22) + 1e22;      /* 10^22 a 10^25 kg */
            double densidade = random.NextDouble() * (6000 - 3000) + 3000;  /* 3000 a 6000 kg/m³ */
            double posX = random.NextDouble() * (5e9 - (-5e9)) + (-5e9);    /* ±5 bilhões de metros */
            double posY = random.NextDouble() * (5e9 - (-5e9)) + (-5e9);    
            double velX = random.NextDouble() * (200 - (-200)) + (-200);    /* ±200 m/s */
            double velY = random.NextDouble() * (200 - (-200)) + (-200);

            /* Cria um novo corpo com as propriedades geradas aleatoriamente. */
            Corpo corpo = new Corpo(
                nome,
                massa,
                densidade,
                posX,
                posY,
                velX,
                velY
            );

            /* Adiciona o corpo gerado aleatoriamente ao universo. */
            AdicionarCorpo(corpo);
        }
    }
}
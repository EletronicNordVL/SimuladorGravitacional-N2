using SimuladorGravitacional_N2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks; 
namespace TrabalhoN2;

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

    /* Instância do gravador do MySQL */
    private GravadorUniversoBD gravador;

    /* Construtor da classe Universo. */
    public Universo()
    {
        Corpos = new List<Corpo>();
        gravador = new GravadorUniversoBD();
    }

    /* Adiciona um corpo ao universo. */
    public void AdicionarCorpo(Corpo corpo)
    {
        /* Adiciona o corpo à lista de corpos do universo. */
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
        /* Calcula a distância entre os corpos usando o método CalcularDistancia. */
        double distancia = CalcularDistancia(corpo1, corpo2);

        /* Evita divisão por zero caso os corpos estejam na mesma posição. */
        if (distancia == 0)
        {
            return 0;
        }

        /* Aplica a fórmula da força gravitacional: F = G * (m1 * m2) / r² */
        return G * corpo1.Massa * corpo2.Massa
               / (distancia * distancia);
    }

    /* Calcula a força gravitacional resultante sobre os corpos usando Parallel.For. */
    public void CalcularForcas()
    {
        /* Zera as forças resultantes antes de calcular as novas forças. */
        Parallel.For(0, Corpos.Count, i =>
        {
            /* Obtém o corpo atual da iteração. */
            Corpo corpo1 = Corpos[i];
            double acumuladorForcaX = 0;
            double acumuladorForcaY = 0;

            /* Calcula a força gravitacional entre o corpo atual e todos os outros corpos. */
            for (int j = 0; j < Corpos.Count; j++)
            {
                if (i == j) continue;

                Corpo corpo2 = Corpos[j];

                /* Calcula a diferença de posição entre os corpos para determinar a direção da força. */
                double deltaX = corpo2.PosX - corpo1.PosX;
                double deltaY = corpo2.PosY - corpo1.PosY;
                double distancia = CalcularDistancia(corpo1, corpo2);

                if (distancia == 0) continue;

                /* Calcula a força gravitacional entre os dois corpos e a projeta nas direções X e Y. */
                double forca = CalcularForcaGravitacional(corpo1, corpo2);
                double direcaoX = deltaX / distancia;
                double direcaoY = deltaY / distancia;

                /* Acumula as componentes da força resultante sobre o corpo atual. */
                acumuladorForcaX += forca * direcaoX;
                acumuladorForcaY += forca * direcaoY;
            }

            /* Atualiza as forças resultantes do corpo atual com os valores acumulados. */
            corpo1.ForcaX = acumuladorForcaX;
            corpo1.ForcaY = acumuladorForcaY;

           /* Atualiza as acelerações com base nas forças resultantes. */
           corpo1.AceleracaoX = corpo1.ForcaX / corpo1.Massa;
           corpo1.AceleracaoY = corpo1.ForcaY / corpo1.Massa;
    });

    }

    /* Atualiza posições e velocidades utilizando o método Velocity Verlet. */
    public void AtualizarPosicoes(double tempoEntreIteracoes)
    {
        /* Validar o intervalo de tempo antes de iniciar os cálculos. */
        if (tempoEntreIteracoes <= 0 ||
            !double.IsFinite(tempoEntreIteracoes))
        {
            throw new ArgumentException(
                "O intervalo de tempo deve ser positivo e finito.");
        }

        /* Armazenar as acelerações anteriores de cada corpo. */
        double[] aceleracoesAnterioresX = new double[Corpos.Count];
        double[] aceleracoesAnterioresY = new double[Corpos.Count];

        /* Atualizar as posições dos corpos. */
        Parallel.For(0, Corpos.Count, i =>
        {
            Corpo corpo = Corpos[i];

            /* Guardar as acelerações calculadas antes da atualização. */
            aceleracoesAnterioresX[i] = corpo.AceleracaoX;
            aceleracoesAnterioresY[i] = corpo.AceleracaoY;

            /* Atualizar a posição utilizando a aceleração anterior. */
            corpo.PosX += corpo.VelX * tempoEntreIteracoes
                + 0.5 * corpo.AceleracaoX
                * tempoEntreIteracoes * tempoEntreIteracoes;

            corpo.PosY += corpo.VelY * tempoEntreIteracoes
                + 0.5 * corpo.AceleracaoY
                * tempoEntreIteracoes * tempoEntreIteracoes;
        });

        /* Recalcular as forças nas novas posições. */
        CalcularForcas();

        /* Atualiza as velocidades usando a média
         * entre as acelerações anteriores e as novas acelerações. */
        Parallel.For(0, Corpos.Count, i =>
        {
            Corpo corpo = Corpos[i];

            corpo.VelX += 0.5 *
                (aceleracoesAnterioresX[i] + corpo.AceleracaoX)
                * tempoEntreIteracoes;

            corpo.VelY += 0.5 *
                (aceleracoesAnterioresY[i] + corpo.AceleracaoY)
                * tempoEntreIteracoes;
        });
    }

    /* Tratar colisões entre os corpos, ajustando posições e velocidades conforme necessário. */
    public void TratarColisoes()
    {
        /* O raio depende só de massa e densidade (não muda no laço), mas Raio é uma
         * propriedade calculada com raiz cúbica. Calculá-lo uma vez por corpo troca
         * n² raízes cúbicas por n. */
        double[] raios = new double[Corpos.Count];
        for (int k = 0; k < Corpos.Count; k++)
        {
            raios[k] = Corpos[k].Raio;
        }

        /* Percorre todos os pares de corpos para verificar colisões. */
        for (int i = 0; i < Corpos.Count; i++)
        {
            for (int j = i + 1; j < Corpos.Count; j++)
            {
                /* Obtém os corpos que estão sendo comparados na iteração atual. */
                Corpo corpo1 = Corpos[i];
                Corpo corpo2 = Corpos[j];

                /* A soma dos raios define o limite para ocorrer uma colisão. */
                double somaRaios = raios[i] + raios[j];

                /* Compara os QUADRADOS para evitar a raiz quadrada nos pares que não
                 * colidem (a maioria): d <= r equivale a d² <= r², pois ambos são positivos. */
                double deltaX = corpo2.PosX - corpo1.PosX;
                double deltaY = corpo2.PosY - corpo1.PosY;
                double distanciaQuadrada = deltaX * deltaX + deltaY * deltaY;

                if (distanciaQuadrada <= somaRaios * somaRaios)
                {
                    /* Só agora calcula a distância real, necessária para a normal. */
                    double distancia = Math.Sqrt(distanciaQuadrada);

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
            /* Exibe o nome, posição e velocidade de cada corpo com quatro casas decimais. */
            Console.WriteLine(
                $"{corpo.Nome} - " +
                $"Posição: ({corpo.PosX:F4}, {corpo.PosY:F4}) - " +
                $"Velocidade: ({corpo.VelX:F4}, {corpo.VelY:F4})");
        }
    }

    /* Método que executa as iterações da simulação, mede o tempo de cada fase e grava no MySQL. */
    public void ExecutarSimulacao(
        int iteracoes,
        double tempoEntreIteracoes,
        string? nomeSimulacao = null)
    {
        /* Configura a quantidade de iterações e o tempo entre elas. */
        this.QuantidadeIteracoes = iteracoes;
        this.TempoEntreIteracoes = tempoEntreIteracoes;

        /* Nome padrão com data e hora, para distinguir as simulações na listagem do replay. */
        nomeSimulacao ??= $"Simulação {DateTime.Now:dd/MM/yyyy HH:mm:ss}";

        /* Cronômetro total: começa ANTES de iniciar o registro no banco, porque criar
         * a simulação também conta como tempo de gravação. */
        Stopwatch cronometro = Stopwatch.StartNew();

        /* Cronômetros por fase: acumulam o tempo de todas as iterações. */
        Stopwatch tempoForcas = new Stopwatch();
        Stopwatch tempoPosicoes = new Stopwatch();
        Stopwatch tempoColisoes = new Stopwatch();

        /* Bloco de Try -Finally para garantir que o gravador finalize a simulação e 
         * feche a conexão com o banco, mesmo se ocorrer algum erro durante a execução. */
        try
        {
            /* Iniciar o registro no Banco de Dados MySQL */
            gravador.IniciarSimulacao(nomeSimulacao, Corpos, iteracoes, tempoEntreIteracoes);

            /* Gravar o estado inicial, antes de qualquer iteração. */
            gravador.GravarPasso(0, 0, Corpos);

            /* Variável para acumular o tempo decorrido da simulação. */
            double tempoDecorrido = 0;

            /* Calcular as forças iniciais antes da primeira iteração.
             * O Velocity Verlet precisa das acelerações iniciais. */
            tempoForcas.Start();
            CalcularForcas();
            tempoForcas.Stop();

            /* Executar as iterações da simulação utilizando Velocity Verlet. */
            for (int i = 1; i <= iteracoes; i++)
            {
                /* Atualizar o tempo decorrido com base no tempo entre iterações. */
                tempoDecorrido += tempoEntreIteracoes;

                /* Atualizar as posições e velocidades utilizando Velocity Verlet.
                 * O método também recalcula as forças nas novas posições. */
                tempoPosicoes.Start();
                AtualizarPosicoes(tempoEntreIteracoes);
                tempoPosicoes.Stop();

                /* Tratar possíveis colisões entre os corpos. */
                tempoColisoes.Start();
                TratarColisoes();
                tempoColisoes.Stop();

                /* Recalcular as forças após o tratamento das colisões,
                 * garantindo que estejam atualizadas para a próxima iteração. */
                tempoForcas.Start();
                CalcularForcas();
                tempoForcas.Stop();

                /* Gravar o histórico do passo no MySQL. */
                gravador.GravarPasso(i, tempoDecorrido, Corpos);
            }
        }
        /* Finally garante que o gravador finalize a simulação e 
         * feche a conexão com o banco, mesmo se ocorrer algum erro durante a execução. */
        finally
        {
            /* Grava o último bloco pendente e fecha a conexão, mesmo se der erro no meio. */
            gravador.FinalizarSimulacao();
            cronometro.Stop();
        }

        /* Calcula o tempo total gasto nas fases de cálculo de forças, atualização de posições e tratamento de colisões. */
        TimeSpan calculo = tempoForcas.Elapsed + tempoPosicoes.Elapsed + tempoColisoes.Elapsed;

        /* Exibe o tempo total da simulação e o tempo gasto em cada fase (cálculo de forças, 
         * atualização de posições, tratamento de colisões e gravação no banco). */
        Console.WriteLine();
        Console.WriteLine($"[DESEMPENHO N2] Tempo total: {cronometro.Elapsed.TotalMilliseconds:F2} ms ({cronometro.Elapsed.TotalSeconds:F2} s)");
        Console.WriteLine($"  Cálculo (forças + posições + colisões): {calculo.TotalMilliseconds:F2} ms");
        Console.WriteLine($"    Forças   : {tempoForcas.Elapsed.TotalMilliseconds:F2} ms");
        Console.WriteLine($"    Posições : {tempoPosicoes.Elapsed.TotalMilliseconds:F2} ms");
        Console.WriteLine($"    Colisões : {tempoColisoes.Elapsed.TotalMilliseconds:F2} ms");
        Console.WriteLine($"  Gravação no banco: {gravador.TempoTotalGravacao.TotalMilliseconds:F2} ms");
        Console.WriteLine();
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
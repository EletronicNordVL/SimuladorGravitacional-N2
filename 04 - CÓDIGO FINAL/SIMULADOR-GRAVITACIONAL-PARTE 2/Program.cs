using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using SimuladorGravitacional_N2;
using TrabalhoN2;

/* Programa principal que apresenta um menu para o usuário criar, carregar ou apagar universos gravacionais 2D. */
bool continuar = true;

/* Loop principal que mantém o programa em execução até que o usuário escolha sair. */
while (continuar)
{
    /* Apresentar o menu de opções para o usuário, permitindo criar um novo universo ou carregar um universo salvo. */
    Console.WriteLine("SIMULADOR GRAVITACIONAL 2D");
    Console.WriteLine();

    /* Exibir as opções do menu. */
    Console.WriteLine("1 - Criar novo universo");
    Console.WriteLine("2 - Carregar universo salvo");
    Console.WriteLine("3 - Selecionar e apagar uma simulação salva");
    Console.WriteLine("4 - Apagar todas as simulações salvas");
    Console.WriteLine("0 - Sair do menu");

    /* Solicitar ao usuário que escolha uma opção do menu. */
    Console.WriteLine();
    Console.Write("Escolha uma opção: ");

    /* Ler a entrada do usuário e armazenar a opção escolhida. */
    string? opcao = Console.ReadLine();

    /* Limpar a tela após o usuário escolher uma opção do menu. */
    Console.Clear();

    /* Controlar se deve aguardar ENTER antes de voltar ao menu. */
    bool aguardarEnter = true;

    /* Tentar executar a operação escolhida pelo usuário, capturando 
     * qualquer exceção que possa ocorrer durante a execução. */
    try
    {
        switch (opcao)
        {
            /* Criar um novo universo com base na quantidade de corpos, iterações e tempo entre iterações informados pelo usuário. */
            case "1":
                Console.WriteLine("Criar novo universo");
                Console.WriteLine();

                /* Solicitar ao usuário que informe a quantidade de corpos que deseja gerar no universo. */
                Console.Write("Digite a quantidade de corpos: ");

                /* Ler a entrada do usuário e armazenar a quantidade de corpos informada. */
                int quantidadeCorpos;
                string? entradaCorpos = Console.ReadLine();

                /* Verificar se a quantidade de corpos informada é válida. */
                if (!int.TryParse(entradaCorpos, out quantidadeCorpos)
                    || quantidadeCorpos <= 0)
                {
                    /* Caso a quantidade de corpos informada seja inválida, solicitar ao usuário que informe um valor válido ou 0 para voltar ao menu principal. */
                    while (true)
                    {
                        Console.Write(
                            "Valor inválido. Digite um número válido ou 0 para voltar: "
                        );

                        /* Ler a entrada do usuário novamente. */
                        entradaCorpos = Console.ReadLine();

                        /* Voltar ao menu principal caso o usuário escolha a opção 0 após informar um valor inválido. */
                        if (entradaCorpos == "0")
                        {
                            quantidadeCorpos = 0;
                            break;
                        }

                        /* Verificar se a quantidade de corpos informada é válida. */
                        if (int.TryParse(
                                entradaCorpos,
                                out quantidadeCorpos)
                            && quantidadeCorpos > 0)
                        {
                            break;
                        }
                    }
                }

                /* Voltar ao menu principal caso o usuário escolha a opção 0 após informar um valor inválido. */
                if (quantidadeCorpos == 0)
                {
                    /* Não aguardar ENTER antes de voltar ao menu principal. */
                    aguardarEnter = false;
                    Console.Clear();
                    break;
                }

                /* Criar uma instância do universo com base na quantidade de corpos informada pelo usuário. */
                Universo universo = new Universo();

                /* Gerar corpos aleatórios com base na quantidade informada pelo usuário. */
                universo.GerarCorposAleatorios(quantidadeCorpos);

                /* Solicitar ao usuário que informe a quantidade de iterações que deseja executar na simulação do universo. */
                Console.Write("Digite a quantidade de iterações: ");

                /* Inicializar a variável que armazenará a quantidade de iterações. */
                int quantidadeIteracoes;
                string? entradaIteracoes = Console.ReadLine();

                /* Verificar se a quantidade de iterações informada é válida. */
                if (!int.TryParse(
                        entradaIteracoes,
                        out quantidadeIteracoes)
                    || quantidadeIteracoes <= 0)
                {
                    /* Caso a quantidade de iterações informada seja inválida, 
                     * solicitar ao usuário que informe um valor válido ou 0 para voltar ao menu principal. */
                    while (true)
                    {
                        /* Solicitar ao usuário que informe uma quantidade de iterações válida ou 0 para voltar ao menu principal. */
                        Console.Write(
                            "Valor inválido. Digite um número válido ou 0 para voltar: "
                        );

                        /* Ler a entrada do usuário novamente. */
                        entradaIteracoes = Console.ReadLine();

                        /* Voltar ao menu principal caso o usuário escolha a opção 0 após informar um valor inválido. */
                        if (entradaIteracoes == "0")
                        {
                            quantidadeIteracoes = 0;
                            break;
                        }

                        /* Verificar se a quantidade de iterações informada é válida. */
                        if (int.TryParse(
                                entradaIteracoes,
                                out quantidadeIteracoes)
                            && quantidadeIteracoes > 0)
                        {
                            break;
                        }
                    }
                }

                /* Voltar ao menu principal caso o usuário escolha a opção 0 após informar um valor inválido. */
                if (quantidadeIteracoes == 0)
                {
                    /*  Não aguardar ENTER antes de voltar ao menu principal. */
                    aguardarEnter = false;
                    Console.Clear();
                    break;
                }

                /* Atribuir a quantidade de iterações informada pelo usuário ao universo criado. */
                universo.QuantidadeIteracoes =
                    quantidadeIteracoes;

                /* Solicitar ao usuário que informe o tempo entre as iterações do universo. */
                Console.Write(
                    "Digite o tempo entre as iterações (em segundos): "
                );

                /* Inicializar a variável que armazenará o tempo entre as iterações. */
                double tempoEntreIteracoes = 0;

                /* Ler a entrada do usuário e substituir vírgulas por pontos para permitir a entrada de números decimais. */
                string? entradaTempo =
                    Console.ReadLine()?.Replace(',', '.');

                /* Verificar se o tempo entre as iterações informado é válido. */
                bool tempoValido =
                    double.TryParse(
                        entradaTempo,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out tempoEntreIteracoes
                    )
                    /* Verificar se o tempo informado é maior que zero. */
                    && tempoEntreIteracoes > 0;

                /* Caso o tempo informado seja inválido, solicitar ao usuário que informe um valor válido ou 0 para voltar ao menu principal. */
                if (!tempoValido)
                {
                    while (true)
                    {
                        /* Solicitar ao usuário que informe um tempo válido ou 0 para voltar ao menu principal. */
                        Console.Write(
                            "Valor inválido. Digite um tempo válido ou 0 para voltar: "
                        );

                        /* Ler a entrada do usuário e substituir vírgulas por pontos para permitir a entrada de números decimais. */
                        entradaTempo =
                            Console.ReadLine()?.Replace(',', '.');

                        /* Voltar ao menu principal caso o usuário escolha a opção 0 após informar um valor inválido. */
                        if (entradaTempo == "0")
                        {
                            tempoEntreIteracoes = 0;
                            break;
                        }

                        /* Verificar se o tempo entre as iterações informado é válido. */
                        tempoValido =
                            double.TryParse(
                                entradaTempo,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out tempoEntreIteracoes
                            )
                            && tempoEntreIteracoes > 0;

                        /* Caso o tempo informado seja válido, sair do loop. */
                        if (tempoValido)
                        {
                            break;
                        }
                    }
                }

                /* Voltar ao menu principal caso o usuário escolha a opção 0 após informar um valor inválido. */
                if (tempoEntreIteracoes == 0)
                {
                    /*  Não aguardar ENTER antes de voltar ao menu principal. */
                    aguardarEnter = false;
                    Console.Clear();
                    break;
                }

                /* Atribuir o tempo entre as iterações informado pelo usuário ao universo criado. */
                universo.TempoEntreIteracoes =
                    tempoEntreIteracoes;

                /* Exibir uma mensagem informando que o universo foi criado com sucesso. */
                Console.WriteLine();
                Console.WriteLine("Corpos gerados:");
                Console.WriteLine();

                /* Exibir o estado inicial do universo criado, mostrando os corpos gerados e suas propriedades. */
                universo.ExibirEstado();

                /* Iniciar a simulação gravitacional com persistência automática no MySQL */
                Console.WriteLine();
                Console.WriteLine("Iniciando a simulação e salvando no MySQL...");
                Console.WriteLine();

                /* Executa a física paralelizada (Parallel.For) e grava no banco de dados */
                universo.ExecutarSimulacao(
                   universo.QuantidadeIteracoes,
                   universo.TempoEntreIteracoes,
                   $"Simulação Gravitacional ({quantidadeCorpos} corpos)"
                );

                break;

            /* Carregar uma simulação salva do banco de dados e exibir os passos da simulação para o usuário. */
            case "2":

                /* Exibir mensagem informando que o usuário está selecionando uma simulação para replay. */
                Console.WriteLine("Selecionar simulação para replay");
                Console.WriteLine();

                /* Criar uma instância do GravadorUniversoBD para interagir com o banco de dados. */
                GravadorUniversoBD gravadorBD =
                    new GravadorUniversoBD();

                /* Listar todas as simulações salvas no banco de dados. */
                List<GravadorUniversoBD.ResumoSimulacao> simulacoesSalvas =
                    gravadorBD.ListarSimulacoes();

                /* Verificar se há simulações salvas no banco de dados. 
                 * Caso não haja, exibir uma mensagem informando que nenhuma simulação foi encontrada. */
                if (simulacoesSalvas.Count == 0)
                {
                    /* Exibir mensagem informando que nenhuma simulação foi encontrada no banco de dados. */
                    Console.WriteLine(
                        "Nenhuma simulação foi encontrada no banco de dados.");
                    break;
                }

                /* Exibir a lista de simulações salvas, mostrando o nome, ID, 
                 * quantidade de corpos, quantidade de iterações e data de criação. */
                for (int i = 0; i < simulacoesSalvas.Count; i++)
                {
                    /* Obter a simulação atual da lista de simulações salvas. */
                    GravadorUniversoBD.ResumoSimulacao simulacao =
                        simulacoesSalvas[i];

                    /* Exibir as informações da simulação atual, 
                     * incluindo o nome, ID, quantidade de corpos, quantidade de iterações e data de criação. */
                    Console.WriteLine(
                        $"{i + 1} - {simulacao.Nome} " +
                        $"(ID: {simulacao.Id}, corpos: {simulacao.QuantidadeCorpos}, " +
                        $"iterações: {simulacao.QuantidadeIteracoes}, " +
                        $"criada em: {simulacao.DataCriacao:dd/MM/yyyy HH:mm})");
                }

                /* Exibir a opção para voltar ao menu principal. */
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha uma simulação: ");

                /* Ler a escolha do usuário e validar se é um número válido. */
                int escolhaSimulacao;

                /* Continuar solicitando ao usuário que digite um número válido até que ele forneça uma entrada correta. */
                while (!int.TryParse(
                    Console.ReadLine(),
                    out escolhaSimulacao))
                {
                    Console.Write("Digite um número válido: ");
                }

                /* Verificar se o usuário escolheu voltar ao menu principal. */
                if (escolhaSimulacao == 0)
                {
                    /* Caso o usuário escolha voltar ao menu principal, 
                     * definir a variável aguardarEnter como false para não aguardar ENTER antes de voltar ao menu. */
                    aguardarEnter = false;
                    Console.Clear();
                    break;
                }

                /* Verificar se a escolha do usuário é válida, ou seja, se está dentro do intervalo das simulações salvas. */
                if (escolhaSimulacao < 1 ||
                    escolhaSimulacao > simulacoesSalvas.Count)
                {
                    /* Caso a escolha do usuário seja inválida, exibir uma mensagem informando que a opção é inválida. */
                    Console.WriteLine("Opção inválida.");
                    break;
                }

                /* Obter a simulação selecionada pelo usuário com base na escolha feita. */
                GravadorUniversoBD.ResumoSimulacao simulacaoSelecionada =
                    simulacoesSalvas[escolhaSimulacao - 1];

                /* Perguntar se o replay avança sozinho ou passo a passo (ENTER). */
                Console.Write("Reprodução automática? (S = automática, N = passo a passo): ");
                bool replayAutomatico =
                    (Console.ReadLine() ?? string.Empty).Trim().ToUpper() == "S";

                /* Exibir mensagem informando que o replay da simulação selecionada está sendo iniciado. */
                for (int passo = 1;
                     passo <= simulacaoSelecionada.QuantidadeIteracoes;
                     passo++)
                {
                    /* Carregar os corpos do passo atual da simulação selecionada do banco de dados. */
                    List<Corpo> corposPasso =
                        gravadorBD.CarregarPasso(
                            simulacaoSelecionada.Id,
                            passo);

                    /*  Verificar se não foram encontrados corpos para o passo atual da simulação. 
                     * Caso não haja corpos, exibir uma mensagem informando que não foram encontrados corpos para o passo e encerrar o loop. */
                    if (corposPasso.Count == 0)
                    {
                        /* Exibir mensagem informando que não foram encontrados corpos para o passo atual da simulação. */
                        Console.WriteLine(
                            $"Não foram encontrados corpos para o passo {passo}.");
                        break;
                    }

                    Console.Clear();

                    /* Exibir informações sobre a simulação selecionada, incluindo o nome da simulação e o passo atual em relação à quantidade total de iterações. */
                    Console.WriteLine(
                        $"Replay: {simulacaoSelecionada.Nome}");
                    Console.WriteLine(
                        $"Passo {passo} de " +
                        $"{simulacaoSelecionada.QuantidadeIteracoes}");
                    Console.WriteLine();

                    /* Exibir as informações de cada corpo no passo atual da simulação, 
                     * incluindo o nome do corpo, posição e velocidade. */
                    foreach (Corpo corpo in corposPasso)
                    {
                        Console.WriteLine(
                            $"{corpo.Nome} - " +
                            $"Posição: ({corpo.PosX:F4}, {corpo.PosY:F4}) - " +
                            $"Velocidade: ({corpo.VelX:F4}, {corpo.VelY:F4})");
                    }

                    /* Verificar se o replay deve avançar automaticamente ou passo a passo. 
                     * Caso seja automático, aguardar um curto período de tempo entre os passos e permitir que o usuário interrompa o replay pressionando ESC. 
                     * Caso seja passo a passo, aguardar o usuário pressionar ENTER para mostrar o próximo passo. */
                    if (replayAutomatico)
                    {
                        /* Pausa curta entre os passos; ESC interrompe o replay. */
                        Console.WriteLine();
                        Console.WriteLine("Pressione ESC para interromper.");
                        Thread.Sleep(300);

                        /* Verificar se o usuário pressionou a tecla ESC para interromper o replay. */
                        if (Console.KeyAvailable &&
                            Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            /* Exibir mensagem informando que o replay foi interrompido e encerrar o loop. */
                            Console.WriteLine("Replay interrompido.");
                            break;
                        }
                    }
                    /* Caso o replay seja passo a passo, aguardar o usuário pressionar ENTER para mostrar o próximo passo. */
                    else if (passo < simulacaoSelecionada.QuantidadeIteracoes)
                    {
                        /* Exibir uma linha em branco para separar a exibição das mensagens do prompt de entrada. */
                        Console.WriteLine();
                        Console.WriteLine(
                            "Pressione ENTER para mostrar o próximo passo.");
                        Console.ReadLine();
                    }
                }

                break;

            /* Selecionar e apagar uma simulação salva do banco de dados. */
            case "3":

                /* Exibir mensagem informando que o usuário está selecionando uma simulação para apagar. */
                Console.WriteLine("Selecionar e apagar simulação");
                Console.WriteLine();

                /* Criar uma instância do GravadorUniversoBD para interagir com o banco de dados. */
                GravadorUniversoBD gravadorExcluirBD =
                    new GravadorUniversoBD();

                /* Listar todas as simulações salvas no banco de dados para que o usuário possa escolher qual deseja apagar. */
                List<GravadorUniversoBD.ResumoSimulacao> simulacoesParaExcluir =
                    gravadorExcluirBD.ListarSimulacoes();

                /* Verificar se há simulações salvas no banco de dados. 
                 * Caso não haja, exibir uma mensagem informando que nenhuma simulação foi encontrada. */
                if (simulacoesParaExcluir.Count == 0)
                {
                    Console.WriteLine(
                        "Nenhuma simulação foi encontrada no banco de dados.");
                    break;
                }

                /* Exibir a lista de simulações salvas, mostrando o nome, ID 
                 * e data de criação para que o usuário possa escolher qual deseja apagar. */
                for (int i = 0; i < simulacoesParaExcluir.Count; i++)
                {
                    /*  Obter a simulação atual da lista de simulações salvas para exclusão. */
                    GravadorUniversoBD.ResumoSimulacao simulacao =
                        simulacoesParaExcluir[i];

                    Console.WriteLine(
                        $"{i + 1} - {simulacao.Nome} " +
                        $"(ID: {simulacao.Id}, criada em: " +
                        $"{simulacao.DataCriacao:dd/MM/yyyy HH:mm})");
                }

                /* Exibir a opção para voltar ao menu principal. */
                Console.WriteLine("0 - Voltar");
                Console.WriteLine();
                Console.Write("Escolha uma simulação para apagar: ");

                /* Ler a escolha do usuário e validar se é um número válido. */
                int escolhaApagar;

                /* Continuar solicitando ao usuário que digite um número válido até que ele forneça uma entrada correta. */
                while (!int.TryParse(
                    Console.ReadLine(),
                    out escolhaApagar))
                {
                    Console.Write("Digite um número válido: ");
                }

                /* Verificar se o usuário escolheu voltar ao menu principal. */
                if (escolhaApagar == 0)
                {
                    /*  Caso o usuário escolha voltar ao menu principal, 
                     *  definir a variável aguardarEnter como false para não aguardar ENTER antes de voltar ao menu. */
                    aguardarEnter = false;
                    Console.Clear();
                    break;
                }

                /* Verificar se a escolha do usuário é válida, ou seja, se está dentro do intervalo das simulações salvas. */
                if (escolhaApagar < 1 ||
                    escolhaApagar > simulacoesParaExcluir.Count)
                {
                    Console.WriteLine("Opção inválida.");
                    break;
                }

                /* Obter a simulação selecionada pelo usuário para exclusão com base na escolha feita. */
                GravadorUniversoBD.ResumoSimulacao simulacaoParaExcluir =
                    simulacoesParaExcluir[escolhaApagar - 1];

                /* Solicitar ao usuário que confirme a exclusão da simulação selecionada, 
                 * exibindo o nome da simulação para que ele possa confirmar ou cancelar a operação. */
                Console.Write(
                    $"Confirma a exclusão de '{simulacaoParaExcluir.Nome}'? (S/N): ");

                /* Ler a entrada do usuário para confirmar ou cancelar a exclusão da simulação. */
                string? confirmacaoExclusao = Console.ReadLine();

                /* Verificar se o usuário confirmou a exclusão da simulação. 
                 * Caso o usuário não confirme, exibir uma mensagem informando que a exclusão foi cancelada. */
                if (confirmacaoExclusao == null ||
                    confirmacaoExclusao.Trim().ToUpper() != "S")
                {
                    Console.WriteLine("Exclusão cancelada.");
                    break;
                }

                /* Chamar o método ExcluirSimulacao do GravadorUniversoBD para apagar a simulação selecionada do banco de dados. 
                 * O método retorna true se a exclusão foi bem-sucedida, ou false caso contrário. */
                bool simulacaoExcluida =
                    gravadorExcluirBD.ExcluirSimulacao(
                        simulacaoParaExcluir.Id);

                /* Exibir uma mensagem informando se a simulação foi apagada com sucesso ou se não foi encontrada no banco de dados. */
                if (simulacaoExcluida)
                {
                    /* Exibir mensagem informando que a simulação e os dados associados foram apagados com sucesso. */
                    Console.WriteLine(
                        "Simulação e dados associados apagados com sucesso.");
                }
                else
                {
                    /* Exibir mensagem informando que a simulação não foi encontrada no banco de dados. */
                    Console.WriteLine(
                        "A simulação não foi encontrada no banco.");
                }

                break;

            /* Apagar todas as simulações salvas no banco de dados. */
            case "4":

                /* Exibir mensagem informando que o usuário está prestes a apagar todas as simulações salvas no banco de dados. */
                Console.WriteLine("Apagar todas as simulações");
                Console.WriteLine();

                /* Criar uma instância do GravadorUniversoBD para interagir com o banco de dados. */
                GravadorUniversoBD gravadorApagarTodasBD =
                    new GravadorUniversoBD();

                /* Listar todas as simulações salvas no banco de dados para que o usuário possa confirmar a exclusão de todas elas. */
                List<GravadorUniversoBD.ResumoSimulacao> simulacoesParaApagar =
                    gravadorApagarTodasBD.ListarSimulacoes();

                /* Verificar se há simulações salvas no banco de dados. 
                 * Caso não haja, exibir uma mensagem informando que nenhuma simulação foi encontrada. */
                if (simulacoesParaApagar.Count == 0)
                {
                    Console.WriteLine(
                        "Nenhuma simulação foi encontrada no banco de dados.");
                    break;
                }

                /* Solicitar ao usuário que confirme a exclusão de todas as simulações salvas, 
                 * exibindo a quantidade de simulações que serão apagadas para que ele possa confirmar ou cancelar a operação. */
                Console.WriteLine(
                    $"Isso vai apagar {simulacoesParaApagar.Count} simulação(ões) " +
                    "e os dados associados.");
                Console.Write("Tem certeza? (S/N): ");

                string? confirmacaoApagarTodas = Console.ReadLine();

                /* Verificar se o usuário confirmou a exclusão de todas as simulações. 
                 * Caso o usuário não confirme, exibir uma mensagem informando que a operação foi cancelada. */
                if (confirmacaoApagarTodas == null ||
                    confirmacaoApagarTodas.Trim().ToUpper() != "S")
                {
                    Console.WriteLine("Operação cancelada.");
                    break;
                }

                /* Chamar o método ExcluirTodasSimulacoes do GravadorUniversoBD para apagar todas as simulações salvas no banco de dados. 
                 * O método retorna a quantidade de simulações apagadas. */
                int quantidadeApagada =
                    gravadorApagarTodasBD.ExcluirTodasSimulacoes();

                /* Exibir uma mensagem informando a quantidade de simulações apagadas com sucesso. */
                Console.WriteLine(
                    $"{quantidadeApagada} simulação(ões) apagada(s) com sucesso.");

                break;
        }

        /* Aguardar o usuário pressionar ENTER antes de voltar ao menu principal, caso a operação tenha sido concluída com sucesso. */
        if (continuar && aguardarEnter)
        {
            /* Exibir uma linha em branco para separar a exibição das mensagens do prompt de entrada. */
            Console.WriteLine();
            Console.WriteLine(
                "Pressione ENTER para voltar ao menu."
            );

            /* Aguardar o usuário pressionar ENTER antes de voltar ao menu principal. */
            Console.ReadLine();
            Console.Clear();
        }

    }
    /* Capturar qualquer exceção que ocorra durante a execução das operações do menu 
     * e exibir uma mensagem de erro para o usuário. */
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine($"Não foi possível concluir a operação: {ex.Message}");
        Console.WriteLine(
            "Verifique se o MySQL está em execução e se a variável de ambiente " +
            "SIMULADOR_DB_CONNECTION está configurada.");
    }
}
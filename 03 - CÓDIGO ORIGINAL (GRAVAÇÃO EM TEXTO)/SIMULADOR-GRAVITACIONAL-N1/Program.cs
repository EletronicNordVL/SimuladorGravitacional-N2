using System;
using System.Globalization;
using System.IO;
using TrabalhoN1;

/* Programa principal que apresenta um menu para o usuário criar, carregar ou apagar universos gravacionais 2D. */
bool continuar = true;

while (continuar)
{
    /* Apresentar o menu de opções para o usuário, permitindo criar um novo universo ou carregar um universo salvo. */
    Console.WriteLine("SIMULADOR GRAVITACIONAL 2D");
    Console.WriteLine();

    /* Exibir as opções do menu. */
    Console.WriteLine("1 - Criar novo universo");
    Console.WriteLine("2 - Carregar universo salvo");
    Console.WriteLine("3 - Selecionar e apagar algum universo salvo");
    Console.WriteLine("4 - Apagar todos os universos salvos");
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

    /* Executar a opção escolhida pelo usuário. */
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
                aguardarEnter = false;
                Console.Clear();
                break;
            }

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
                while (true)
                {
                    Console.Write(
                        "Valor inválido. Digite um número válido ou 0 para voltar: "
                    );

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
                && tempoEntreIteracoes > 0;

            /* Caso o tempo informado seja inválido, solicitar ao usuário que informe um valor válido ou 0 para voltar ao menu principal. */
            if (!tempoValido)
            {
                while (true)
                {
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

            /* Criar um gravador de arquivo de texto para salvar o universo criado em um arquivo txt. */
            GravadorArquivoTexto gravador =
                new GravadorArquivoTexto();

            /* Gerar um nome de arquivo diferente para cada universo salvo, evitando sobrescrever os anteriores. */
            int numeroArquivo = 1;
            string caminhoArquivo;
            do
            /* Gerar o nome do arquivo com base no número do arquivo, incrementando o número a cada iteração. */
            {
                caminhoArquivo =
                    $"universo_{numeroArquivo}.txt";

                numeroArquivo++;
            }
            /* Verificar se o arquivo já existe, caso exista, gerar um novo nome de arquivo. */
            while (File.Exists(caminhoArquivo));

            /* Salvar o universo criado em um arquivo txt com o nome gerado. */
            gravador.Salvar(
                universo,
                caminhoArquivo
            );

            Console.WriteLine();

            /* Exibir uma mensagem informando que a configuração inicial do universo foi salva com sucesso. */
            Console.WriteLine(
                $"Configuração inicial salva em {caminhoArquivo}."
            );

            /* Serve para aguardar o usuário pressionar ENTER antes de iniciar a simulação. */
            Console.WriteLine();
            Console.WriteLine("Iniciando simulação...");
            Console.WriteLine();

            /* Executar a simulação do universo com base na quantidade de iterações e tempo entre iterações informados pelo usuário. */
            universo.ExecutarSimulacao(
                universo.QuantidadeIteracoes,
                universo.TempoEntreIteracoes
            );

            break;

        /* Carregar um universo salvo de um arquivo txt e continuar a simulação. */
        case "2":

            /* Exibir uma mensagem informando que o usuário escolheu carregar um universo salvo. */
            Console.WriteLine("Carregar universo salvo");
            Console.WriteLine();

            /* Buscar todos os arquivos de universos salvos. */
            string[] universosSalvos =
                Directory.GetFiles(
                    ".",
                    "universo_*.txt"
                );

            /* Verificar se existem universos salvos. */
            if (universosSalvos.Length == 0)
            {
                Console.WriteLine(
                    "Nenhum universo salvo encontrado."
                );

                break;
            }

            /* Exibir os universos salvos para o usuário escolher. */
            for (int i = 0; i < universosSalvos.Length; i++)
            {
                /* Exibir o número do universo e o nome do arquivo correspondente. */
                Console.WriteLine(
                    $"{i + 1} - {Path.GetFileName(universosSalvos[i])}"
                );
            }

            Console.WriteLine("0 - Voltar");

            /* Solicitar ao usuário que escolha um universo para carregar. */
            Console.WriteLine();
            Console.Write("Escolha um universo: ");

            /* Inicializar a variável que armazenará a escolha do usuário. */
            int escolhaUniverso;

            /* Verificar se a entrada do usuário é um número válido, caso contrário, solicitar novamente. */
            while (!int.TryParse(
                       Console.ReadLine(),
                       out escolhaUniverso))
            {
                Console.Write(
                    "Valor inválido. Escolha um universo ou 0 para voltar: "
                );
            }

            /* Voltar ao menu principal caso o usuário escolha a opção 0. */
            if (escolhaUniverso == 0)
            {
                aguardarEnter = false;
                Console.Clear();
                break;
            }

            /* Verificar se o universo escolhido existe na lista. */
            if (escolhaUniverso < 1 ||
                escolhaUniverso > universosSalvos.Length)
            {
                Console.WriteLine();
                Console.WriteLine("Opção inválida.");
                break;
            }

            /*  Obter o caminho do arquivo do universo escolhido pelo usuário. */
            string arquivoEscolhido =
                universosSalvos[escolhaUniverso - 1];

            /* Limpar a tela antes de exibir o universo escolhido. */
            Console.Clear();

            /*  Criar um gravador de arquivo de texto para carregar o universo salvo. */
            GravadorArquivoTexto gravadorCarregar =
                new GravadorArquivoTexto();

            /* Carregar o universo salvo do arquivo txt. */
            Universo universoCarregado =
                gravadorCarregar.Carregar(
                    arquivoEscolhido
                );

            /* Exibir uma mensagem informando que o universo foi carregado com sucesso. */
            Console.WriteLine("Universo carregado");
            Console.WriteLine();

            /* Exibir informações sobre o universo carregado, como o nome do arquivo, quantidade de corpos, iterações e tempo entre iterações. */
            Console.WriteLine(
                $"Arquivo: {Path.GetFileName(arquivoEscolhido)}"
            );

            Console.WriteLine();

            /* Exibir a quantidade de corpos, iterações e tempo entre iterações do universo carregado. */
            Console.WriteLine(
                $"Quantidade de corpos: {universoCarregado.Corpos.Count}"
            );

            /* Exibir a quantidade de iterações e o tempo entre iterações do universo carregado. */
            Console.WriteLine(
                $"Quantidade de iterações: {universoCarregado.QuantidadeIteracoes}"
            );

            /* Exibir o tempo entre iterações do universo carregado. */
            Console.WriteLine(
                $"Tempo entre iterações: {universoCarregado.TempoEntreIteracoes}"
            );

            Console.WriteLine();
            Console.WriteLine("Corpos carregados:");
            Console.WriteLine();

            /* Exibir o estado inicial do universo carregado, mostrando os corpos e suas propriedades. */
            universoCarregado.ExibirEstado();

            Console.WriteLine();
            Console.WriteLine("Continuando simulação...");
            Console.WriteLine();

            /* Executar a simulação do universo carregado com base na quantidade de iterações e tempo entre iterações informados no arquivo. */
            universoCarregado.ExecutarSimulacao(
                universoCarregado.QuantidadeIteracoes,
                universoCarregado.TempoEntreIteracoes
            );

            break;

        /* Apagar um universo salvo de um arquivo txt. */
        case "3":

            Console.WriteLine("Apagar universo salvo");
            Console.WriteLine();

            /* Aqui serve para buscar todos os arquivos de universos salvos. */
            string[] universosParaDeletar =
                Directory.GetFiles(
                    ".",
                    "universo_*.txt"
                );

            /* Verificar se existem universos salvos. */
            if (universosParaDeletar.Length == 0)
            {
                Console.WriteLine(
                    "Nenhum universo salvo foi encontrado no sistema."
                );

                break;
            }

            /* Exibir os universos salvos para o usuário escolher qual apagar. */
            for (int i = 0; i < universosParaDeletar.Length; i++)
            {
                Console.WriteLine(
                    $"{i + 1} - {Path.GetFileName(universosParaDeletar[i])}"
                );
            }

            /* Exibir a opção de voltar ao menu principal. */
            Console.WriteLine("0 - Voltar");

            /* Solicitar ao usuário que escolha um universo para apagar. */
            Console.WriteLine();
            Console.Write("Escolha um universo para apagar: ");

            /* Inicializar a variável que armazenará a escolha do usuário. */
            int escolhaApagar;

            /* Verificar se a entrada do usuário é um número válido, caso contrário, solicitar novamente. */
            while (!int.TryParse(
                       Console.ReadLine(),
                       out escolhaApagar))
            {
                Console.Write(
                    "Valor inválido. Escolha um universo para apagar ou 0 para voltar: "
                );
            }

            /* Voltar ao menu principal caso o usuário escolha a opção 0. */
            if (escolhaApagar == 0)
            {
                aguardarEnter = false;
                Console.Clear();
                break;
            }

            /* Verificar se a opção escolhida existe na lista. */
            if (escolhaApagar < 1 ||
                escolhaApagar > universosParaDeletar.Length)
            {
                Console.WriteLine();
                Console.WriteLine("Opção inválida.");
                break;
            }

            /* Obter o caminho do arquivo selecionado para deletar. */
            string arquivoDeletar =
                universosParaDeletar[escolhaApagar - 1];

            /* Excluir o arquivo selecionado no disco. */
            File.Delete(arquivoDeletar);

            Console.WriteLine();
            Console.WriteLine(
                $"Arquivo {Path.GetFileName(arquivoDeletar)} apagado com sucesso."
            );

            break;

        /* Apagar todos os universos salvos de uma vez. */
        case "4":

            /* Exibir uma mensagem informando que o usuário escolheu apagar todos os universos salvos. */
            Console.WriteLine("Apagar todos os universos salvos");
            Console.WriteLine();

            /* Buscar todos os arquivos de universos salvos para apagar. */
            string[] universosParaApagarTodos =
                Directory.GetFiles(".", "universo_*.txt");

            /* Verificar se existem universos salvos para apagar. */
            if (universosParaApagarTodos.Length == 0)
            {
                Console.WriteLine("Nenhum universo salvo foi encontrado no sistema.");
                break;
            }

            /* Solicitar confirmação do usuário antes de apagar todos os arquivos. */
            Console.WriteLine($"Isso vai apagar {universosParaApagarTodos.Length} arquivo(s) salvo(s).");
            Console.Write("Tem certeza? (S/N): ");

            /* Ler a confirmação do usuário. */
            string? confirmacaoApagarTodos = Console.ReadLine();

            /* Verificar se a confirmação do usuário é válida, caso contrário, cancelar a operação. */
            if (confirmacaoApagarTodos == null ||
                confirmacaoApagarTodos.Trim().ToUpper() != "S")
            {
                /* Cancelar a operação caso o usuário não confirme com "S". */
                Console.WriteLine();
                Console.WriteLine("Operação cancelada.");
                break;
            }

            /* Apagar todos os arquivos de universos salvos encontrados. */
            foreach (string arquivoParaApagar in universosParaApagarTodos)
            {
                File.Delete(arquivoParaApagar);
            }

            /* Exibir uma mensagem informando que todos os arquivos foram apagados com sucesso. */
            Console.WriteLine();
            Console.WriteLine($"{universosParaApagarTodos.Length} universo(s) apagado(s) com sucesso.");

            break;

        /* Encerrar o programa. */
        case "0":

            Console.WriteLine("Programa encerrado.");

            /* Definir as variáveis de controle para encerrar o loop principal e não aguardar ENTER antes de sair. */
            continuar = false;
            aguardarEnter = false;

            break;

        default:

            /* Caso o usuário escolha uma opção inválida, exibir uma mensagem informando que a opção é inválida. */
            Console.WriteLine("Opção inválida.");

            break;
    }

    /* Aguardar o usuário pressionar ENTER para voltar ao menu, caso a opção escolhida não seja sair. */
    if (continuar && aguardarEnter)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Pressione ENTER para voltar ao menu."
        );

        Console.ReadLine();
        Console.Clear();
    }
}
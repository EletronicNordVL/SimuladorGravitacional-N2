# Simulador Gravitacional 2D — N2

> Projeto prático desenvolvido para a disciplina de **Programação Avançada de Sistemas (PAS)** no curso de **Análise e Desenvolvimento de Sistemas (TADS)**.

## 📌 Sobre o Projeto

O **Simulador Gravitacional 2D (Versão N2)** é a evolução do sistema de simulação de mecânica celeste e gravitação universal em C#.

Nesta segunda versão (N2), a aplicação substituiu o antigo salvamento em arquivos de texto por uma **arquitetura de banco de dados relacional MySQL (3FN)** com **transações ADO.NET**, e acelerou o motor físico integrando **Computação Paralela (`Parallel.For`)** para distribuir os cálculos de atração gravitacional N-corpos entre múltiplos núcleos de processador.

## 🚀 Principais Novidades e Funcionalidades (N2)

1. **Computação Paralela (`Parallel.For`)**:
   - Paralelização dos cálculos das forças gravitacionais resultantes entre todos os pares de corpos.
   - Atualização simultânea de posições, velocidades e acelerações (MRUV) usando a biblioteca `System.Threading.Tasks`.
   - Execução *thread-safe* (sem condições de corrida) garantida pelo acúmulo local de forças por corpo.

2. **Persistência Relacional em MySQL (3ª Forma Normal)**:
   - Estrutura relacional normalizada: `Simulacao` (Metadata), `Corpo` (Propriedades físicas) e `EstadoCorpo` (Histórico temporal).
   - Gerenciamento de alterações com **Transações (`MySqlTransaction`)** para garantir atomicidade nas inserções.
   - Chaves estrangeiras com **`ON DELETE CASCADE`** para remoção automática e segura de simulações e seus históricos.
   - **Índices de Performance**: Índice composto `idx_estado_corpo_passo (corpo_id, num_passo)` otimizando leituras na funcionalidade de Replay.

3. **Menu Interativo e Replay de Simulação**:
   - **Criar Novo Universo**: Configuração personalizada de quantidade de corpos, iterações e intervalo de tempo.
   - **Replay de Simulação (Leitura do MySQL)**: Consulta dos registros armazenados no MySQL e reprodução passo a passo do histórico de movimento.
   - **Gerenciamento de Dados**: Listagem e remoção de simulações diretamente via banco de dados.

## 🗄️ Modelo do Banco de Dados (MySQL)

O esquema relacional é composto por três tabelas estruturadas em hierarquia de 3ª Forma Normal (3FN):

* **`Simulacao`** (Tabela Pai): Armazena os parâmetros globais da execução (`id_simulacao`, `nome`, `data_criacao`, `qtd_corpos`, `qtd_iteracoes`, `tempo_entre_iteracoes`).
* **`Corpo`** (Tabela Filho): Registra cada corpo celeste e suas propriedades físicas fixas (`id_corpo`, `id_simulacao`, `nome`, `massa`, `densidade`).
* **`EstadoCorpo`** (Tabela Neto): Armazena todo o histórico da simulação (`id_registro`, `corpo_id`, `num_passo`, `tempo_decorrido`, posições, velocidades, forças e acelerações em X e Y).

```
+------------------------------------+
|             Simulacao              |
+------------------------------------+
| PK  id_simulacao (INT, AUTO_INC)   |
|     nome (VARCHAR)                 |
|     data_criacao (DATETIME)        |
|     qtd_corpos (INT)               |
|     qtd_iteracoes (INT)            |
|     tempo_entre_iteracoes (DOUBLE) |
+------------------------------------+
                  | 1
                  |
                  | N
+------------------------------------+
|               Corpo                |
+------------------------------------+
| PK  id_corpo (INT, AUTO_INC)       |
| FK  id_simulacao (INT)             |
|     nome (VARCHAR)                 |
|     massa (DOUBLE)                 |
|     densidade (DOUBLE)             |
+------------------------------------+
                  | 1
                  |
                  | N
+------------------------------------+
|            EstadoCorpo             |
+------------------------------------+
| PK  id_registro (BIGINT, AUTO_INC) |
| FK  corpo_id (INT)                 |
|     num_passo (INT)                |
|     tempo_decorrido (DOUBLE)       |
|     pos_x, pos_y (DOUBLE)          |
|     vel_x, vel_y (DOUBLE)          |
|     forca_x, forca_y (DOUBLE)      |
|     aceleracao_x, aceleracao_y     |
+------------------------------------+
```

---

## 🏗️ Estrutura e Arquitetura do Projeto

* `ConexaoBD.cs`: Gerenciador da conexão ADO.NET com `MySql.Data`.
* `GravadorUniversoBD.cs`: DAO com transações para operações de gravação, replay e deleção no MySQL.
* `Universo.cs`: Motor físico de mecânica celeste paralelizado com `Parallel.For`.
* `Corpo.cs`: Modelo de dados e estado físico das entidades celestes.
* `Program.cs`: Menu interativo via console para controle de execução.

## ⚙️ Tecnologias Utilizadas

* **Linguagem**: C# (.NET SDK)
* **Computação Paralela**: Task Parallel Library (TPL) — `Parallel.For`
* **Banco de Dados**: MySQL Server 8.0+
* **Driver MySQL**: `MySql.Data` (ADO.NET)

## 💻 Como Executar o Projeto

### Pré-requisitos
1. .NET SDK instalado na máquina.
2. Servidor MySQL em execução e MySQL Workbench.

### Passo a Passo

1. **Configurar o Banco de Dados**:
   - No MySQL Workbench, execute o script DDL para criar o banco `simulador_gravitacional` e suas tabelas (`Simulacao`, `Corpo`, `EstadoCorpo`).

2. **Configurar Conexão**:
   - No arquivo `ConexaoBD.cs`, ajuste os parâmetros da string de conexão para apontar para o seu servidor MySQL.

3. **Executar a Aplicação**:
   - No terminal, navegue até a pasta do projeto e rode `dotnet run`.

## 👨‍💻 Autores

Projeto desenvolvido para o trabalho acadêmico da **N2** da disciplina de **Programação Avançada de Sistemas (PAS)**.

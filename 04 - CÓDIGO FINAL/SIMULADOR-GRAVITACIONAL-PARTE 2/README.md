# Simulador Gravitacional 2D — Versão N2 (MySQL + Computação Paralela)

> Projeto prático desenvolvido para as disciplinas de **Programação Avançada de Sistemas (PAS)** e **Banco de Dados II (BD II)** no curso de **Análise e Desenvolvimento de Sistemas (TADS)**.

## 📌 Sobre o Projeto

O **Simulador Gravitacional 2D — N2** é a evolução da aplicação desenvolvida na etapa N1. O sistema simula a atração gravitacional e interações físicas entre múltiplos corpos celestes (N-Corpos) no espaço bidimensional, aplicando as leis de gravitação universal de Newton e equações de movimento.

Nesta versão **N2**, o sistema passou por duas grandes evoluções arquiteturais:
1. **Persistência Relacional em MySQL**: Substituição da gravação em arquivos texto (`.txt`) por um banco de dados relacional otimizado, permitindo registrar metadados de simulação, dados de corpos e todo o histórico temporal de estados para reprodução (Replay).
2. **Computação Paralela (`Parallel.For`)**: Paralelização dos cálculos de força gravitacional e aceleração entre múltiplos núcleos de processamento, otimizando o desempenho na execução com múltiplos corpos.

## 🚀 Principais Evoluções da N2 (vs N1)

| Funcionalidade | Versão N1 (Original) | Versão N2 (Atual) |
| :--- | :--- | :--- |
| **Persistência de Dados** | Arquivos locais `.txt` (`GravadorUniverso.cs`) | Banco de dados **MySQL** (`simulador_gravitacional`) |
| **Processamento Físico** | Sequencial / Monothread (`for` simples) | **Paralelo / Multithread** (`Parallel.For`) |
| **Modelagem de Dados** | Estrutura plana em texto | Modelagem Relacional (3FN) com PKs, FKs e Índices |
| **Histórico / Replay** | Leitura sequencial de arquivo | Consultas SQL otimizadas por índice (`idx_estado_corpo_passo`) |

## 🛠️ Tecnologias Utilizadas

* **Linguagem Backend**: C# (.NET 8.0+)
* **Banco de Dados**: MySQL 8.0+
* **Driver MySQL**: `MySql.Data`
* **Paralelismo**: `System.Threading.Tasks` (`Parallel.For`)
* **Interface / Visualização**: HTML5 Canvas, JavaScript, Web Frontend
* **Controle de Versão**: Git e GitHub

## 🗄️ Arquitetura do Banco de Dados

O banco de dados foi modelado seguindo as regras de integridade relacional e a 3ª Forma Normal (3FN), estruturado em três tabelas hierárquicas:

```
+------------------+          +------------------+          +----------------------+
|    Simulacao     | 1      N |      Corpo       | 1      N |     EstadoCorpo      |
+------------------+----------+------------------+----------+----------------------+
| PK id_simulacao  |<---------| PK id_corpo      |<---------| PK id_registro       |
|    nome          |          | FK id_simulacao  |          | FK corpo_id          |
|    data_criacao  |          |    nome          |          |    num_passo         |
|    qtd_corpos    |          |    massa         |          |    tempo_decorrido   |
|    qtd_iteracoes |          |    densidade     |          |    pos_x, pos_y      |
|    tempo_entre...|          +------------------+          |    vel_x, vel_y      |
+------------------+                                        |    forca_x, forca_y  |
                                                            |    aceleracao_x, y   |
                                                            +----------------------+
```

* **`Simulacao` (Pai)**: Registra o contexto e os parâmetros globais da simulação.
* **`Corpo` (Filho)**: Guarda as propriedades físicas fixas/iniciais de cada corpo celeste vinculado à simulação.
* **`EstadoCorpo` (Neto)**: Registra a série temporal de posições, velocidades, forças e acelerações de cada corpo em cada passo (iteração).

## 📄 Script SQL DDL (`simulador_gravitacional.sql`)

Para criar o banco de dados e suas estruturas no MySQL Workbench ou Terminal, execute o script abaixo:

```sql
/* 1. Criação e Seleção do Banco de Dados */
DROP DATABASE IF EXISTS simulador_gravitacional;
CREATE DATABASE IF NOT EXISTS simulador_gravitacional;
USE simulador_gravitacional;

/* 2. Tabela Pai: Simulacao */
CREATE TABLE Simulacao (
    id_simulacao INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255) NOT NULL,
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    qtd_corpos INT NOT NULL,
    qtd_iteracoes INT NOT NULL,
    tempo_entre_iteracoes DOUBLE NOT NULL
);

/* 3. Tabela Filho (1:N): Corpo */
CREATE TABLE Corpo (
    id_corpo INT PRIMARY KEY AUTO_INCREMENT,
    id_simulacao INT NOT NULL,
    nome VARCHAR(255) NOT NULL,
    massa DOUBLE NOT NULL,
    densidade DOUBLE NOT NULL
);

/* 4. Tabela Neto (1:N): EstadoCorpo (Histórico do Replay) */
CREATE TABLE EstadoCorpo (
    id_registro BIGINT PRIMARY KEY AUTO_INCREMENT,
    corpo_id INT NOT NULL,
    num_passo INT NOT NULL,
    tempo_decorrido DOUBLE NOT NULL,
    pos_x DOUBLE NOT NULL,
    pos_y DOUBLE NOT NULL,
    vel_x DOUBLE NOT NULL,
    vel_y DOUBLE NOT NULL,
    forca_x DOUBLE NOT NULL,
    forca_y DOUBLE NOT NULL,
    aceleracao_x DOUBLE NOT NULL,
    aceleracao_y DOUBLE NOT NULL
);

/* 5. Definição das Chaves Estrangeiras (FKs) com ON DELETE CASCADE */
ALTER TABLE Corpo 
    ADD CONSTRAINT FK_Corpo_Simulacao
    FOREIGN KEY (id_simulacao)
    REFERENCES Simulacao (id_simulacao)
    ON DELETE CASCADE;

ALTER TABLE EstadoCorpo 
    ADD CONSTRAINT FK_EstadoCorpo_Corpo
    FOREIGN KEY (corpo_id)
    REFERENCES Corpo (id_corpo)
    ON DELETE CASCADE;

/* 6. Índice de Performance para Leitura do Replay */
CREATE INDEX idx_estado_corpo_passo ON EstadoCorpo(corpo_id, num_passo);
```

## ⚡ Computação Paralela (`Parallel.For`)

O cálculo das forças gravitacionais entre os corpos possui complexidade computacional quadrática. Para otimizar a atualização do universo a cada iteração, o laço de cálculo das forças foi paralelizado em C# utilizando a classe `Parallel.For`:

```csharp
/* Exemplo da estrutura de paralelização no Universo.cs */
Parallel.For(0, corpos.Count, i =>
{
    Corpo corpoA = corpos[i];
    /* Zera forças acumuladas do passo anterior */
    corpoA.ZerarForca();

    for (int j = 0; j < corpos.Count; j++)
    {
        if (i != j)
        {
            Corpo corpoB = corpos[j];
            corpoA.CalcularAtracaoGravitacional(corpoB);
        }
    }
});
```

## ⚙️ Como Configurar e Executar o Projeto

### Pré-requisitos
* .NET SDK 8.0+
* MySQL Server 8.0+ ou MySQL Workbench

### 1. Configurar o Banco de Dados
1. Abra o MySQL Workbench e conecte-se ao seu servidor local.
2. Execute o script SQL acima (`simulador_gravitacional.sql`).

### 2. Configurar a String de Conexão no C#
No arquivo de configuração/conexão do projeto (`ConexaoBD.cs` ou `appsettings.json`), ajuste as credenciais de acesso ao seu MySQL:
```csharp
string connectionString = "Server=localhost;Database=simulador_gravitacional;Uid=root;Pwd=sua_senha;";
```

### 3. Instalar o Driver MySQL e Executar
No terminal, dentro da pasta do projeto:
```bash
dotnet restore
dotnet run
```

## 📊 Relatório Comparativo de Desempenho

Como parte das exigências do trabalho, os tempos de execução entre a versão N1 (Sequencial + Arquivo TXT) e a versão N2 (Paralela + MySQL) são medidos e comparados para diferentes quantidades de corpos (ex: 10, 50, 100 e 500 corpos). Os dados detalhados encontram-se no relatório técnico entregue em anexo.

## 👨‍💻 Autor

Desenvolvido como requisito de avaliação do Trabalho N2 para as disciplinas de **Programação Avançada de Sistemas (PAS)** e **Banco de Dados II (BD II)**.

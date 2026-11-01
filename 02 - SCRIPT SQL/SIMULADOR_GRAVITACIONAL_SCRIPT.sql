/* 1. Criação e Seleção do Banco de Dados */
DROP DATABASE IF EXISTS simulador_gravitacional;
CREATE DATABASE IF NOT EXISTS simulador_gravitacional; 
USE simulador_gravitacional;

/* 2. Tabela de Simulacao */
CREATE TABLE Simulacao (
  id_simulacao INT PRIMARY KEY AUTO_INCREMENT,
  nome VARCHAR(255) NOT NULL,
  data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
  qtd_corpos INT NOT NULL, 
  qtd_iteracoes INT NOT NULL, 
  tempo_entre_iteracoes DOUBLE NOT NULL
);

/* 3. Tabela de Corpos */
CREATE TABLE Corpo (
  id_corpo INT PRIMARY KEY AUTO_INCREMENT,
  id_simulacao INT NOT NULL,
  nome VARCHAR(255) NOT NULL,
  massa DOUBLE NOT NULL, 
  densidade DOUBLE NOT NULL
);

/* 4. Tabela de EstadoCorpo */
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

/* 5. Definição das Chaves Estrangeiras (FKs) */
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
   
/* 6. Criação de Indice de Performance para Otimizar a Leitura do Replay */
CREATE INDEX idx_estado_corpo_passo ON EstadoCorpo(corpo_id, num_passo);

/* 7. Lista de Selects */

/* A) Ver a simulação criada */
SELECT * FROM Simulacao;

/* B) Ver os corpos da simulação */
SELECT * FROM Corpo;

/* B.1) Ver o Estado dos Corpos presentes da simulação */
SELECT * FROM EstadoCorpo;

/* C) Ver os primeiros passos do histórico gravados */
SELECT * FROM EstadoCorpo LIMIT 15;

/* D) Comando para apagar as simulações por meio do ID */
DELETE FROM Simulacao WHERE id_simulacao = 1;

/* E) Comando para apagar todas as simulações */
DELETE FROM Simulacao;

/* F) Comando para uma consulta de um EstadoCorpo utilizando o Explain Select */
EXPLAIN SELECT * FROM EstadoCorpo WHERE corpo_id = 1 AND num_passo = 50;
 
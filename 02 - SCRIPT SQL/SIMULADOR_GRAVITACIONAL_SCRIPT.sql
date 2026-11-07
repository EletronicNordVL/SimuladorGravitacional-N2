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

/* C.1) Uma consulta da contagem do total de estados dos corpos */
SELECT COUNT(*) AS TotalEstados FROM EstadoCorpo;

/* C.2) Ver o total de corpos e o total de seus estados */
SELECT ec.corpo_id, c.nome, COUNT(*) AS total_estados FROM EstadoCorpo ec INNER JOIN Corpo c ON c.id_corpo = ec.corpo_id
GROUP BY ec.corpo_id, c.nome ORDER BY ec.corpo_id;

/* D) Comando para apagar as simulações por meio do ID */
DELETE FROM Simulacao WHERE id_simulacao = 1;
DELETE FROM Simulacao WHERE id_simulacao = 5;

/* E) Comando para apagar todas as simulações */
DELETE FROM Simulacao;

/* F) Comando para uma consulta de um EstadoCorpo utilizando o Explain Select */
EXPLAIN SELECT * FROM EstadoCorpo WHERE corpo_id = 1 AND num_passo = 50;

/* F.1) Ter a descrição completo dos Corpos e seus estados */
DESCRIBE EstadoCorpo;
DESCRIBE Corpo;

/* G) Adicionar o 'Stored Procedure' para remover registros com mais de 1 mês de antiguidade */
DELIMITER $$
CREATE PROCEDURE sp_limpar_simulacoes_antigas() 
BEGIN
    DELETE FROM Simulacao
    WHERE data_criacao < DATE_SUB(NOW(), INTERVAL 1 MONTH); 
    END $$ 
DELIMITER ;

/* G.1) Executar a limpeza automaticamente uma vez por dia */
CREATE EVENT IF NOT EXISTS ev_limpar_simulacoes_antigas
ON SCHEDULE EVERY 1 DAY
STARTS CURRENT_TIMESTAMP + INTERVAL 1 DAY
DO
    CALL sp_limpar_simulacoes_antigas();

/* 6.2) Verificar se o Agendador está ligado */
SHOW VARIABLES LIKE 'event_scheduler';
SHOW EVENTS FROM simulador_gravitacional;

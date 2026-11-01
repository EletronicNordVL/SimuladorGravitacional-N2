/* ModeloLógico: */

CREATE TABLE Simulacao (
    id_simulacao INT PRIMARY KEY AUTO_INCREMENT,
    nome VARCHAR(255),
    data_criacao DATETIME,
    qtd_corpos INT,
    qtd_iteracoes INT,
    tempo_entre_iteracoes DOUBLE
);

CREATE TABLE Corpo (
    id_corpo INT PRIMARY KEY AUTO_INCREMENT,
    id_simulacao INT,
    nome VARCHAR(255),
    massa DOUBLE,
    densidade DOUBLE
);

CREATE TABLE EstadoCorpo (
    id_registro BIGINT PRIMARY KEY AUTO_INCREMENT,
    tempo_decorrido DOUBLE,
    pos_x DOUBLE,
    pos_y DOUBLE,
    vel_x DOUBLE,
    vel_y DOUBLE,
    forca_x DOUBLE,
    forca_y DOUBLE,
    aceleracao_x DOUBLE,
    aceleracao_y DOUBLE,
    num_passo INT,
    corpo_id INT
);
 
ALTER TABLE Simulacao ADD CONSTRAINT FK_Simulacao_2
    FOREIGN KEY (Id_Corpo)
    REFERENCES Corpo (id_corpo)
    ON DELETE RESTRICT;
 
ALTER TABLE Corpo ADD CONSTRAINT FK_Corpo_2
    FOREIGN KEY (EstadoCorpo)
    REFERENCES EstadoCorpo (id_registro)
    ON DELETE RESTRICT;
 
ALTER TABLE Corpo ADD CONSTRAINT FK_Corpo_3
    FOREIGN KEY (id_simulacao)
    REFERENCES Simulacao (id_simulacao);
 
ALTER TABLE EstadoCorpo ADD CONSTRAINT FK_EstadoCorpo_2
    FOREIGN KEY (corpo_id)
    REFERENCES Corpo (id_corpo)
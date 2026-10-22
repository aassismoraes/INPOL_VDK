ALTER TABLE pessoa 
ADD COLUMN Coordenadas POINT NULL;

ALTER TABLE pessoa 
ADD COLUMN politicoId CHAR(36) NULL;

update pessoa set politicoId = 'f2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e';
-- 1. Normaliza registros de pessoas que estão com Nome ou Status nulos no MySQL
UPDATE pessoa SET Status = 'ATIVO' WHERE Status IS NULL;
UPDATE pessoa SET Nome = 'Sem Nome' WHERE Nome IS NULL;

-- é necessário migrar a tabela padrinho primeiro !!!!
-- 1. Script para migração dos registros da tabela legada 'padrinho' para 'caboseleitorais'
INSERT INTO caboseleitorais (
    Id,
    Nome,
    Email,
    Telefone,
    AreaInfluencia,
    Ativo,
    CriadoEm
)
SELECT 
    p.Id,
    p.Nome,
    p.Email,
    p.Celular,           -- Mapeia a coluna Celular para Telefone
    LEFT(p.Influencia, 150), -- Trunca para 150 chars se a coluna AreaInfluencia for varchar(150)
    1,                   -- Marca o registro como Ativo (1 = true)
    NOW(6)               -- Define a data de criação com precisão de microsegundos
FROM padrinho p
WHERE NOT EXISTS (
    SELECT 1 
    FROM caboseleitorais c 
    WHERE c.Id = p.Id
);

-- 2. Se a tabela de cabos eleitorais (padrinhos) tiver linhas com Nome nulo
UPDATE caboseleitorais SET Nome = 'Sem Nome' WHERE Nome IS NULL;

-- importar os dados da tabela tag !!!!

INSERT INTO tags (
    Id, 
    Nome, 
    Categoria, 
    CriadoEm, 
    AtualizadoEm, 
    DeletadoEm, 
    Ativo
)
SELECT 
    Id, 
    LEFT(Name, 50) AS Nome,                   -- Trunca para 50 chars para caber em varchar(50)
    LEFT(Description, 50) AS Categoria,       -- Mapeia a descrição como Categoria (limitando a 50 chars)
    NOW(6) AS CriadoEm,                       -- Preenche a data atual de criação
    NULL AS AtualizadoEm,                     -- Nulo por padrão na importação
    NULL AS DeletadoEm,                       -- Nulo por padrão
    1 AS Ativo                                -- Define como ativo (tinyint 1)
FROM tag;

ALTER TABLE pessoa DROP FOREIGN KEY FK_Pessoa_Padrinho_PadrinhoId;

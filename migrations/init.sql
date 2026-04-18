-- 1. Usuários
CREATE TABLE usuarios (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150) UNIQUE NOT NULL,
    senha_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('ADMIN', 'PROFISSIONAL')),
    profissao VARCHAR(100),
    taxa_comissao_padrao DECIMAL(5,2) DEFAULT 40.00,
    criado_em TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 2. Pacientes
CREATE TABLE pacientes (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    cpf VARCHAR(14) UNIQUE NOT NULL,
    telefone VARCHAR(20),
    data_nascimento DATE,
    criado_em TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 3. Vínculo Paciente <-> Profissional
CREATE TABLE paciente_profissional (
    paciente_id INT NOT NULL REFERENCES pacientes(id) ON DELETE CASCADE,
    profissional_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    criado_em TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (paciente_id, profissional_id)
);

-- 4. Serviços
CREATE TABLE servicos (
    id SERIAL PRIMARY KEY,
    profissional_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    nome VARCHAR(100) NOT NULL,
    valor_atual DECIMAL(10,2) NOT NULL,
    ativo BOOLEAN DEFAULT TRUE
);

-- 5. Agendamentos
CREATE TABLE agendamentos (
    id SERIAL PRIMARY KEY,
    paciente_id INT NOT NULL REFERENCES pacientes(id) ON DELETE CASCADE,
    profissional_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    servico_id INT NOT NULL REFERENCES servicos(id) ON DELETE RESTRICT,
    data_hora_inicio TIMESTAMPTZ NOT NULL,
    data_hora_fim TIMESTAMPTZ NOT NULL,
    valor_combinado DECIMAL(10,2) NOT NULL,
    percentual_comissao_momento DECIMAL(5,2) NOT NULL,
    status VARCHAR(20) NOT NULL CHECK (status IN ('AGENDADO', 'REALIZADO', 'FALTA', 'CANCELADO')),
    pago_pelo_paciente BOOLEAN DEFAULT FALSE,
    recorrencia_group_id VARCHAR(50),
    criado_em TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- 6. Acertos de Comissão
CREATE TABLE acertos_comissao (
    id SERIAL PRIMARY KEY,
    profissional_id INT NOT NULL REFERENCES usuarios(id) ON DELETE CASCADE,
    periodo_referencia VARCHAR(7) NOT NULL,
    valor_pago_a_clinica DECIMAL(10,2) NOT NULL,
    data_pagamento TIMESTAMPTZ NOT NULL,
    confirmado_pelo_admin BOOLEAN DEFAULT FALSE,
    observacao TEXT
);

-- 7. Despesas da Clínica
CREATE TABLE despesas_clinica (
    id SERIAL PRIMARY KEY,
    descricao VARCHAR(255) NOT NULL,
    valor DECIMAL(10,2) NOT NULL,
    data_vencimento DATE NOT NULL,
    status_pagamento BOOLEAN DEFAULT FALSE,
    categoria VARCHAR(20) NOT NULL CHECK (categoria IN ('FIXA', 'VARIAVEL')),
    criado_em TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Seed: primeiro admin do sistema
INSERT INTO usuarios (nome, email, senha_hash, role)
VALUES (
    'Admin',
    'admin@clinica.com',
    -- bcrypt de 'admin123' — troque após o primeiro login!
    '$2a$10$h.9KoahsezEDgLRJJrQQD.IQindC..IuDvmeUcMRKN7bfzN9KPBOy',
    'ADMIN'
);
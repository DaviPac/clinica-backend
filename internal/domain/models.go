package domain

import "time"

// --- Usuário ---

type Role string

const (
	RoleAdmin        Role = "ADMIN"
	RoleProfissional Role = "PROFISSIONAL"
)

type Usuario struct {
	ID                 int       `json:"id"`
	Nome               string    `json:"nome"`
	Email              string    `json:"email"`
	SenhaHash          string    `json:"-"` // nunca serializa pra JSON
	Role               Role      `json:"role"`
	Profissao          *string   `json:"profissao"` // ponteiro = nullable
	TaxaComissaoPadrao float64   `json:"taxa_comissao_padrao"`
	CriadoEm           time.Time `json:"criado_em"`
}

// --- Paciente ---

type Paciente struct {
	ID             int        `json:"id"`
	Nome           string     `json:"nome"`
	CPF            *string    `json:"cpf"`
	Telefone       *string    `json:"telefone"`
	DataNascimento *time.Time `json:"data_nascimento"`
	CriadoEm       time.Time  `json:"criado_em"`
}

// --- Serviço ---

type Servico struct {
	ID             int     `json:"id"`
	ProfissionalID int     `json:"profissional_id"`
	Nome           string  `json:"nome"`
	ValorAtual     float64 `json:"valor_atual"`
	Ativo          bool    `json:"ativo"`
}

// --- Agendamento ---

type StatusAgendamento string

const (
	StatusAgendado  StatusAgendamento = "AGENDADO"
	StatusRealizado StatusAgendamento = "REALIZADO"
	StatusFalta     StatusAgendamento = "FALTA"
	StatusCancelado StatusAgendamento = "CANCELADO"
)

type Agendamento struct {
	ID                        int               `json:"id"`
	PacienteID                int               `json:"paciente_id"`
	ProfissionalID            int               `json:"profissional_id"`
	ServicoID                 int               `json:"servico_id"`
	DataHoraInicio            time.Time         `json:"data_hora_inicio"`
	DataHoraFim               time.Time         `json:"data_hora_fim"`
	ValorCombinado            float64           `json:"valor_combinado"`
	PercentualComissaoMomento float64           `json:"percentual_comissao_momento"`
	Status                    StatusAgendamento `json:"status"`
	PagoPeloPaciente          bool              `json:"pago_pelo_paciente"`
	RecorrenciaGroupID        *string           `json:"recorrencia_group_id"`
	CriadoEm                  time.Time         `json:"criado_em"`
}

// --- Acerto de Comissão ---

type AcertoComissao struct {
	ID                  int       `json:"id"`
	ProfissionalID      int       `json:"profissional_id"`
	PeriodoReferencia   string    `json:"periodo_referencia"` // "YYYY-MM"
	ValorPagoAClinica   float64   `json:"valor_pago_a_clinica"`
	DataPagamento       time.Time `json:"data_pagamento"`
	ConfirmadoPeloAdmin bool      `json:"confirmado_pelo_admin"`
	Observacao          *string   `json:"observacao"`
}

// --- Despesa ---

type CategoriaDesp string

const (
	CategoriaFixa     CategoriaDesp = "FIXA"
	CategoriaVariavel CategoriaDesp = "VARIAVEL"
)

type DespesaClinica struct {
	ID              int           `json:"id"`
	Descricao       string        `json:"descricao"`
	Valor           float64       `json:"valor"`
	DataVencimento  time.Time     `json:"data_vencimento"`
	StatusPagamento bool          `json:"status_pagamento"`
	Categoria       CategoriaDesp `json:"categoria"`
	CriadoEm        time.Time     `json:"criado_em"`
}

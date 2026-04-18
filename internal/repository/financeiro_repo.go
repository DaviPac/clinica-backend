package repository

import (
	"clinica-api/internal/domain"
	"context"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
)

type FinanceiroRepository struct {
	db *pgxpool.Pool
}

func NewFinanceiroRepository(db *pgxpool.Pool) *FinanceiroRepository {
	return &FinanceiroRepository{db: db}
}

// ── Acertos de Comissão ──────────────────────────────────────────────────────

func (r *FinanceiroRepository) CreateAcerto(ctx context.Context, a *domain.AcertoComissao) error {
	query := `
		INSERT INTO acertos_comissao
			(profissional_id, periodo_referencia, valor_pago_a_clinica, data_pagamento, observacao)
		VALUES ($1, $2, $3, $4, $5)
		RETURNING id`

	return r.db.QueryRow(ctx, query,
		a.ProfissionalID, a.PeriodoReferencia,
		a.ValorPagoAClinica, a.DataPagamento, a.Observacao,
	).Scan(&a.ID)
}

func (r *FinanceiroRepository) ConfirmarAcerto(ctx context.Context, id int) error {
	_, err := r.db.Exec(ctx,
		`UPDATE acertos_comissao SET confirmado_pelo_admin = true WHERE id = $1`,
		id,
	)
	return err
}

func (r *FinanceiroRepository) ListAcertosByProfissional(ctx context.Context, profissionalID int) ([]*domain.AcertoComissao, error) {
	query := `
		SELECT id, profissional_id, periodo_referencia,
			   valor_pago_a_clinica, data_pagamento, confirmado_pelo_admin, observacao
		FROM acertos_comissao
		WHERE profissional_id = $1
		ORDER BY data_pagamento DESC`

	rows, err := r.db.Query(ctx, query, profissionalID)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var lista []*domain.AcertoComissao
	for rows.Next() {
		a := &domain.AcertoComissao{}
		if err := rows.Scan(
			&a.ID, &a.ProfissionalID, &a.PeriodoReferencia,
			&a.ValorPagoAClinica, &a.DataPagamento, &a.ConfirmadoPeloAdmin, &a.Observacao,
		); err != nil {
			return nil, err
		}
		lista = append(lista, a)
	}
	return lista, rows.Err()
}

// ── Despesas ─────────────────────────────────────────────────────────────────

func (r *FinanceiroRepository) CreateDespesa(ctx context.Context, d *domain.DespesaClinica) error {
	query := `
		INSERT INTO despesas_clinica (descricao, valor, data_vencimento, categoria, status_pagamento)
		VALUES ($1, $2, $3, $4, $5)
		RETURNING id, criado_em`

	return r.db.QueryRow(ctx, query,
		d.Descricao, d.Valor, d.DataVencimento, d.Categoria, d.StatusPagamento,
	).Scan(&d.ID, &d.CriadoEm)
}

func (r *FinanceiroRepository) ListDespesas(ctx context.Context, apenasEmAberto bool) ([]*domain.DespesaClinica, error) {
	query := `
		SELECT id, descricao, valor, data_vencimento, status_pagamento, categoria, criado_em
		FROM despesas_clinica
		WHERE ($1 = false OR status_pagamento = false)
		ORDER BY data_vencimento`

	rows, err := r.db.Query(ctx, query, apenasEmAberto)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var lista []*domain.DespesaClinica
	for rows.Next() {
		d := &domain.DespesaClinica{}
		if err := rows.Scan(
			&d.ID, &d.Descricao, &d.Valor, &d.DataVencimento,
			&d.StatusPagamento, &d.Categoria, &d.CriadoEm,
		); err != nil {
			return nil, err
		}
		lista = append(lista, d)
	}
	return lista, rows.Err()
}

func (r *FinanceiroRepository) MarcarDespesaPaga(ctx context.Context, id int) error {
	_, err := r.db.Exec(ctx,
		`UPDATE despesas_clinica SET status_pagamento = true WHERE id = $1`, id,
	)
	return err
}

// ── Relatório ─────────────────────────────────────────────────────────────────

type ResumoComissaoProfissional struct {
	ProfissionalID   int     `json:"profissional_id"`
	NomeProfissional string  `json:"nome_profissional"`
	TotalRecebido    float64 `json:"total_recebido"`   // soma dos valores_combinados pagos
	ComissaoClinica  float64 `json:"comissao_clinica"` // o que a clínica tem a receber
	TotalAcertado    float64 `json:"total_acertado"`   // já pago via acerto
	Pendente         float64 `json:"pendente"`         // comissao_clinica - total_acertado
}

type RelatorioFinanceiro struct {
	Periodo        string                       `json:"periodo"` // "YYYY-MM"
	Profissionais  []ResumoComissaoProfissional `json:"profissionais"`
	TotalComissoes float64                      `json:"total_comissoes"` // soma de comissao_clinica
	TotalDespesas  float64                      `json:"total_despesas"`
	LucroLiquido   float64                      `json:"lucro_liquido"`
}

func (r *FinanceiroRepository) GerarRelatorio(ctx context.Context, periodo string) (*RelatorioFinanceiro, error) {
	// 1. Comissões por profissional no período
	queryComissoes := `
		SELECT
			u.id,
			u.nome,
			COALESCE(SUM(a.valor_combinado), 0)                                           AS total_recebido,
			COALESCE(SUM(a.valor_combinado * a.percentual_comissao_momento / 100.0), 0)   AS comissao_clinica
		FROM agendamentos a
		INNER JOIN usuarios u ON u.id = a.profissional_id
		WHERE a.status = 'REALIZADO'
		  AND a.pago_pelo_paciente = true
		  AND TO_CHAR(a.data_hora_inicio AT TIME ZONE 'UTC', 'YYYY-MM') = $1
		GROUP BY u.id, u.nome
		ORDER BY u.nome`

	rows, err := r.db.Query(ctx, queryComissoes, periodo)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var profissionais []ResumoComissaoProfissional
	var totalComissoes float64

	for rows.Next() {
		var p ResumoComissaoProfissional
		if err := rows.Scan(&p.ProfissionalID, &p.NomeProfissional, &p.TotalRecebido, &p.ComissaoClinica); err != nil {
			return nil, err
		}
		profissionais = append(profissionais, p)
		totalComissoes += p.ComissaoClinica
	}
	if err := rows.Err(); err != nil {
		return nil, err
	}

	// 2. Acertos já realizados no período
	queryAcertos := `
		SELECT profissional_id, COALESCE(SUM(valor_pago_a_clinica), 0)
		FROM acertos_comissao
		WHERE periodo_referencia = $1 AND confirmado_pelo_admin = true
		GROUP BY profissional_id`

	rowsAcertos, err := r.db.Query(ctx, queryAcertos, periodo)
	if err != nil {
		return nil, err
	}
	defer rowsAcertos.Close()

	acertosMap := map[int]float64{}
	for rowsAcertos.Next() {
		var pid int
		var total float64
		if err := rowsAcertos.Scan(&pid, &total); err != nil {
			return nil, err
		}
		acertosMap[pid] = total
	}

	// 3. Calcula pendente por profissional
	for i := range profissionais {
		p := &profissionais[i]
		p.TotalAcertado = acertosMap[p.ProfissionalID]
		p.Pendente = p.ComissaoClinica - p.TotalAcertado
	}

	// 4. Total de despesas do período
	var totalDespesas float64
	err = r.db.QueryRow(ctx, `
		SELECT COALESCE(SUM(valor), 0)
		FROM despesas_clinica
		WHERE status_pagamento = true
		  AND TO_CHAR(data_vencimento, 'YYYY-MM') = $1`,
		periodo,
	).Scan(&totalDespesas)
	if err != nil {
		return nil, err
	}

	return &RelatorioFinanceiro{
		Periodo:        periodo,
		Profissionais:  profissionais,
		TotalComissoes: totalComissoes,
		TotalDespesas:  totalDespesas,
		LucroLiquido:   totalComissoes - totalDespesas,
	}, nil
}

// Calcula quanto um profissional deve à clínica num período (usado antes de criar acerto)
func (r *FinanceiroRepository) SaldoDevidoProfissional(ctx context.Context, profissionalID int, periodo string) (float64, error) {
	var comissaoTotal float64
	err := r.db.QueryRow(ctx, `
		SELECT COALESCE(SUM(valor_combinado * percentual_comissao_momento / 100.0), 0)
		FROM agendamentos
		WHERE profissional_id = $1
		  AND status = 'REALIZADO'
		  AND pago_pelo_paciente = true
		  AND TO_CHAR(data_hora_inicio AT TIME ZONE 'UTC', 'YYYY-MM') = $2`,
		profissionalID, periodo,
	).Scan(&comissaoTotal)
	if err != nil {
		return 0, err
	}

	var jaAcertado float64
	err = r.db.QueryRow(ctx, `
		SELECT COALESCE(SUM(valor_pago_a_clinica), 0)
		FROM acertos_comissao
		WHERE profissional_id = $1
		  AND periodo_referencia = $2
		  AND confirmado_pelo_admin = true`,
		profissionalID, periodo,
	).Scan(&jaAcertado)
	if err != nil {
		return 0, err
	}

	return comissaoTotal - jaAcertado, nil
}

// Utilitário: período no formato YYYY-MM a partir de uma data
func PeriodoDe(t time.Time) string {
	return t.UTC().Format("2006-01")
}

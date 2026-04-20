package repository

import (
	"clinica-api/internal/domain"
	"context"
	"fmt"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
)

type AgendamentoRepository struct {
	db *pgxpool.Pool
}

func NewAgendamentoRepository(db *pgxpool.Pool) *AgendamentoRepository {
	return &AgendamentoRepository{db: db}
}

func (r *AgendamentoRepository) Create(ctx context.Context, a *domain.Agendamento) error {
	query := `
		INSERT INTO agendamentos (
			paciente_id, profissional_id, servico_id,
			data_hora_inicio, data_hora_fim,
			valor_combinado, percentual_comissao_momento,
			status, pago_pelo_paciente, recorrencia_group_id
		) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10)
		RETURNING id, criado_em`

	return r.db.QueryRow(ctx, query,
		a.PacienteID, a.ProfissionalID, a.ServicoID,
		a.DataHoraInicio, a.DataHoraFim,
		a.ValorCombinado, a.PercentualComissaoMomento,
		a.Status, a.PagoPeloPaciente, a.RecorrenciaGroupID,
	).Scan(&a.ID, &a.CriadoEm)
}

// Criação em lote para agendamentos recorrentes
func (r *AgendamentoRepository) CreateLote(ctx context.Context, agendamentos []*domain.Agendamento) error {
	tx, err := r.db.Begin(ctx)
	if err != nil {
		return err
	}
	defer tx.Rollback(ctx)

	for _, a := range agendamentos {
		query := `
			INSERT INTO agendamentos (
				paciente_id, profissional_id, servico_id,
				data_hora_inicio, data_hora_fim,
				valor_combinado, percentual_comissao_momento,
				status, pago_pelo_paciente, recorrencia_group_id
			) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10)
			RETURNING id, criado_em`

		if err := tx.QueryRow(ctx, query,
			a.PacienteID, a.ProfissionalID, a.ServicoID,
			a.DataHoraInicio, a.DataHoraFim,
			a.ValorCombinado, a.PercentualComissaoMomento,
			a.Status, a.PagoPeloPaciente, a.RecorrenciaGroupID,
		).Scan(&a.ID, &a.CriadoEm); err != nil {
			return fmt.Errorf("erro ao inserir agendamento: %w", err)
		}
	}

	return tx.Commit(ctx)
}

func (r *AgendamentoRepository) FindByID(ctx context.Context, id int) (*domain.Agendamento, error) {
	query := `
		SELECT id, paciente_id, profissional_id, servico_id,
			   data_hora_inicio, data_hora_fim,
			   valor_combinado, percentual_comissao_momento,
			   status, pago_pelo_paciente, recorrencia_group_id, criado_em
		FROM agendamentos WHERE id = $1`

	a := &domain.Agendamento{}
	err := r.db.QueryRow(ctx, query, id).Scan(
		&a.ID, &a.PacienteID, &a.ProfissionalID, &a.ServicoID,
		&a.DataHoraInicio, &a.DataHoraFim,
		&a.ValorCombinado, &a.PercentualComissaoMomento,
		&a.Status, &a.PagoPeloPaciente, &a.RecorrenciaGroupID, &a.CriadoEm,
	)
	return a, err
}

// Lista com filtro opcional de período
func (r *AgendamentoRepository) ListAll(
	ctx context.Context,
	de, ate *time.Time,
) ([]*domain.Agendamento, error) {
	query := `
		SELECT id, paciente_id, profissional_id, servico_id,
			   data_hora_inicio, data_hora_fim,
			   valor_combinado, percentual_comissao_momento,
			   status, pago_pelo_paciente, recorrencia_group_id, criado_em
		FROM agendamentos
		WHERE ($1::timestamptz IS NULL OR data_hora_inicio >= $1)
		  AND ($2::timestamptz IS NULL OR data_hora_inicio <= $2)
		ORDER BY data_hora_inicio`

	rows, err := r.db.Query(ctx, query, de, ate)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	return scanAgendamentos(rows)
}

// Lista por profissional com filtro opcional de período
func (r *AgendamentoRepository) ListByProfissional(
	ctx context.Context,
	profissionalID int,
	de, ate *time.Time,
) ([]*domain.Agendamento, error) {
	query := `
		SELECT id, paciente_id, profissional_id, servico_id,
			   data_hora_inicio, data_hora_fim,
			   valor_combinado, percentual_comissao_momento,
			   status, pago_pelo_paciente, recorrencia_group_id, criado_em
		FROM agendamentos
		WHERE profissional_id = $1
		  AND ($2::timestamptz IS NULL OR data_hora_inicio >= $2)
		  AND ($3::timestamptz IS NULL OR data_hora_inicio <= $3)
		ORDER BY data_hora_inicio`

	rows, err := r.db.Query(ctx, query, profissionalID, de, ate)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	return scanAgendamentos(rows)
}

func (r *AgendamentoRepository) UpdateStatus(ctx context.Context, id int, status domain.StatusAgendamento) error {
	_, err := r.db.Exec(ctx,
		`UPDATE agendamentos SET status = $1 WHERE id = $2`,
		status, id,
	)
	return err
}

func (r *AgendamentoRepository) UpdatePagamento(ctx context.Context, id int, pago bool) error {
	_, err := r.db.Exec(ctx,
		`UPDATE agendamentos SET pago_pelo_paciente = $1 WHERE id = $2`,
		pago, id,
	)
	return err
}

// Cancela todos os agendamentos futuros de um grupo de recorrência
func (r *AgendamentoRepository) CancelarRecorrencia(ctx context.Context, groupID string) error {
	_, err := r.db.Exec(ctx, `
		UPDATE agendamentos
		SET status = 'CANCELADO'
		WHERE recorrencia_group_id = $1
		  AND data_hora_inicio > NOW()
		  AND status = 'AGENDADO'`,
		groupID,
	)
	return err
}

func (r *AgendamentoRepository) VerificarOwnership(ctx context.Context, groupID string, profissionalID int) (bool, error) {
	query := `
		SELECT EXISTS (
			SELECT 1 FROM agendamentos
			WHERE recorrencia_group_id = $1 
			  AND profissional_id = $2
		)`

	var isOwner bool
	err := r.db.QueryRow(ctx, query, groupID, profissionalID).Scan(&isOwner)

	return isOwner, err
}

// Usado para verificar conflito de horário antes de agendar
func (r *AgendamentoRepository) ExisteConflito(
	ctx context.Context,
	profissionalID int,
	inicio, fim time.Time,
	ignorarID *int, // para edições — ignora o próprio agendamento
) (bool, error) {
	query := `
		SELECT EXISTS (
			SELECT 1 FROM agendamentos
			WHERE profissional_id = $1
			  AND status NOT IN ('CANCELADO', 'FALTA')
			  AND data_hora_inicio < $3
			  AND data_hora_fim   > $2
			  AND ($4::int IS NULL OR id != $4)
		)`

	var existe bool
	err := r.db.QueryRow(ctx, query, profissionalID, inicio, fim, ignorarID).Scan(&existe)
	return existe, err
}

func scanAgendamentos(rows interface {
	Next() bool
	Scan(...any) error
	Err() error
}) ([]*domain.Agendamento, error) {
	var lista []*domain.Agendamento
	for rows.Next() {
		a := &domain.Agendamento{}
		if err := rows.Scan(
			&a.ID, &a.PacienteID, &a.ProfissionalID, &a.ServicoID,
			&a.DataHoraInicio, &a.DataHoraFim,
			&a.ValorCombinado, &a.PercentualComissaoMomento,
			&a.Status, &a.PagoPeloPaciente, &a.RecorrenciaGroupID, &a.CriadoEm,
		); err != nil {
			return nil, err
		}
		lista = append(lista, a)
	}
	return lista, rows.Err()
}

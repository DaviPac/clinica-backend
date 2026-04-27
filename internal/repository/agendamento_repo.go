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
			valor_combinado, valor_pacote, percentual_comissao_momento,
			status, pago_pelo_paciente, recorrencia_group_id
		) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11)
		RETURNING id, criado_em`

	return r.db.QueryRow(ctx, query,
		a.PacienteID, a.ProfissionalID, a.ServicoID,
		a.DataHoraInicio, a.DataHoraFim,
		a.ValorCombinado, a.ValorPacote, a.PercentualComissaoMomento,
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
				valor_combinado, valor_pacote, percentual_comissao_momento,
				status, pago_pelo_paciente, recorrencia_group_id
			) VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11)
			RETURNING id, criado_em`

		if err := tx.QueryRow(ctx, query,
			a.PacienteID, a.ProfissionalID, a.ServicoID,
			a.DataHoraInicio, a.DataHoraFim,
			a.ValorCombinado, a.ValorPacote, a.PercentualComissaoMomento,
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
			   valor_combinado, valor_pacote, percentual_comissao_momento,
			   status, pago_pelo_paciente, recorrencia_group_id, criado_em
		FROM agendamentos WHERE id = $1`

	a := &domain.Agendamento{}
	err := r.db.QueryRow(ctx, query, id).Scan(
		&a.ID, &a.PacienteID, &a.ProfissionalID, &a.ServicoID,
		&a.DataHoraInicio, &a.DataHoraFim,
		&a.ValorCombinado, &a.ValorPacote, &a.PercentualComissaoMomento,
		&a.Status, &a.PagoPeloPaciente, &a.RecorrenciaGroupID, &a.CriadoEm,
	)
	return a, err
}

func (r *AgendamentoRepository) List(ctx context.Context, f domain.FiltroAgendamento) ([]*domain.Agendamento, error) {
	// 1. Query base (1=1 é um truque clássico para facilitar a adição de ANDs depois)
	query := `
		SELECT id, paciente_id, profissional_id, servico_id,
			   data_hora_inicio, data_hora_fim,
			   valor_combinado, valor_pacote, percentual_comissao_momento,
			   status, pago_pelo_paciente, recorrencia_group_id, criado_em
		FROM agendamentos
		WHERE 1=1`

	var args []interface{}
	argID := 1 // Contador dinâmico para os parâmetros do Postgres ($1, $2...)

	// 2. Anexando filtros dinamicamente
	if f.ProfissionalID != nil {
		query += fmt.Sprintf(" AND profissional_id = $%d", argID)
		args = append(args, *f.ProfissionalID)
		argID++
	}

	if f.De != nil {
		query += fmt.Sprintf(" AND data_hora_inicio >= $%d", argID)
		args = append(args, *f.De)
		argID++
	}

	if f.Ate != nil {
		query += fmt.Sprintf(" AND data_hora_inicio <= $%d", argID)
		args = append(args, *f.Ate)
		argID++
	}

	if f.Status != nil {
		query += fmt.Sprintf(" AND status = $%d", argID)
		args = append(args, *f.Status)
		argID++
	}

	if f.ApenasAtrasados {
		query += " AND data_hora_fim < NOW()"
	}

	if f.PagamentoPendente {
		query += " AND pago_pelo_paciente = FALSE"
	}

	// 3. Finalizando a query
	query += " ORDER BY data_hora_inicio"

	// 4. Executando a query passando o slice 'args' descompactado (...args)
	rows, err := r.db.Query(ctx, query, args...)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	return scanAgendamentos(rows)
}

func (r *AgendamentoRepository) UpdateValorCombinado(ctx context.Context, id int, valor float64) error {
	_, err := r.db.Exec(ctx,
		`UPDATE agendamentos SET valor_combinado = ROUND($1, 2) WHERE id = $2`,
		valor, id,
	)
	return err
}

func (r *AgendamentoRepository) UpdateValorCombinadoRecorrente(ctx context.Context, id int, valor float64) error {
	a, err := r.FindByID(ctx, id)
	if err != nil {
		return err
	}
	if a == nil {
		return fmt.Errorf("agendamento nao encontrado")
	}
	if a.ValorPacote != nil {
		return fmt.Errorf("nao e possivel alterar recorrencia de pacote")
	}
	_, err = r.db.Exec(ctx,
		`UPDATE agendamentos SET valor_combinado = ROUND($1, 2) WHERE recorrencia_group_id = $2 AND pago_pelo_paciente = FALSE`,
		valor, id,
	)
	return err
}

func (r *AgendamentoRepository) UpdateStatus(ctx context.Context, id int, status domain.StatusAgendamento) error {
	_, err := r.db.Exec(ctx,
		`UPDATE agendamentos SET status = $1 WHERE id = $2`,
		status, id,
	)
	return err
}

func (r *AgendamentoRepository) UpdatePagamento(ctx context.Context, id int, pago bool) error {
	a, err := r.FindByID(ctx, id)
	if err != nil {
		return err
	}

	if a.ValorPacote != nil {
		if a.RecorrenciaGroupID == nil {
			return fmt.Errorf("id de recorrencia nulo em pacote")
		}

		if pago {
			// Lógica: MARCAR COMO PAGO
			var pendentes int
			queryCheck := `
				SELECT COUNT(1) 
				FROM agendamentos 
				WHERE recorrencia_group_id = $1 
				  AND id != $2 
				  AND valor_combinado > 0 
				  AND pago_pelo_paciente = false
			`
			err = r.db.QueryRow(ctx, queryCheck, *a.RecorrenciaGroupID, id).Scan(&pendentes)
			if err != nil {
				return fmt.Errorf("erro ao verificar status do pacote: %w", err)
			}

			// Se não há nenhum outro pendente, atinge todos da recorrência
			if pendentes == 0 {
				_, err = r.db.Exec(ctx,
					`UPDATE agendamentos SET pago_pelo_paciente = true WHERE recorrencia_group_id = $1`,
					*a.RecorrenciaGroupID,
				)
				return err // Retorna aqui, pois já atualizou todos
			}

			// Se houver pendentes, o código continuará para o final da função
			// e atualizará apenas o ID em questão.

		} else {
			// Lógica: DESMARCAR COMO PAGO
			// Atualiza o agendamento atual (independente do valor) E
			// todos os outros do mesmo grupo que tenham valor = 0
			_, err = r.db.Exec(ctx, `
				UPDATE agendamentos 
				SET pago_pelo_paciente = false 
				WHERE id = $1 
				   OR (recorrencia_group_id = $2 AND valor_combinado = 0)
			`, id, *a.RecorrenciaGroupID)

			return err // Retorna aqui, pois já resolveu a regra do pacote para desmarcar
		}
	}

	// Fallback padrão:
	// 1. Se NÃO for pacote.
	// 2. Se FOR pacote marcando como PAGO, mas ainda existirem outros pendentes de valor > 0.
	_, err = r.db.Exec(ctx,
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
			&a.ValorCombinado, &a.ValorPacote, &a.PercentualComissaoMomento,
			&a.Status, &a.PagoPeloPaciente, &a.RecorrenciaGroupID, &a.CriadoEm,
		); err != nil {
			return nil, err
		}
		lista = append(lista, a)
	}
	return lista, rows.Err()
}

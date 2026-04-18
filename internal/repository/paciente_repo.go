package repository

import (
	"clinica-api/internal/domain"
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"
)

type PacienteRepository struct {
	db *pgxpool.Pool
}

func NewPacienteRepository(db *pgxpool.Pool) *PacienteRepository {
	return &PacienteRepository{db: db}
}

// Cria paciente e já vincula ao profissional numa transação
func (r *PacienteRepository) Create(ctx context.Context, p *domain.Paciente, profissionalID int) error {
	tx, err := r.db.Begin(ctx)
	if err != nil {
		return err
	}
	defer tx.Rollback(ctx)

	query := `
		INSERT INTO pacientes (nome, cpf, telefone, data_nascimento)
		VALUES ($1, $2, $3, $4)
		RETURNING id, criado_em`

	err = tx.QueryRow(ctx, query,
		p.Nome, p.CPF, p.Telefone, p.DataNascimento,
	).Scan(&p.ID, &p.CriadoEm)
	if err != nil {
		return fmt.Errorf("erro ao criar paciente: %w", err)
	}

	_, err = tx.Exec(ctx,
		`INSERT INTO paciente_profissional (paciente_id, profissional_id) VALUES ($1, $2)`,
		p.ID, profissionalID,
	)
	if err != nil {
		return fmt.Errorf("erro ao vincular paciente: %w", err)
	}

	return tx.Commit(ctx)
}

// Vincula paciente já existente (por CPF) a outro profissional
func (r *PacienteRepository) VincularProfissional(ctx context.Context, pacienteID, profissionalID int) error {
	_, err := r.db.Exec(ctx,
		`INSERT INTO paciente_profissional (paciente_id, profissional_id)
		 VALUES ($1, $2)
		 ON CONFLICT DO NOTHING`,
		pacienteID, profissionalID,
	)
	return err
}

// Busca por CPF — útil para verificar se paciente já existe antes de cadastrar
func (r *PacienteRepository) FindByCPF(ctx context.Context, cpf string) (*domain.Paciente, error) {
	query := `
		SELECT id, nome, cpf, telefone, data_nascimento, criado_em
		FROM pacientes WHERE cpf = $1`

	p := &domain.Paciente{}
	err := r.db.QueryRow(ctx, query, cpf).Scan(
		&p.ID, &p.Nome, &p.CPF, &p.Telefone, &p.DataNascimento, &p.CriadoEm,
	)
	if err != nil {
		return nil, err
	}
	return p, nil
}

func (r *PacienteRepository) FindByID(ctx context.Context, id int) (*domain.Paciente, error) {
	query := `
		SELECT id, nome, cpf, telefone, data_nascimento, criado_em
		FROM pacientes WHERE id = $1`

	p := &domain.Paciente{}
	err := r.db.QueryRow(ctx, query, id).Scan(
		&p.ID, &p.Nome, &p.CPF, &p.Telefone, &p.DataNascimento, &p.CriadoEm,
	)
	if err != nil {
		return nil, err
	}
	return p, nil
}

// Lista apenas pacientes do profissional autenticado (ou todos, se admin)
func (r *PacienteRepository) ListByProfissional(ctx context.Context, profissionalID int) ([]*domain.Paciente, error) {
	query := `
		SELECT p.id, p.nome, p.cpf, p.telefone, p.data_nascimento, p.criado_em
		FROM pacientes p
		INNER JOIN paciente_profissional pp ON pp.paciente_id = p.id
		WHERE pp.profissional_id = $1
		ORDER BY p.nome`

	rows, err := r.db.Query(ctx, query, profissionalID)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var pacientes []*domain.Paciente
	for rows.Next() {
		p := &domain.Paciente{}
		if err := rows.Scan(
			&p.ID, &p.Nome, &p.CPF, &p.Telefone, &p.DataNascimento, &p.CriadoEm,
		); err != nil {
			return nil, err
		}
		pacientes = append(pacientes, p)
	}
	return pacientes, nil
}

func (r *PacienteRepository) ListAll(ctx context.Context) ([]*domain.Paciente, error) {
	query := `
		SELECT id, nome, cpf, telefone, data_nascimento, criado_em
		FROM pacientes ORDER BY nome`

	rows, err := r.db.Query(ctx, query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var pacientes []*domain.Paciente
	for rows.Next() {
		p := &domain.Paciente{}
		if err := rows.Scan(
			&p.ID, &p.Nome, &p.CPF, &p.Telefone, &p.DataNascimento, &p.CriadoEm,
		); err != nil {
			return nil, err
		}
		pacientes = append(pacientes, p)
	}
	return pacientes, nil
}

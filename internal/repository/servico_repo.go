package repository

import (
	"clinica-api/internal/domain"
	"context"

	"github.com/jackc/pgx/v5/pgxpool"
)

type ServicoRepository struct {
	db *pgxpool.Pool
}

func NewServicoRepository(db *pgxpool.Pool) *ServicoRepository {
	return &ServicoRepository{db: db}
}

func (r *ServicoRepository) Create(ctx context.Context, s *domain.Servico) error {
	query := `
		INSERT INTO servicos (profissional_id, nome, valor_atual, ativo)
		VALUES ($1, $2, $3, $4)
		RETURNING id`

	return r.db.QueryRow(ctx, query,
		s.ProfissionalID, s.Nome, s.ValorAtual, s.Ativo,
	).Scan(&s.ID)
}

func (r *ServicoRepository) FindByID(ctx context.Context, id int) (*domain.Servico, error) {
	query := `
		SELECT id, profissional_id, nome, valor_atual, ativo
		FROM servicos WHERE id = $1`

	s := &domain.Servico{}
	err := r.db.QueryRow(ctx, query, id).Scan(
		&s.ID, &s.ProfissionalID, &s.Nome, &s.ValorAtual, &s.Ativo,
	)
	return s, err
}

func (r *ServicoRepository) ListAll(ctx context.Context, apenasAtivos bool) ([]*domain.Servico, error) {
	query := `
		SELECT id, profissional_id, nome, valor_atual, ativo
		FROM servicos
		WHERE ($1 = false OR ativo = true)
		ORDER BY nome`

	rows, err := r.db.Query(ctx, query, apenasAtivos)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var lista []*domain.Servico
	for rows.Next() {
		s := &domain.Servico{}
		if err := rows.Scan(&s.ID, &s.ProfissionalID, &s.Nome, &s.ValorAtual, &s.Ativo); err != nil {
			return nil, err
		}
		lista = append(lista, s)
	}
	return lista, rows.Err()
}

// Lista serviços do próprio profissional (ativos por padrão)
func (r *ServicoRepository) ListByProfissional(ctx context.Context, profissionalID int, apenasAtivos bool) ([]*domain.Servico, error) {
	query := `
		SELECT id, profissional_id, nome, valor_atual, ativo
		FROM servicos
		WHERE profissional_id = $1
		  AND ($2 = false OR ativo = true)
		ORDER BY nome`

	rows, err := r.db.Query(ctx, query, profissionalID, apenasAtivos)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var lista []*domain.Servico
	for rows.Next() {
		s := &domain.Servico{}
		if err := rows.Scan(&s.ID, &s.ProfissionalID, &s.Nome, &s.ValorAtual, &s.Ativo); err != nil {
			return nil, err
		}
		lista = append(lista, s)
	}
	return lista, rows.Err()
}

func (r *ServicoRepository) Update(ctx context.Context, s *domain.Servico) error {
	_, err := r.db.Exec(ctx,
		`UPDATE servicos SET nome = $1, valor_atual = $2, ativo = $3 WHERE id = $4 AND profissional_id = $5`,
		s.Nome, s.ValorAtual, s.Ativo, s.ID, s.ProfissionalID,
	)
	return err
}

// Soft delete — mantém histórico dos agendamentos que usaram o serviço
func (r *ServicoRepository) Desativar(ctx context.Context, id, profissionalID int) error {
	_, err := r.db.Exec(ctx,
		`UPDATE servicos SET ativo = false WHERE id = $1 AND profissional_id = $2`,
		id, profissionalID,
	)
	return err
}

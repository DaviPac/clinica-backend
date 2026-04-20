package repository

import (
	"clinica-api/internal/domain"
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"
)

type UsuarioRepository struct {
	db *pgxpool.Pool
}

func NewUsuarioRepository(db *pgxpool.Pool) *UsuarioRepository {
	return &UsuarioRepository{db: db}
}

func (r *UsuarioRepository) Create(ctx context.Context, u *domain.Usuario, senhaHash string) error {
	query := `
        INSERT INTO usuarios (nome, email, senha_hash, role, profissao, taxa_comissao_padrao)
        VALUES ($1, $2, $3, $4, $5, $6)
        RETURNING id, criado_em`

	return r.db.QueryRow(ctx, query,
		u.Nome, u.Email, senhaHash, u.Role, u.Profissao, u.TaxaComissaoPadrao,
	).Scan(&u.ID, &u.CriadoEm)
}

func (r *UsuarioRepository) FindByEmail(ctx context.Context, email string) (*domain.Usuario, string, error) {
	query := `
        SELECT id, nome, email, senha_hash, role, profissao, taxa_comissao_padrao, criado_em
        FROM usuarios
        WHERE email = $1`

	u := &domain.Usuario{}
	var senhaHash string

	err := r.db.QueryRow(ctx, query, email).Scan(
		&u.ID, &u.Nome, &u.Email, &senhaHash,
		&u.Role, &u.Profissao, &u.TaxaComissaoPadrao, &u.CriadoEm,
	)
	if err != nil {
		return nil, "", fmt.Errorf("usuário não encontrado: %w", err)
	}

	return u, senhaHash, nil
}

func (r *UsuarioRepository) FindByID(ctx context.Context, id int) (*domain.Usuario, error) {
	query := `
        SELECT id, nome, email, role, profissao, taxa_comissao_padrao, criado_em
        FROM usuarios
        WHERE id = $1`

	u := &domain.Usuario{}
	err := r.db.QueryRow(ctx, query, id).Scan(
		&u.ID, &u.Nome, &u.Email,
		&u.Role, &u.Profissao, &u.TaxaComissaoPadrao, &u.CriadoEm,
	)
	if err != nil {
		return nil, fmt.Errorf("usuário não encontrado: %w", err)
	}

	return u, nil
}

func (r *UsuarioRepository) ListAll(ctx context.Context) ([]*domain.Usuario, error) {
	query := `
        SELECT id, nome, email, role, profissao, taxa_comissao_padrao, criado_em
        FROM usuarios
        ORDER BY nome`

	rows, err := r.db.Query(ctx, query)
	if err != nil {
		return nil, err
	}
	defer rows.Close()

	var usuarios []*domain.Usuario
	for rows.Next() {
		u := &domain.Usuario{}
		if err := rows.Scan(
			&u.ID, &u.Nome, &u.Email,
			&u.Role, &u.Profissao, &u.TaxaComissaoPadrao, &u.CriadoEm,
		); err != nil {
			return nil, err
		}
		usuarios = append(usuarios, u)
	}

	return usuarios, nil
}

func (r *UsuarioRepository) UpdateProfile(ctx context.Context, id int, nome, profissao string) error {
	query := `
		UPDATE usuarios 
		SET nome = $1, profissao = $2 
		WHERE id = $3`

	commandTag, err := r.db.Exec(ctx, query, nome, profissao, id)
	if err != nil {
		return fmt.Errorf("erro ao atualizar perfil: %w", err)
	}

	if commandTag.RowsAffected() == 0 {
		return fmt.Errorf("usuário com id %d não encontrado", id)
	}

	return nil
}

func (r *UsuarioRepository) UpdatePassword(ctx context.Context, id int, novaSenhaHash string) error {
	query := `
        UPDATE usuarios 
        SET senha_hash = $1 
        WHERE id = $2`

	commandTag, err := r.db.Exec(ctx, query, novaSenhaHash, id)
	if err != nil {
		return fmt.Errorf("erro ao atualizar senha: %w", err)
	}

	if commandTag.RowsAffected() == 0 {
		return fmt.Errorf("usuário com id %d não encontrado", id)
	}

	return nil
}

func (r *UsuarioRepository) UpdateSystemRoles(ctx context.Context, id int, role domain.Role, taxaComissao float64) error {
	query := `
        UPDATE usuarios 
        SET role = $1, taxa_comissao_padrao = $2 
        WHERE id = $3`

	commandTag, err := r.db.Exec(ctx, query, role, taxaComissao, id)
	if err != nil {
		return fmt.Errorf("erro ao atualizar regras de sistema: %w", err)
	}

	if commandTag.RowsAffected() == 0 {
		return fmt.Errorf("usuário com id %d não encontrado", id)
	}

	return nil
}

func (r *UsuarioRepository) UpdateEmail(ctx context.Context, id int, novoEmail string) error {
	query := `
        UPDATE usuarios 
        SET email = $1 
        WHERE id = $2`

	commandTag, err := r.db.Exec(ctx, query, novoEmail, id)
	if err != nil {
		return fmt.Errorf("erro ao atualizar email: %w", err)
	}

	if commandTag.RowsAffected() == 0 {
		return fmt.Errorf("usuário com id %d não encontrado", id)
	}

	return nil
}

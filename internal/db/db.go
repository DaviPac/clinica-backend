package db

import (
	"context"
	"fmt"
	"os"

	"github.com/jackc/pgx/v5/pgxpool"
)

func NewPool(ctx context.Context) (*pgxpool.Pool, error) {
	dsn := os.Getenv("DATABASE_URL")
	if dsn == "" {
		// fallback para desenvolvimento local
		dsn = fmt.Sprintf(
			"host=%s port=%s user=%s password=%s dbname=%s sslmode=%s",
			os.Getenv("DB_HOST"), os.Getenv("DB_PORT"),
			os.Getenv("DB_USER"), os.Getenv("DB_PASSWORD"),
			os.Getenv("DB_NAME"), os.Getenv("DB_SSLMODE"),
		)
	}

	pool, err := pgxpool.New(ctx, dsn)
	if err != nil {
		return nil, fmt.Errorf("erro ao criar pool: %w", err)
	}

	if err := pool.Ping(ctx); err != nil {
		return nil, fmt.Errorf("banco inacessível: %w", err)
	}

	return pool, nil
}

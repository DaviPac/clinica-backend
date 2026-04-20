package main

import (
	"clinica-api/internal/db"
	"clinica-api/internal/router"
	"context"
	"log"
	"net/http"
	"os"

	"github.com/joho/godotenv"
)

func main() {
	if os.Getenv("RAILWAY_ENVIRONMENT") == "" {
		if err := godotenv.Load(); err != nil {
			log.Println("Arquivo .env não encontrado, usando variáveis de ambiente do sistema")
		}
	}

	ctx := context.Background()

	pool, err := db.NewPool(ctx)
	if err != nil {
		log.Fatalf("Falha na conexão com banco: %v", err)
	}
	defer pool.Close()

	log.Println("Banco de dados conectado com sucesso")

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	r := router.New(pool)
	log.Printf("Servidor rodando na porta %s", port)
	if err := http.ListenAndServe(":"+port, r); err != nil {
		log.Fatal(err)
	}
}

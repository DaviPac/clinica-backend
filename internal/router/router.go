package router

import (
	"clinica-api/internal/handler"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"

	"github.com/go-chi/chi/v5"
	chiMiddleware "github.com/go-chi/chi/v5/middleware"
	"github.com/go-chi/cors"
	"github.com/jackc/pgx/v5/pgxpool"
)

func New(db *pgxpool.Pool) *chi.Mux {
	r := chi.NewRouter()
	r.Use(cors.Handler(cors.Options{
		AllowedOrigins:   []string{"http://localhost:4200", "https://instituto-cin.vercel.app"}, // Exemplo para desenvolvimento (React/Vue/Vite)
		AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"},
		AllowedHeaders:   []string{"Accept", "Authorization", "Content-Type", "X-CSRF-Token"},
		ExposedHeaders:   []string{"Link"},
		AllowCredentials: true,
		MaxAge:           300,
	}))
	r.Use(chiMiddleware.Logger)
	r.Use(chiMiddleware.Recoverer)
	r.Use(chiMiddleware.RequestID)

	// Repositórios
	usuarioRepo := repository.NewUsuarioRepository(db)
	pacienteRepo := repository.NewPacienteRepository(db)
	agendamentoRepo := repository.NewAgendamentoRepository(db)
	servicoRepo := repository.NewServicoRepository(db)
	financeiroRepo := repository.NewFinanceiroRepository(db)

	// Handlers
	authHandler := handler.NewAuthHandler(usuarioRepo)
	pacienteHandler := handler.NewPacienteHandler(pacienteRepo)
	agendamentoHandler := handler.NewAgendamentoHandler(agendamentoRepo, usuarioRepo)
	servicoHandler := handler.NewServicoHandler(servicoRepo)
	financeiroHandler := handler.NewFinanceiroHandler(financeiroRepo)

	// Rotas públicas
	r.Post("/auth/login", authHandler.Login)

	r.Group(func(r chi.Router) {
		r.Use(middleware.Autenticar)

		// Auth
		r.Get("/auth/me", authHandler.Me)

		// Pacientes
		r.Get("/pacientes", pacienteHandler.Listar)
		r.Post("/pacientes", pacienteHandler.Criar)
		r.Get("/pacientes/{id}", pacienteHandler.BuscarPorID)

		// Agendamentos
		r.Post("/agendamentos", agendamentoHandler.Criar)
		r.Get("/agendamentos", agendamentoHandler.Listar)
		r.Patch("/agendamentos/{id}/status", agendamentoHandler.AtualizarStatus)
		r.Patch("/agendamentos/{id}/pagamento", agendamentoHandler.AtualizarPagamento)
		r.Delete("/agendamentos/recorrencia/{groupID}", agendamentoHandler.CancelarRecorrencia)

		// Serviços (cada profissional gerencia os próprios)
		r.Get("/servicos", servicoHandler.Listar)
		r.Post("/servicos", servicoHandler.Criar)
		r.Put("/servicos/{id}", servicoHandler.Atualizar)
		r.Delete("/servicos/{id}", servicoHandler.Desativar)

		// Financeiro — profissional
		r.Post("/financeiro/acertos", financeiroHandler.CriarAcerto)
		r.Get("/financeiro/acertos", financeiroHandler.ListarAcertos)
		r.Get("/financeiro/saldo-devido", financeiroHandler.SaldoDevido)

		// Financeiro + Admin exclusivo
		r.Group(func(r chi.Router) {
			r.Use(middleware.ApenasAdmin)
			r.Post("/auth/registrar", authHandler.Registrar)
			r.Get("/usuarios", authHandler.ListarUsuarios)
			r.Patch("/financeiro/acertos/{id}/confirmar", financeiroHandler.ConfirmarAcerto)
			r.Get("/financeiro/relatorio", financeiroHandler.Relatorio)
			r.Post("/financeiro/despesas", financeiroHandler.CriarDespesa)
			r.Get("/financeiro/despesas", financeiroHandler.ListarDespesas)
			r.Patch("/financeiro/despesas/{id}/pagar", financeiroHandler.MarcarDespesaPaga)
		})
	})

	return r
}

package handler

import (
	"clinica-api/internal/domain"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"
	"clinica-api/internal/utils"
	"encoding/json"
	"net/http"
	"strconv"

	"github.com/go-chi/chi/v5"
	"github.com/jackc/pgx/v5"
)

type PacienteHandler struct {
	repo *repository.PacienteRepository
}

func NewPacienteHandler(repo *repository.PacienteRepository) *PacienteHandler {
	return &PacienteHandler{repo: repo}
}

type criarPacienteRequest struct {
	Nome           string  `json:"nome"`
	CPF            string  `json:"cpf"`
	Telefone       *string `json:"telefone"`
	DataNascimento *string `json:"data_nascimento"` // "YYYY-MM-DD"
}

// POST /pacientes
func (h *PacienteHandler) Criar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())

	var req criarPacienteRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}
	if req.Nome == "" || req.CPF == "" {
		respondErro(w, "nome e cpf são obrigatórios", http.StatusBadRequest)
		return
	}

	// Se CPF já existe, apenas vincula ao profissional
	existente, err := h.repo.FindByCPF(r.Context(), req.CPF)
	if err == nil {
		if err := h.repo.VincularProfissional(r.Context(), existente.ID, profissionalID); err != nil {
			respondErro(w, "erro ao vincular paciente", http.StatusInternalServerError)
			return
		}
		respondJSON(w, existente, http.StatusOK)
		return
	}

	p := &domain.Paciente{
		Nome:     req.Nome,
		CPF:      req.CPF,
		Telefone: req.Telefone,
	}

	if req.DataNascimento != nil {
		t, err := utils.ParseDate(*req.DataNascimento)
		if err != nil {
			respondErro(w, "data_nascimento inválida (use YYYY-MM-DD)", http.StatusBadRequest)
			return
		}
		p.DataNascimento = &t
	}

	if err := h.repo.Create(r.Context(), p, profissionalID); err != nil {
		respondErro(w, "erro ao criar paciente", http.StatusInternalServerError)
		return
	}

	respondJSON(w, p, http.StatusCreated)
}

// GET /pacientes
func (h *PacienteHandler) Listar(w http.ResponseWriter, r *http.Request) {
	userID := middleware.GetUserID(r.Context())

	var (
		pacientes []*domain.Paciente
		err       error
	)

	pacientes, err = h.repo.ListByProfissional(r.Context(), userID)

	if err != nil {
		respondErro(w, "erro ao listar pacientes", http.StatusInternalServerError)
		return
	}

	if pacientes == nil {
		pacientes = []*domain.Paciente{}
	}

	respondJSON(w, pacientes, http.StatusOK)
}

// GET /pacientes/{id}
func (h *PacienteHandler) BuscarPorID(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondErro(w, "id inválido", http.StatusBadRequest)
		return
	}

	p, err := h.repo.FindByID(r.Context(), id)
	if err != nil {
		if err == pgx.ErrNoRows {
			respondErro(w, "paciente não encontrado", http.StatusNotFound)
			return
		}
		respondErro(w, "erro interno", http.StatusInternalServerError)
		return
	}

	respondJSON(w, p, http.StatusOK)
}

package handler

import (
	"clinica-api/internal/domain"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"
	"clinica-api/internal/utils"
	"encoding/json"
	"net/http"
	"strconv"
	"strings"

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
	CPF            *string `json:"cpf"`
	Telefone       *string `json:"telefone"`
	DataNascimento *string `json:"data_nascimento"` // "YYYY-MM-DD"
}

// POST /pacientes
func (h *PacienteHandler) Criar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())
	isAdmin := middleware.GetRole(r.Context()) == domain.RoleAdmin
	idQuery := r.URL.Query().Get("profissional_id")
	if idQuery != "" {
		if !isAdmin {
			respondErro(w, "nao autorizado", http.StatusForbidden)
			return
		}
		id, err := strconv.Atoi(idQuery)
		if err != nil {
			respondErro(w, "id deve ser numérico", http.StatusBadRequest)
			return
		}
		profissionalID = id
	}

	var req criarPacienteRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}
	if req.Nome == "" {
		respondErro(w, "nome é obrigatório", http.StatusBadRequest)
		return
	}

	if req.CPF != nil && strings.TrimSpace(*req.CPF) == "" {
		req.CPF = nil
	}

	// Se CPF já existe, apenas vincula ao profissional
	if req.CPF != nil {
		existente, err := h.repo.FindByCPF(r.Context(), *req.CPF)
		if err == nil {
			// Achou pelo CPF, vincula ao profissional!
			if err := h.repo.VincularProfissional(r.Context(), existente.ID, profissionalID); err != nil {
				respondErro(w, "erro ao vincular paciente", http.StatusInternalServerError)
				return
			}
			respondJSON(w, existente, http.StatusOK)
			return
		}
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
	isAdmin := middleware.GetRole(r.Context()) == domain.RoleAdmin
	mostrarTodos := r.URL.Query().Get("todos") == "true"
	idQuery := r.URL.Query().Get("profissional_id")
	if idQuery != "" {
		id, err := strconv.Atoi(idQuery)
		if err != nil {
			respondErro(w, "id deve ser numérico", http.StatusBadRequest)
			return
		}
		userID = id
	}

	var (
		pacientes []*domain.Paciente
		err       error
	)

	// Define qual query executar baseando-se nas permissões e filtros
	if isAdmin && mostrarTodos {
		pacientes, err = h.repo.ListAll(r.Context())
	} else {
		pacientes, err = h.repo.ListByProfissional(r.Context(), userID)
	}

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

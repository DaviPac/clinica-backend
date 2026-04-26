package handler

import (
	"clinica-api/internal/domain"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"
	"encoding/json"
	"net/http"
	"strconv"

	"github.com/go-chi/chi/v5"
)

type ServicoHandler struct {
	repo *repository.ServicoRepository
}

func NewServicoHandler(repo *repository.ServicoRepository) *ServicoHandler {
	return &ServicoHandler{repo: repo}
}

type criarServicoRequest struct {
	Nome       string  `json:"nome"`
	ValorAtual float64 `json:"valor_atual"`
	Pacote     bool    `json:"pacote"`
}

// POST /servicos
func (h *ServicoHandler) Criar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())

	var req criarServicoRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}
	if req.Nome == "" || req.ValorAtual <= 0 {
		respondErro(w, "nome e valor_atual são obrigatórios", http.StatusBadRequest)
		return
	}

	s := &domain.Servico{
		ProfissionalID: profissionalID,
		Nome:           req.Nome,
		ValorAtual:     req.ValorAtual,
		Ativo:          true,
		IsPacote:       req.Pacote,
	}

	if err := h.repo.Create(r.Context(), s); err != nil {
		respondErro(w, "erro ao criar serviço", http.StatusInternalServerError)
		return
	}

	respondJSON(w, s, http.StatusCreated)
}

// GET /servicos
func (h *ServicoHandler) Listar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())
	mostrarTodos := r.URL.Query().Get("todos") == "true" && middleware.GetRole(r.Context()) == domain.RoleAdmin
	apenasAtivos := r.URL.Query().Get("inativos") != "true"

	var servicos []*domain.Servico
	var err error
	if mostrarTodos {
		servicos, err = h.repo.ListAll(r.Context(), apenasAtivos)
	} else {
		servicos, err = h.repo.ListByProfissional(r.Context(), profissionalID, apenasAtivos)
	}

	if err != nil {
		respondErro(w, "erro ao listar serviços", http.StatusInternalServerError)
		return
	}
	if servicos == nil {
		servicos = []*domain.Servico{}
	}

	respondJSON(w, servicos, http.StatusOK)
}

// PUT /servicos/{id}
func (h *ServicoHandler) Atualizar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondErro(w, "id inválido", http.StatusBadRequest)
		return
	}

	var req struct {
		Nome       string  `json:"nome"`
		ValorAtual float64 `json:"valor_atual"`
		Ativo      bool    `json:"ativo"`
		Pacote     bool    `json:"pacote"`
	}
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	s := &domain.Servico{
		ID:             id,
		ProfissionalID: profissionalID, // garante que só edita o próprio
		Nome:           req.Nome,
		ValorAtual:     req.ValorAtual,
		Ativo:          req.Ativo,
		IsPacote:       req.Pacote,
	}

	if err := h.repo.Update(r.Context(), s); err != nil {
		respondErro(w, "erro ao atualizar serviço", http.StatusInternalServerError)
		return
	}

	respondJSON(w, s, http.StatusOK)
}

// DELETE /servicos/{id}
func (h *ServicoHandler) Desativar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondErro(w, "id inválido", http.StatusBadRequest)
		return
	}

	if err := h.repo.Desativar(r.Context(), id, profissionalID); err != nil {
		respondErro(w, "erro ao desativar serviço", http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusNoContent)
}

package handler

import (
	"clinica-api/internal/domain"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"
	"encoding/json"
	"net/http"
	"strconv"
	"time"

	"github.com/go-chi/chi/v5"
)

type FinanceiroHandler struct {
	repo *repository.FinanceiroRepository
}

func NewFinanceiroHandler(repo *repository.FinanceiroRepository) *FinanceiroHandler {
	return &FinanceiroHandler{repo: repo}
}

// POST /financeiro/acertos  (apenas admin — registra repasse ao profissional)
func (h *FinanceiroHandler) CriarAcerto(w http.ResponseWriter, r *http.Request) {
	var body struct {
		ProfissionalID    int     `json:"profissional_id"`
		PeriodoReferencia string  `json:"periodo_referencia"` // "YYYY-MM"
		ValorPago         float64 `json:"valor_pago"`
		Observacao        *string `json:"observacao"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}
	if body.ProfissionalID <= 0 {
		respondErro(w, "profissional_id é obrigatório", http.StatusBadRequest)
		return
	}
	if body.ValorPago <= 0 {
		respondErro(w, "valor_pago deve ser positivo", http.StatusBadRequest)
		return
	}

	a := &domain.AcertoComissao{
		ProfissionalID:    body.ProfissionalID,
		PeriodoReferencia: body.PeriodoReferencia,
		ValorPago:         body.ValorPago,
		DataPagamento:     time.Now(),
		Observacao:        body.Observacao,
	}

	if err := h.repo.CreateAcerto(r.Context(), a); err != nil {
		respondErro(w, "erro ao registrar acerto", http.StatusInternalServerError)
		return
	}

	respondJSON(w, a, http.StatusCreated)
}

// GET /financeiro/acertos  (profissional vê os próprios; admin filtra por ?profissional_id=)
func (h *FinanceiroHandler) ListarAcertos(w http.ResponseWriter, r *http.Request) {
	ctx := r.Context()
	role := middleware.GetRole(ctx)
	userID := middleware.GetUserID(ctx)

	var profissionalID int
	var listarTodos bool

	if role == domain.RoleAdmin {
		if pid := r.URL.Query().Get("profissional_id"); pid != "" {
			id, err := strconv.Atoi(pid)
			if err != nil || id <= 0 {
				respondErro(w, "profissional_id inválido", http.StatusBadRequest)
				return
			}
			profissionalID = id
		} else {
			listarTodos = true
		}
	} else {
		profissionalID = userID
	}

	var acertos []*domain.AcertoComissao
	var err error

	if listarTodos {
		acertos, err = h.repo.ListAcertos(ctx)
	} else {
		acertos, err = h.repo.ListAcertosByProfissional(ctx, profissionalID)
	}

	if err != nil {
		respondErro(w, "erro ao listar acertos", http.StatusInternalServerError)
		return
	}
	if acertos == nil {
		acertos = []*domain.AcertoComissao{}
	}

	respondJSON(w, acertos, http.StatusOK)
}

// GET /financeiro/saldo-a-receber?periodo=2025-01
func (h *FinanceiroHandler) SaldoAReceber(w http.ResponseWriter, r *http.Request) {
	ctx := r.Context()
	profissionalID := middleware.GetUserID(ctx)
	role := middleware.GetRole(ctx)

	if role == domain.RoleAdmin {
		if pid := r.URL.Query().Get("profissional_id"); pid != "" {
			id, err := strconv.Atoi(pid)
			if err != nil || id <= 0 {
				respondErro(w, "profissional_id inválido", http.StatusBadRequest)
				return
			}
			profissionalID = id
		}
	}

	periodo := r.URL.Query().Get("periodo")
	if periodo == "" {
		periodo = repository.PeriodoDe(time.Now())
	}

	saldo, err := h.repo.SaldoAReceberProfissional(ctx, profissionalID, periodo)
	if err != nil {
		respondErro(w, "erro ao calcular saldo", http.StatusInternalServerError)
		return
	}

	respondJSON(w, map[string]any{
		"profissional_id": profissionalID,
		"periodo":         periodo,
		"saldo_a_receber": saldo,
	}, http.StatusOK)
}

// GET /financeiro/relatorio?periodo=2025-01  (apenas admin)
func (h *FinanceiroHandler) Relatorio(w http.ResponseWriter, r *http.Request) {
	periodo := r.URL.Query().Get("periodo")
	if periodo == "" {
		periodo = repository.PeriodoDe(time.Now())
	}

	relatorio, err := h.repo.GerarRelatorio(r.Context(), periodo)
	if err != nil {
		respondErro(w, "erro ao gerar relatório", http.StatusInternalServerError)
		return
	}

	respondJSON(w, relatorio, http.StatusOK)
}

// POST /financeiro/despesas  (apenas admin)
func (h *FinanceiroHandler) CriarDespesa(w http.ResponseWriter, r *http.Request) {
	var body struct {
		Descricao      string               `json:"descricao"`
		Valor          float64              `json:"valor"`
		DataVencimento string               `json:"data_vencimento"` // "YYYY-MM-DD"
		Categoria      domain.CategoriaDesp `json:"categoria"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	venc, err := time.Parse("2006-01-02", body.DataVencimento)
	if err != nil {
		respondErro(w, "data_vencimento inválida (use YYYY-MM-DD)", http.StatusBadRequest)
		return
	}

	d := &domain.DespesaClinica{
		Descricao:      body.Descricao,
		Valor:          body.Valor,
		DataVencimento: venc,
		Categoria:      body.Categoria,
	}

	if err := h.repo.CreateDespesa(r.Context(), d); err != nil {
		respondErro(w, "erro ao criar despesa", http.StatusInternalServerError)
		return
	}

	respondJSON(w, d, http.StatusCreated)
}

// GET /financeiro/despesas?em_aberto=true
func (h *FinanceiroHandler) ListarDespesas(w http.ResponseWriter, r *http.Request) {
	apenasEmAberto := r.URL.Query().Get("em_aberto") == "true"

	despesas, err := h.repo.ListDespesas(r.Context(), apenasEmAberto)
	if err != nil {
		respondErro(w, "erro ao listar despesas", http.StatusInternalServerError)
		return
	}
	if despesas == nil {
		despesas = []*domain.DespesaClinica{}
	}

	respondJSON(w, despesas, http.StatusOK)
}

// PATCH /financeiro/despesas/{id}/pagar  (apenas admin)
func (h *FinanceiroHandler) MarcarDespesaPaga(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondErro(w, "id inválido", http.StatusBadRequest)
		return
	}

	if err := h.repo.MarcarDespesaPaga(r.Context(), id); err != nil {
		respondErro(w, "erro ao marcar despesa como paga", http.StatusInternalServerError)
		return
	}

	respondJSON(w, map[string]bool{"pago": true}, http.StatusOK)
}

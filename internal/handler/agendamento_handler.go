package handler

import (
	"clinica-api/internal/domain"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"
	"encoding/json"
	"fmt"
	"net/http"
	"strconv"
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/google/uuid"
)

type AgendamentoHandler struct {
	repo        *repository.AgendamentoRepository
	usuarioRepo *repository.UsuarioRepository
}

func NewAgendamentoHandler(
	repo *repository.AgendamentoRepository,
	usuarioRepo *repository.UsuarioRepository,
) *AgendamentoHandler {
	return &AgendamentoHandler{repo: repo, usuarioRepo: usuarioRepo}
}

type criarAgendamentoRequest struct {
	PacienteID     int     `json:"paciente_id"`
	ServicoID      int     `json:"servico_id"`
	DataHoraInicio string  `json:"data_hora_inicio"` // RFC3339
	DataHoraFim    string  `json:"data_hora_fim"`
	ValorCombinado float64 `json:"valor_combinado"`
	// Recorrência opcional
	Recorrente       bool `json:"recorrente"`
	TotalSessoes     int  `json:"total_sessoes"`     // ex: 10
	IntervaloSemanas int  `json:"intervalo_semanas"` // ex: 1 (semanal)
}

// POST /agendamentos
func (h *AgendamentoHandler) Criar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())

	var req criarAgendamentoRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	inicio, err := time.Parse(time.RFC3339, req.DataHoraInicio)
	if err != nil {
		respondErro(w, "data_hora_inicio inválida (use RFC3339)", http.StatusBadRequest)
		return
	}
	fim, err := time.Parse(time.RFC3339, req.DataHoraFim)
	if err != nil {
		respondErro(w, "data_hora_fim inválida (use RFC3339)", http.StatusBadRequest)
		return
	}
	if !fim.After(inicio) {
		respondErro(w, "data_hora_fim deve ser após data_hora_inicio", http.StatusBadRequest)
		return
	}

	// Verifica conflito de horário
	conflito, err := h.repo.ExisteConflito(r.Context(), profissionalID, inicio, fim, nil)
	if err != nil {
		respondErro(w, "erro ao verificar conflito", http.StatusInternalServerError)
		return
	}
	if conflito {
		respondErro(w, "conflito de horário com agendamento existente", http.StatusConflict)
		return
	}

	// Busca taxa de comissão do profissional
	usuario, err := h.usuarioRepo.FindByID(r.Context(), profissionalID)
	if err != nil {
		respondErro(w, "erro ao buscar profissional", http.StatusInternalServerError)
		return
	}

	// Sessão única
	if !req.Recorrente {
		a := &domain.Agendamento{
			PacienteID:                req.PacienteID,
			ProfissionalID:            profissionalID,
			ServicoID:                 req.ServicoID,
			DataHoraInicio:            inicio,
			DataHoraFim:               fim,
			ValorCombinado:            req.ValorCombinado,
			PercentualComissaoMomento: usuario.TaxaComissaoPadrao,
			Status:                    domain.StatusAgendado,
		}
		if err := h.repo.Create(r.Context(), a); err != nil {
			respondErro(w, "erro ao criar agendamento", http.StatusInternalServerError)
			return
		}
		respondJSON(w, a, http.StatusCreated)
		return
	}

	// Agendamento recorrente
	if req.TotalSessoes < 2 {
		respondErro(w, "total_sessoes deve ser >= 2 para recorrência", http.StatusBadRequest)
		return
	}
	if req.IntervaloSemanas < 1 {
		req.IntervaloSemanas = 1
	}

	groupID := uuid.NewString()
	duracao := fim.Sub(inicio)
	var lote []*domain.Agendamento

	for i := 0; i < req.TotalSessoes; i++ {
		offset := time.Duration(i*req.IntervaloSemanas) * 7 * 24 * time.Hour
		s := inicio.Add(offset)
		lote = append(lote, &domain.Agendamento{
			PacienteID:                req.PacienteID,
			ProfissionalID:            profissionalID,
			ServicoID:                 req.ServicoID,
			DataHoraInicio:            s,
			DataHoraFim:               s.Add(duracao),
			ValorCombinado:            req.ValorCombinado,
			PercentualComissaoMomento: usuario.TaxaComissaoPadrao,
			Status:                    domain.StatusAgendado,
			RecorrenciaGroupID:        &groupID,
		})
	}

	if err := h.repo.CreateLote(r.Context(), lote); err != nil {
		respondErro(w, "erro ao criar agendamentos recorrentes", http.StatusInternalServerError)
		return
	}

	respondJSON(w, map[string]any{
		"recorrencia_group_id": groupID,
		"total_criados":        len(lote),
		"agendamentos":         lote,
	}, http.StatusCreated)
}

// GET /agendamentos?de=2025-01-01&ate=2025-01-31
func (h *AgendamentoHandler) Listar(w http.ResponseWriter, r *http.Request) {
	profissionalID := middleware.GetUserID(r.Context())

	de, ate := parseFiltrosPeriodo(r)

	agendamentos, err := h.repo.ListByProfissional(r.Context(), profissionalID, de, ate)
	if err != nil {
		respondErro(w, "erro ao listar agendamentos", http.StatusInternalServerError)
		return
	}

	if agendamentos == nil {
		agendamentos = []*domain.Agendamento{}
	}

	respondJSON(w, agendamentos, http.StatusOK)
}

// PATCH /agendamentos/{id}/status
func (h *AgendamentoHandler) AtualizarStatus(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondErro(w, "id inválido", http.StatusBadRequest)
		return
	}

	var body struct {
		Status domain.StatusAgendamento `json:"status"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	validos := map[domain.StatusAgendamento]bool{
		domain.StatusAgendado: true, domain.StatusRealizado: true,
		domain.StatusFalta: true, domain.StatusCancelado: true,
	}
	if !validos[body.Status] {
		respondErro(w, fmt.Sprintf("status inválido: %s", body.Status), http.StatusBadRequest)
		return
	}

	agendamento, err := h.repo.FindByID(r.Context(), id)
	if agendamento.ProfissionalID != middleware.GetUserID(r.Context()) {
		respondErro(w, "não autorizado", http.StatusForbidden)
	}

	if err := h.repo.UpdateStatus(r.Context(), id, body.Status); err != nil {
		respondErro(w, "erro ao atualizar status", http.StatusInternalServerError)
		return
	}

	respondJSON(w, map[string]string{"status": string(body.Status)}, http.StatusOK)
}

// PATCH /agendamentos/{id}/pagamento
func (h *AgendamentoHandler) AtualizarPagamento(w http.ResponseWriter, r *http.Request) {
	id, err := strconv.Atoi(chi.URLParam(r, "id"))
	if err != nil {
		respondErro(w, "id inválido", http.StatusBadRequest)
		return
	}

	var body struct {
		Pago bool `json:"pago_pelo_paciente"`
	}
	if err := json.NewDecoder(r.Body).Decode(&body); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	agendamento, err := h.repo.FindByID(r.Context(), id)
	if agendamento.ProfissionalID != middleware.GetUserID(r.Context()) {
		respondErro(w, "não autorizado", http.StatusForbidden)
	}

	if err := h.repo.UpdatePagamento(r.Context(), id, body.Pago); err != nil {
		respondErro(w, "erro ao atualizar pagamento", http.StatusInternalServerError)
		return
	}

	respondJSON(w, map[string]bool{"pago_pelo_paciente": body.Pago}, http.StatusOK)
}

// DELETE /agendamentos/recorrencia/{groupID}
func (h *AgendamentoHandler) CancelarRecorrencia(w http.ResponseWriter, r *http.Request) {
	groupID := chi.URLParam(r, "groupID")

	if err := h.repo.CancelarRecorrencia(r.Context(), groupID); err != nil {
		respondErro(w, "erro ao cancelar recorrência", http.StatusInternalServerError)
		return
	}

	respondJSON(w, map[string]string{"mensagem": "sessões futuras canceladas"}, http.StatusOK)
}

// --- helpers ---

func parseFiltrosPeriodo(r *http.Request) (*time.Time, *time.Time) {
	parse := func(s string) *time.Time {
		if s == "" {
			return nil
		}
		t, err := time.Parse("2006-01-02", s)
		if err != nil {
			return nil
		}
		return &t
	}
	return parse(r.URL.Query().Get("de")), parse(r.URL.Query().Get("ate"))
}

func parseDate(s string) (time.Time, error) {
	return time.Parse("2006-01-02", s)
}

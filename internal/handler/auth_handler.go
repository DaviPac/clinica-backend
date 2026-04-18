package handler

import (
	"clinica-api/internal/domain"
	"clinica-api/internal/middleware"
	"clinica-api/internal/repository"
	"encoding/json"
	"net/http"
	"os"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"golang.org/x/crypto/bcrypt"
)

type AuthHandler struct {
	repo *repository.UsuarioRepository
}

func NewAuthHandler(repo *repository.UsuarioRepository) *AuthHandler {
	return &AuthHandler{repo: repo}
}

// --- DTOs (o que entra e sai da API) ---

type registrarRequest struct {
	Nome               string      `json:"nome"`
	Email              string      `json:"email"`
	Senha              string      `json:"senha"`
	Role               domain.Role `json:"role"`
	Profissao          *string     `json:"profissao"`
	TaxaComissaoPadrao *float64    `json:"taxa_comissao_padrao"`
}

type loginRequest struct {
	Email string `json:"email"`
	Senha string `json:"senha"`
}

type loginResponse struct {
	Token   string          `json:"token"`
	Usuario *domain.Usuario `json:"usuario"`
}

// POST /auth/registrar  (apenas ADMIN chama isso)
func (h *AuthHandler) Registrar(w http.ResponseWriter, r *http.Request) {
	var req registrarRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	// Validação mínima
	if req.Nome == "" || req.Email == "" || req.Senha == "" {
		respondErro(w, "nome, email e senha são obrigatórios", http.StatusBadRequest)
		return
	}
	if req.Role != domain.RoleAdmin && req.Role != domain.RoleProfissional {
		respondErro(w, "role inválido", http.StatusBadRequest)
		return
	}

	hash, err := bcrypt.GenerateFromPassword([]byte(req.Senha), bcrypt.DefaultCost)
	if err != nil {
		respondErro(w, "erro interno", http.StatusInternalServerError)
		return
	}

	taxa := 40.0
	if req.TaxaComissaoPadrao != nil {
		taxa = *req.TaxaComissaoPadrao
	}

	u := &domain.Usuario{
		Nome:               req.Nome,
		Email:              req.Email,
		Role:               req.Role,
		Profissao:          req.Profissao,
		TaxaComissaoPadrao: taxa,
	}

	if err := h.repo.Create(r.Context(), u, string(hash)); err != nil {
		respondErro(w, "e-mail já cadastrado ou erro no banco", http.StatusConflict)
		return
	}

	respondJSON(w, u, http.StatusCreated)
}

// POST /auth/login
func (h *AuthHandler) Login(w http.ResponseWriter, r *http.Request) {
	var req loginRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		respondErro(w, "corpo inválido", http.StatusBadRequest)
		return
	}

	u, senhaHash, err := h.repo.FindByEmail(r.Context(), req.Email)
	if err != nil {
		// Mensagem genérica — não revela se o e-mail existe
		respondErro(w, "credenciais inválidas", http.StatusUnauthorized)
		return
	}

	if err := bcrypt.CompareHashAndPassword([]byte(senhaHash), []byte(req.Senha)); err != nil {
		respondErro(w, "credenciais inválidas", http.StatusUnauthorized)
		return
	}

	token, err := gerarToken(u)
	if err != nil {
		respondErro(w, "erro ao gerar token", http.StatusInternalServerError)
		return
	}

	respondJSON(w, loginResponse{Token: token, Usuario: u}, http.StatusOK)
}

func (h *AuthHandler) ListarUsuarios(w http.ResponseWriter, r *http.Request) {
	resp, err := h.repo.ListAll(r.Context())
	if err != nil {
		respondErro(w, err.Error(), http.StatusInternalServerError)
	}
	respondJSON(w, resp, http.StatusOK)
}

// GET /auth/me
func (h *AuthHandler) Me(w http.ResponseWriter, r *http.Request) {
	id := middleware.GetUserID(r.Context())

	u, err := h.repo.FindByID(r.Context(), id)
	if err != nil {
		respondErro(w, "usuário não encontrado", http.StatusNotFound)
		return
	}

	respondJSON(w, u, http.StatusOK)
}

// --- helpers ---

func gerarToken(u *domain.Usuario) (string, error) {
	claims := middleware.Claims{
		UserID: u.ID,
		Role:   u.Role,
		RegisteredClaims: jwt.RegisteredClaims{
			ExpiresAt: jwt.NewNumericDate(time.Now().Add(24 * time.Hour)),
			IssuedAt:  jwt.NewNumericDate(time.Now()),
		},
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)
	return token.SignedString([]byte(os.Getenv("JWT_SECRET")))
}

func respondJSON(w http.ResponseWriter, data any, status int) {
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(status)
	json.NewEncoder(w).Encode(data)
}

func respondErro(w http.ResponseWriter, msg string, status int) {
	respondJSON(w, map[string]string{"error": msg}, status)
}

package middleware

import (
	"clinica-api/internal/domain"
	"context"
	"net/http"
	"os"
	"strings"

	"github.com/golang-jwt/jwt/v5"
)

// Chaves tipadas para o context — evita colisões com outras libs
type contextKey string

const (
	ContextKeyUserID contextKey = "user_id"
	ContextKeyRole   contextKey = "role"
)

type Claims struct {
	UserID int         `json:"user_id"`
	Role   domain.Role `json:"role"`
	jwt.RegisteredClaims
}

func Autenticar(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		authHeader := r.Header.Get("Authorization")
		if authHeader == "" || !strings.HasPrefix(authHeader, "Bearer ") {
			http.Error(w, `{"error":"token ausente"}`, http.StatusUnauthorized)
			return
		}

		tokenStr := strings.TrimPrefix(authHeader, "Bearer ")

		claims := &Claims{}
		token, err := jwt.ParseWithClaims(tokenStr, claims, func(t *jwt.Token) (any, error) {
			return []byte(os.Getenv("JWT_SECRET")), nil
		})

		if err != nil || !token.Valid {
			http.Error(w, `{"error":"token inválido"}`, http.StatusUnauthorized)
			return
		}

		// Injeta no context para os handlers consumirem
		ctx := context.WithValue(r.Context(), ContextKeyUserID, claims.UserID)
		ctx = context.WithValue(ctx, ContextKeyRole, claims.Role)

		next.ServeHTTP(w, r.WithContext(ctx))
	})
}

// ApenasAdmin bloqueia profissionais de rotas administrativas
func ApenasAdmin(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		role, ok := r.Context().Value(ContextKeyRole).(domain.Role)
		if !ok || role != domain.RoleAdmin {
			http.Error(w, `{"error":"acesso negado"}`, http.StatusForbidden)
			return
		}
		next.ServeHTTP(w, r)
	})
}

// Helpers para os handlers extraírem do context de forma limpa
func GetUserID(ctx context.Context) int {
	id, _ := ctx.Value(ContextKeyUserID).(int)
	return id
}

func GetRole(ctx context.Context) domain.Role {
	role, _ := ctx.Value(ContextKeyRole).(domain.Role)
	return role
}

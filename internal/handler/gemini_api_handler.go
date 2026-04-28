package handler

import (
	"net/http"
	"os"
)

type GeminiApiHandler struct{}

func NewGeminiApiHandler() *GeminiApiHandler {
	return &GeminiApiHandler{}
}

type GetApiKeyResponse struct {
	ApiKey string `json:"api_key"`
}

func (h *GeminiApiHandler) GetApiKey(w http.ResponseWriter, r *http.Request) {
	key := os.Getenv("GEMINI_API_KEY")
	if key == "" {
		respondErro(w, "api key nao encontrada", http.StatusInternalServerError)
		return
	}

	respondJSON(w, GetApiKeyResponse{ApiKey: key}, http.StatusCreated)
}

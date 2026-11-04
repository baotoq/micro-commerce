import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

// Aspire injects the ApiService's address through service discovery. Proxying keeps the browser
// same-origin, so the API needs no CORS policy (research R9).
const apiUrl = process.env.services__apiservice__https__0 ?? process.env.services__apiservice__http__0

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
  },
  server: {
    port: process.env.PORT ? Number(process.env.PORT) : undefined,
    strictPort: true,
    proxy: apiUrl
      ? {
          '/api': { target: apiUrl, changeOrigin: true, secure: false },
          '/health': { target: apiUrl, changeOrigin: true, secure: false },
        }
      : undefined,
  },
})

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Forward every /api request to the backend.
      '/api': {
        target: 'https://localhost:7050',
        changeOrigin: true,
        secure: false, // yerel gelistirme sertifikasi dogrulanamaz
      },
    },
  },
})

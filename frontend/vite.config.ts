import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      // lets us write imports like '@/api/http' instead of '../../api/http'
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173,
    // During development, the browser calls /api/... on the Vite server,
    // and Vite forwards it to the .NET API. Same origin = no CORS setup needed.
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})

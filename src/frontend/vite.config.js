import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Keep index.html and its inline runtime-config script intact so the
// `__VITE_API_URL__` placeholder survives the build for entrypoint substitution.
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: false,
  },
  server: {
    port: 5173,
    host: true,
    // Mirror the Nginx sidecar proxy locally so relative /api/* calls work in dev.
    proxy: {
      '/api': {
        target: 'http://localhost:5066',
        changeOrigin: true,
      },
    },
  },
});

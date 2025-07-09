import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@api': path.resolve(__dirname, 'src/api'),
      '@assets': path.resolve(__dirname, 'src/assets'),
      '@pages': path.resolve(__dirname, 'src/Pages'),
      '@components': path.resolve(__dirname, 'src/components'), // если будет
      '@': path.resolve(__dirname, 'src'),
    },
  },
  server:{
    port: 80,
    host: '0.0.0.0',
  },
})

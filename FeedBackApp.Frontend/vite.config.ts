import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import mkcert from "vite-plugin-mkcert";
import path from "path";

export default defineConfig({
  plugins: [react(), mkcert()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "src"),
    },
  },
  server: {
    https: {}, // TypeScript kompatibilis (true helyett üres objektum)
    proxy: {
      "/api": {
        target: "http://localhost:7277", // backend HTTP
        changeOrigin: true,
        secure: false,
      },
    },
  },
});

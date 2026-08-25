import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";
import mkcert from "vite-plugin-mkcert";
import path from "path";

export default defineConfig(({ mode }) => {

  const env = loadEnv(mode, process.cwd(), "");
  const tenant = env.TENANT || "";
  const suffix = tenant ? `_${tenant}` : "";

  const TENANT_SPECIFIC_KEYS = ["ENABLED_LOGIN_PROVIDERS"];

  const tenantDefines: Record<string, string> = {};
  for (const key of TENANT_SPECIFIC_KEYS) {
    const rawValue = env[`VITE_${key}${suffix}`];
    tenantDefines[`import.meta.env.VITE_${key}`] = JSON.stringify(rawValue);
  }

  return{
  plugins: [react(), mkcert()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "src"),
    },
  },
  define: tenantDefines,
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
};
});

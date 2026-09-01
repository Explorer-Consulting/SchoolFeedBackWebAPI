import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react-swc";
import mkcert from "vite-plugin-mkcert";
import path from "path";
import fs from "node:fs";

export default defineConfig(({ mode }) => {

  const env = loadEnv(mode, process.cwd(), "");
  const tenant = env.TENANT || "";

  const TENANT_SPECIFIC_KEYS = ["ENABLED_LOGIN_PROVIDERS"];

  const tenantDefines: Record<string, string> = {};
  for (const key of TENANT_SPECIFIC_KEYS) {
    const prefix = tenant ? `${tenant}_` : "";
    const rawValue = env[`VITE_${prefix}${key}`];
    tenantDefines[`import.meta.env.VITE_${key}`] = JSON.stringify(rawValue);
  }

  const resourceTenant = tenant || "EXPLORER";
  tenantDefines[`import.meta.env.VITE_DASHBOARD_IMAGE_PATH`] = JSON.stringify(`/resources/${resourceTenant}/Image.png`);
  tenantDefines[`import.meta.env.VITE_FAVICON_PATH`] = JSON.stringify(`/resources/${resourceTenant}/favicon.png`);

  const tenantConfigPath = path.resolve(__dirname, "src/tenants", resourceTenant, "description.json");
  if (!fs.existsSync(tenantConfigPath)) {
    throw new Error(
      `Ismeretlen TENANT: "${resourceTenant}". Hiányzik: src/tenants/${resourceTenant}/description.json`
    );
  }

  const tenantDescription = JSON.parse(fs.readFileSync(tenantConfigPath, "utf-8"));
  const institutionName: string = tenantDescription.institutionName;

  tenantDefines[`import.meta.env.VITE_INSTITUTION_NAME`] = JSON.stringify(institutionName);
  tenantDefines[`import.meta.env.VITE_TENANT_DESCRIPTION`] = JSON.stringify(JSON.stringify(tenantDescription));
 
  return{
  plugins: [
    react(),
    mkcert(),
    {
     name: "html-tenant-favicon",
     transformIndexHtml(html){
      return html
        .replace(/__FAVICON_PATH__/g, `/resources/${resourceTenant}/favicon.png`)
        .replace(/__INSTITUTION_NAME__/g, institutionName);
     } 
    },
  ],
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

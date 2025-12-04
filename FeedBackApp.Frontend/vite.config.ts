import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { componentTagger } from "lovable-tagger";
import devcert from "devcert";

export default defineConfig(async ({ mode }) => {
  const ssl = await devcert.certificateFor('localhost');

  return {
    server: {
      host: "::",
      port: 8080,
      https: {
        key: ssl.key,
        cert: ssl.cert,
      },
    },
    plugins: [
      react(),
      mode === "development" && componentTagger(),
    ].filter(Boolean),
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "./src"),
      },
    },
  };
});

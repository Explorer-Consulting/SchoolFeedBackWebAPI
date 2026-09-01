/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_GOOGLE_CLIENT_ID: string
  readonly VITE_API_BASE_URL: string
  readonly VITE_TENANT_DESCRIPTION: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
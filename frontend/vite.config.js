import { defineConfig } from "vite";
import { fileURLToPath, URL } from "node:url";

export default defineConfig({
  server: {
    port: 3000,
    proxy: {
      "/api":  { target: "https://localhost:7146", changeOrigin: true, secure: false },
      "/hubs": { target: "https://localhost:7146", ws: true, secure: false }
    }
  },
  build: {
    rollupOptions: {
      input: {
        index: fileURLToPath(new URL("./index.html", import.meta.url)),
        login: fileURLToPath(new URL("./login.html", import.meta.url))
      }
    }
  }
});
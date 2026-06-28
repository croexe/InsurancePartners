import { defineConfig } from "vite";

export default defineConfig({
    server: {
        port: 3000,
        proxy: {
            "/api": { target: "https://localhost:7146", changeOrigin: true, secure: false },
            "/hubs": { target: "https://localhost:7146", ws: true, secure: false }
        }
    }
});
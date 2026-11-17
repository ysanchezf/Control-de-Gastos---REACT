import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
export default defineConfig({
  publicDir: false,
  base: process.env.NODE_ENV === "production" ? "/build/" : "/",
  build: { outDir: "wwwroot/build", emptyOutDir: true },
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      "/images": "http://127.0.0.1:5080",
      "/api": "http://127.0.0.1:5080",
      "/cuenta": "http://127.0.0.1:5080",
    },
  },
});

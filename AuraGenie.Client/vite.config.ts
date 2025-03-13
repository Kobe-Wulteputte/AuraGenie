import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import vueDevTools from "vite-plugin-vue-devtools";

// https://vite.dev/config/
export default defineConfig({
  build: {
    target: "esnext",
  },
  plugins: [
    vue({
      template: {
        compilerOptions: {
          isCustomElement: (tagName) => {
            return (
              tagName === "vue-advanced-chat" || tagName === "emoji-picker"
            );
          },
        },
      },
    }),
    vueDevTools(),
  ],
  optimizeDeps: {
    include: ["vue-advanced-chat"],
  },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
});

import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { VitePWA } from "vite-plugin-pwa";

// FHSMS PWA - the same build serves hotel customers, hotel agents, and farmer
// agents; role-based routing happens inside the app, not at the build level,
// matching the "one backend, one set of business rules, many channels" principle.
export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: "autoUpdate",
      manifest: {
        name: "FHSMS - Farm-Hotel Supply Management",
        short_name: "FHSMS",
        description: "Farm-Hotel Supply Management System",
        theme_color: "#1C3B32",
        background_color: "#F7F6F1",
        display: "standalone",
        icons: [
          { src: "icon-192.png", sizes: "192x192", type: "image/png" },
          { src: "icon-512.png", sizes: "512x512", type: "image/png" }
        ]
      }
    })
  ],
  server: {
    port: 5173
  }
});

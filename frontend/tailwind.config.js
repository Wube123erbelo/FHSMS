/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        // AgriLink palette: an evergreen/harvest identity, deliberately not the
        // cream+terracotta or near-black+neon defaults - this is a working
        // operations tool for farms and hotels, not a marketing page.
        // Full contiguous scales below - fill in every step (not just the
        // handful first used) so any shade referenced anywhere in the app
        // (e.g. evergreen-200/300/500/800, wheat-200/300/700) actually
        // exists; Tailwind silently drops the class for an undefined shade
        // rather than erroring, which is a easy gap to miss visually.
        evergreen: {
          50: "#EEF3EF",
          100: "#D6E3D9",
          200: "#BBD1C1",
          300: "#96B7A2",
          400: "#3C6B54",
          500: "#2F5A45",
          600: "#264A3A",
          700: "#1C3B32",
          800: "#172E27",
          900: "#12241E",
          950: "#0B1712"
        },
        wheat: {
          50: "#FBF8EF",
          100: "#F3EAC9",
          200: "#EAD9A0",
          300: "#DFC670",
          400: "#C9A227",
          500: "#B38F22",
          600: "#9C7D1D",
          700: "#7A6217"
        },
        clay: {
          50: "#FBF0ED",
          100: "#F2D5CC",
          200: "#E4AC9A",
          300: "#D28268",
          400: "#BD6448",
          500: "#A64B3B",
          600: "#8B3C2F",
          700: "#6E2F25"
        },
        ink: {
          900: "#1B1B18",
          600: "#4A4A44",
          300: "#8B8B82"
        },
        canvas: "#F7F6F1"
      },
      fontFamily: {
        display: ["Fraunces", "ui-serif", "Georgia", "serif"],
        sans: ["Inter", "ui-sans-serif", "system-ui", "sans-serif"],
        mono: ["IBM Plex Mono", "ui-monospace", "monospace"]
      },
      boxShadow: {
        card: "0 1px 2px rgba(28, 59, 50, 0.06), 0 1px 8px rgba(28, 59, 50, 0.05)"
      },
      keyframes: {
        // Landing page hero: slow ambient drift for the "produce" bubbles
        // behind the headline, and a truck that drives the width of the
        // road on a loop - see FieldScene in LandingPage.tsx.
        floatSlow: {
          "0%, 100%": { transform: "translateY(0) translateX(0)" },
          "50%": { transform: "translateY(-14px) translateX(6px)" }
        },
        floatSlower: {
          "0%, 100%": { transform: "translateY(0) translateX(0)" },
          "50%": { transform: "translateY(10px) translateX(-8px)" }
        },
        drive: {
          "0%": { left: "-10%" },
          "100%": { left: "104%" }
        }
      },
      animation: {
        "float-slow": "floatSlow 7s ease-in-out infinite",
        "float-slower": "floatSlower 9s ease-in-out infinite",
        drive: "drive 22s linear infinite"
      }
    }
  },
  plugins: []
};

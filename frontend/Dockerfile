# syntax=docker/dockerfile:1

# Optional: only needed if you'd rather run the frontend as its own Docker
# web service. Render's native "Static Site" service type (build command
# `npm run build`, publish directory `dist`) is simpler and free, and is
# what render.yaml uses by default - see DEPLOYMENT.md. Keep this around for
# anyone who wants everything containerized, or is hosting somewhere other
# than Render.

FROM node:20-alpine AS build
WORKDIR /app

# No package-lock.json is committed in this repo yet, so `npm install` is
# used here rather than `npm ci` (which requires one and fails without it).
# Committing a lockfile and switching this to `npm ci` is worth doing before
# relying on this for real deploys - it makes builds reproducible instead of
# picking up whatever the latest matching versions are at build time.
COPY frontend/package.json ./
RUN npm install

COPY frontend/ ./

# Vite bakes VITE_* variables into the JS bundle at build time, not at
# container runtime - so unlike the API's env vars, this one has to be
# passed as a Docker build ARG (`docker build --build-arg
# VITE_API_BASE_URL=https://your-api.onrender.com/api .`), not a normal
# `docker run -e`.
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
# Files in /etc/nginx/templates/*.template are envsubst'd into
# /etc/nginx/conf.d/ at container startup by the base image's own
# entrypoint - this is how the container ends up listening on whatever port
# Render assigns at runtime (a plain conf.d/default.conf can't do this,
# since it's static and nginx doesn't read env vars on its own).
COPY frontend/nginx.conf.template /etc/nginx/templates/default.conf.template
# Local `docker run` without -e PORT=... still works via this default;
# Render always sets its own PORT, which wins at container startup.
ENV PORT=80

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

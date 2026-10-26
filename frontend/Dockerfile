# ---------- 1. Build: compile the Vue app into static files ----------
FROM node:22-alpine AS build
WORKDIR /app

# Install packages first, so Docker can cache this step when only code changes
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---------- 2. Serve: nginx serves the files and forwards /api to the .NET API ----------
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80

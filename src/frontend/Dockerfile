FROM node:24-alpine AS build
WORKDIR /app

# Copy manifest first so Docker can cache the npm install layer.
COPY package.json ./
# Use npm install so the build works whether or not a lockfile is committed.
RUN npm install --no-audit --no-fund

COPY . .
RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Runtime placeholder injection: the official nginx image runs *.sh files
# in /docker-entrypoint.d/ before starting nginx, so this replaces the
# __VITE_API_URL__ token inside index.html with the runtime VITE_API_URL
# value at container start. No secrets are baked into image layers.
COPY docker-entrypoint.sh /docker-entrypoint.d/40-inject-runtime-env.sh
RUN chmod +x /docker-entrypoint.d/40-inject-runtime-env.sh

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

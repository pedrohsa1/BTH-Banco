FROM node:22-alpine AS build
WORKDIR /web
COPY src/Web/package.json ./
RUN npm install
COPY src/Web/ .
RUN npm run build

FROM nginx:1.27-alpine
COPY src/Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /web/dist /usr/share/nginx/html
EXPOSE 80

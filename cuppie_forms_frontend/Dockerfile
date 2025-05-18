# 1. Сборка на node
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

# 2. Хостинг на nginx
FROM nginx:alpine

# Кладем кастомный конфиг (если нужен SPA fallback)
COPY default.conf /etc/nginx/conf.d/default.conf

# Кладем билд
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

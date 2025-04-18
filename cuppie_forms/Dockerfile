# 1. Используем официальный Node.js образ
FROM node:18-alpine AS build

# 2. Рабочая директория
WORKDIR /app

# 3. Копируем зависимости и устанавливаем их
COPY cuppie_forms/package*.json ./
RUN npm install

# 4. Копируем остальной код и собираем
COPY cuppie_forms/ ./
RUN npm run build

# 5. Используем nginx для обслуживания статики
FROM nginx:alpine

# 6. Копируем собранное приложение
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
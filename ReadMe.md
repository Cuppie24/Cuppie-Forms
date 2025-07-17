# Cuppie Forms

---

## 📋 Содержание
1. [Описание проекта](#описание-проекта)  
2. [Особенности (Features)](#особенности-features)  
3. [Стек технологий](#стек-технологий)  
4. [Архитектура](#архитектура)  
5. [Установка и запуск](#установка-и-запуск)
7. [Использование API](#использование-api)  
11. [Лицензия](#лицензия)

---

## Описание проекта
Cuppie Forms - это готовое решение для аутентификации и авторизации пользователей, построенное на современных технологиях с акцентом на безопасность и масштабируемость.
 
### Для чего подходит:

- Стартапы и MVP: быстрый старт с готовой системой авторизации
- Корпоративные приложения: надежная основа для внутренних систем
- Микросервисная архитектура: независимый сервис аутентификации
- Обучение: изучение Clean Architecture и JWT на практике

### Сценарии использования:

- E-commerce платформы: регистрация покупателей, управление профилями
- CRM/ERP системы: авторизация сотрудников с разными ролями
- Образовательные платформы: разграничение доступа студентов и преподавателей
- SaaS приложения: мультитенантная авторизация
- Мобильные приложения: backend для iOS/Android приложений

### Ключевые преимущества:

✅ Готовое решение "из коробки"
✅ Современные стандарты безопасности (JWT + Refresh tokens)
✅ Масштабируемая архитектура (Clean Architecture)
✅ Docker-ready для легкого деплоя
✅ Подробная документация и примеры

---

## Особенности (Features)
- Clean Architecture: разделение на API, Application, Infrastructure, Domain, Tests
- Аутентификация и авторизация с помощью Access JWT Token & Refresh Token   
- Хранение паролей в хэшированном виде - SHA‑256 + соль   
- Безопасная ротация refresh‑токенов с автоматическим отзывом предыдущих при каждом обновлении

---

## Стек технологий
- **Backend**: ASP.NET Core, Entity Framework Core  
- **База данных**: PostgreSQL 16
- **Тестирование**: xUnit  
- **Инфраструктура**: Traefik (proxy), Docker (Compose)  
- **Frontend**: Vite + React + TypeScript (минимум для демонстрации возможностей)

---

## Архитектура
```
CuppieForms/
├── cuppie_forms_backend/             ← Backend (по Clean Architecture)
│   ├── Cuppie.Api/                   ← Слой API (Web)
│   ├── Cuppie.Application/           ← Слой бизнес-логики и интерфейсов
│   ├── Cuppie.Infrastructure/        ← Доступ к данным, реализации сервисов
│   ├── Cuppie.Domain/                ← Доменные сущности (ядро)
│   └── Cuppie.Tests/                 ← Тесты
├── cuppie_forms_front/              ← Frontend-приложение
└── compose.yaml                     ← Docker Compose конфигурация
```

---

## Установка и запуск
### Скачать и установить 
[.NET SDK 9](https://dotnet.microsoft.com/en-us/download)
[Docker desktop](https://www.docker.com/get-started/)

### Клонировать репозиторий:
   ```bash
   git clone https://github.com/Cuppie24/Cuppie-Forms.git
   cd Cuppie-Forms
   ```

### Восстанавливаем зависимости
В директории `cuppie_forms_backend`
```
dotnet restore
```

### Настройка JWT:
В файле `.env` в корне проекта вставь свои произвольные параметры для JWT
`JWT__Key` - секретный ключ для подписи токенов
`JWT__Issuer` - Кем выдан токен 
`JWT__Audience` - Кому предназначен токен
`JWT__AccessTokenExpiresInMinutes`- Время жизни JWT токена в минутах
`JWT__RefreshTokenExpiresInMinutes` - Время жизни refresh токена в минутах

---

### Настройка БД:
Настройка строки подключения в файле `.env`
```
DATABASE_CONNECTION_STRING=Host=db;Port=<Your port>;Database=<Your database name>;Username=<Your username>;Password=<your password>
```
`Host` - адрес для обращения к бд. В нашем случае название Docker контейнера `db`
`Database` - произвольное название БД. Должен совпадать со значением `POSTGRES_DB`
`Username` - прозивольное имя. Совпадает с `POSTGRES_USER`
`Password` - придумай пароль. Совпадает с `POSTGRES_PASSWORD`

Далее нужно сделать миграцию БД и обновить ее. Для этого перейди в каталог `cuppie_forms_backend` и выполни следующие команды
```
//создание миграции
dotnet ef migrations add InitialMigration --project ./src/Cuppie.Infrastructure --startup-project ./src/Cuppie.Api --context CuppieDbContext --output-dir Data/Migrations
//обновление БД
dotnet ef database update --project ./src/Cuppie.Infrastructure --startup-project ./src/Cuppie.Api --context CuppieDbContext 
``` 

---
### Настройка доменов
В файле `compose.yaml` в строках подставляем свои домены
```
- "traefik.http.routers.front.rule=Host(`cuppie.cup`)"
- "traefik.http.routers.api.rule=Host(`cuppie.api`)"
- "traefik.http.routers.dashboard.rule=Host(`cuppie.dashboard`)"
```
в моем случае это `cuppie.cup` для фронта, `cuppie.api` для бэкенда и `cuppie.dashboard` для Traefik dashboard. 
Если же развертывание локальное и у тебя нет доменов, в `hosts` ставим нужные нам адреса. Пример:
```
127.0.0.1   cuppie.cup
127.0.0.1   cuppie.api
127.0.0.1   cuppie.dashboard      
```
`cuppie.cup` - front
`cuppie.api` - back
`cuppie.dashboard` - Traefik dashboard, для мониторинга подключений

После этого в файле `.env` в строку `FRONTEND_API_URL` прописываем адрес бэкенда, а чтобы сервер разрешал нашему фронту делать запросы, добавим адрес фронтенда в список разрешенных хостов `CORS` в строке `CORS_ALLOWED_ORIGINS`. Если таких клиентов несколько, то добавляем их через запятую:
```
# Frontend API URL
FRONTEND_API_URL=https://cuppie.api
# CORS allowed origins separated by commas
CORS_ALLOWED_ORIGINS=https://cuppie.cup,https://other.client
```

### SSL сертификат
Положи свои файлы сертификата в папку `/traefik/certs` и укажи путь к ним в файле `/traefik/tls_dynamic.yml`:
```
tls:
  certificates:
    - certFile: /certs/local.crt
      keyFile: /certs/local.key
```
docker-compose up --build
Перейти в браузере:

API: https://cuppie.api/api/health
Frontend: https://cuppie.cup/

# Использование API

## Базовая информация

API построено на основе ASP.NET Core и использует JWT токены для аутентификации. Все токены передаются через HTTP-only cookies для обеспечения безопасности.

**Базовый URL:** `https://cuppie.api/api` или другой, который ты настроил

## Аутентификация

### Регистрация пользователя

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "password123"
}
```

**Требования к данным:**
- `username`: 1-30 символов, только буквы, цифры, дефис и подчеркивание
- `email`: валидный email адрес (до 100 символов)
- `password`: минимум 6 символов

**Ответ:**
```json
{
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com"
  },
  "jwtToken": {
    "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
    "expiresInMinutes": 60
  },
  "refreshToken": {
    "token": "refresh_token_here",
    "expiresInMinutes": 10080
  }
}
```

### Авторизация пользователя

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "password123"
}
```

**Ответ:** аналогичен регистрации.

### Обновление токена

```http
POST /api/auth/refresh
Cookie: refreshToken=<refresh_token>
```

Требует наличия refresh токена в cookies для получения нового JWT токена.

### Получение информации о текущем пользователе

```http
GET /api/auth/me
Cookie: jwt=<jwt_token>
```

Требует наличия JWT токена в cookies.

**Ответ:**
```json
{
  "id": 1,
  "username": "john_doe",
  "email": "john@example.com"
}
```

### Выход из системы

```http
POST /api/auth/logout
```

Удаляет токены из cookies.

### Проверка состояния сервиса

```http
GET /api/auth/health
```

**Ответ:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Защищенные эндпоинты

### Главная страница

```http
GET /api/pages/home/get
Cookie: jwt=<jwt_token>
```

Требует авторизации через JWT токен в cookies.

**Ответ:**
```json
{
  "message": "Пароль от wifi на даче: qwert123"
}
```

## Работа с cookies

API автоматически устанавливает следующие cookies при успешной аутентификации:

- `jwt` - JWT токен для авторизации
- `refreshToken` - токен для обновления JWT

**Параметры cookies:**
- `HttpOnly: true` - недоступны для JavaScript
- `SameSite: None` - разрешают cross-site запросы
- `Secure: true` - только через HTTPS

## Коды ответов

- `200 OK` - успешная операция
- `400 Bad Request` - ошибка валидации или обработки
- `401 Unauthorized` - неверные учетные данные или токен
- `404 Not Found` - ресурс не найден
- `409 Conflict` - конфликт данных (например, пользователь уже существует)

## Примеры использования

### JavaScript (fetch)

```javascript
// Регистрация
const response = await fetch('/api/auth/register', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  credentials: 'include', // для работы с cookies
  body: JSON.stringify({
    username: 'john_doe',
    email: 'john@example.com',
    password: 'password123'
  })
});

const data = await response.json();

// Получение защищенных данных
const homeResponse = await fetch('/api/pages/home/get', {
  credentials: 'include'
});

const homeData = await homeResponse.json();
```

### curl

```bash
# Регистрация
curl -X POST https://your-domain.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"john_doe","email":"john@example.com","password":"password123"}' \
  -c cookies.txt

# Использование защищенного эндпоинта
curl -X GET https://your-domain.com/api/pages/home/get \
  -b cookies.txt
```

## Обработка ошибок

API возвращает структурированные ошибки:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Ошибка регистрации",
  "status": 400,
  "detail": "Пользователь с таким именем уже существует"
}
```

При работе с API рекомендуется всегда проверять код ответа и обрабатывать возможные ошибки.

## Обработка ошибок

API возвращает структурированные ошибки:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Ошибка регистрации",
  "status": 400,
  "detail": "Пользователь с таким именем уже существует"
}
```

При работе с API рекомендуется всегда проверять код ответа и обрабатывать возможные ошибки.

# Лицензия


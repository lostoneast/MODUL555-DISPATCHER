# Модуль-555 Диспетчер

Система диспетчеризации производства и логистики (экосистема СУ-555).  
Стек: **ASP.NET Core 10** + **PostgreSQL** + **Keycloak** + **React (Vite, Ant Design)**.

- **Прод:** http://92.53.107.16:8080/ (рядом с LIMS на `:80`)
- **Каталог на сервере:** `/opt/modul555-dispatcher`

---

## Содержание

1. [Структура](#структура)
2. [Требования](#требования)
3. [Локальная отладка](#локальная-отладка)
4. [Docker Compose (локально / прод)](#docker-compose)
5. [Публикация на сервер](#публикация-на-сервер)
6. [Авторизация](#авторизация)
7. [Демо-данные](#демо-данные)
8. [Миграции EF Core](#миграции-ef-core)
9. [Полезные команды](#полезные-команды)

---

## Структура

```
MODUL555-DISPATCHER/
├── docker-compose.yml
├── Dockerfile                 # API
├── .env.example
├── src/Modul555.Dispatcher.Api/
│   ├── Controllers/           # Auth, Catalogs, Admin, DemoData
│   ├── DispatcherApp.Domain/
│   ├── DispatcherApp.Application/
│   └── DispatcherApp.Infrastructure/Persistence/
└── web/                       # Vite + React
    ├── Dockerfile
    └── nginx.conf             # SPA + proxy /api → api:8080
```

---

## Требования

| Компонент | Версия / заметка |
|-----------|------------------|
| .NET SDK | 10.x |
| Node.js | 22+ (для фронта) |
| Docker + Compose | для полного стека / деплоя |
| PostgreSQL | 17 (в compose) или локальный |
| Keycloak | общий realm `stroy-company` |

---

## Локальная отладка

### 1. База данных

Вариант A — только Postgres из compose:

```bash
docker compose up -d postgres
```

Строка подключения по умолчанию в `appsettings.json`:

```
Host=localhost;Port=5432;Database=modul555_dispatcher;Username=dispatcher;Password=dispatcher
```

Если Postgres уже занял `:5432` (или используете LIMS на `:5433`), поправьте `ConnectionStrings:Default` в `appsettings.Development.json` или переменную окружения.

Вариант B — свой локальный PostgreSQL: создайте БД/пользователя и обновите connection string.

Миграции применяются **автоматически** при старте API (`Database.MigrateAsync()`).

### 2. API

```bash
cd src/Modul555.Dispatcher.Api
dotnet run --launch-profile http
```

- URL: http://localhost:5123  
- Профиль `https`: https://localhost:7060 и http://localhost:5123  

### 3. Frontend

```bash
cd web
npm install
npm run dev
```

- URL: http://localhost:5174  
- Vite проксирует `/api` → `http://localhost:5123` (`web/vite.config.ts`)

### 4. Отладка в IDE

1. Запустите Postgres.
2. Запустите API (F5 / `dotnet run` или launch profile `http`).
3. Запустите `npm run dev` в `web/`.
4. Откройте http://localhost:5174 , войдите учётками Keycloak.

Брейкпоинты ставятся в C#-проектах API. Для фронта — обычный DevTools / React DevTools.

### Типичные проблемы при отладке

| Симптом | Что проверить |
|---------|----------------|
| API не стартует / ошибка БД | Postgres доступен, connection string, пользователь/БД существуют |
| 401 на `/api/...` | Нет Bearer-токена или Keycloak недоступен |
| Login «Неверный логин или пароль» | Учётка/пароль в Keycloak realm `stroy-company` |
| Новый завод не виден в select линий | Обновите страницу; кэш lookups сбрасывается после create/update |
| CORS | В dev запросы идут через Vite proxy — CORS не нужен |

---

## Docker Compose

Одна команда поднимает Postgres + API + nginx (UI):

```bash
cp .env.example .env   # при необходимости отредактируйте
docker compose up -d --build
```

| Сервис | Контейнер | Порт |
|--------|-----------|------|
| web | `modul555-dispatcher-web` | хост `${HTTP_PORT:-8080}` → 80 |
| api | `modul555-dispatcher-api` | внутри сети `:8080` (не публикуется) |
| postgres | `modul555-dispatcher-db` | внутри сети `:5432` |

Локально UI: http://localhost:8080/

Остановка:

```bash
docker compose down
```

Сброс тома БД (полная потеря данных):

```bash
docker compose down -v
```

---

## Публикация на сервер

Прод-хост (как у LIMS): **`92.53.107.16`**.  
LIMS занимает **`:80`**, Диспетчер — **`:8080`**.

### Выкладка

```bash
# с машины разработчика
rsync -az --delete \
  --exclude '.git' \
  --exclude 'node_modules' \
  --exclude 'web/node_modules' \
  --exclude 'web/dist' \
  --exclude '**/bin' \
  --exclude '**/obj' \
  --exclude '.cursor' \
  --exclude '.env' \
  --exclude '.env.production' \
  ./ root@92.53.107.16:/opt/modul555-dispatcher/

# на сервере: .env (секреты не в git)
scp .env.production root@92.53.107.16:/opt/modul555-dispatcher/.env

ssh root@92.53.107.16 'cd /opt/modul555-dispatcher && docker compose up -d --build'
```

Пример `.env` на сервере (см. также `.env.example`):

```env
HTTP_PORT=8080
POSTGRES_DB=modul555_dispatcher
POSTGRES_USER=dispatcher
POSTGRES_PASSWORD=<секрет>
KEYCLOAK_AUTHORITY=http://72.56.252.95:8080/realms/stroy-company
KEYCLOAK_AUDIENCE=
KEYCLOAK_TOKEN_CLIENT_ID=admin-cli
```

После деплоя: http://92.53.107.16:8080/

### Обновление

Повторный `rsync` + `docker compose up -d --build`.  
Файл `.env` на сервере не затирайте (`--exclude '.env'`).

---

## Авторизация

- Identity Provider: **Keycloak**, realm `stroy-company`
- API: JWT Bearer + proxy login/refresh (`/api/auth/login`, `/refresh`, `/me`)
- Роли realm: `admin`, `manager`, `tester`, `developer`
- Админка и демо-кнопки: только **`admin`** / **`manager`**
- Учётки **не** хранятся в БД Диспетчера — сброс демо-данных их не трогает

---

## Демо-данные

В UI: **Администрирование** → «Сбросить данные» / «Заполнить демо-данными».

API (нужна роль admin/manager):

```http
POST /api/admin/demo/clear
POST /api/admin/demo/seed
```

Набор seed задаётся явным кодом в  
`src/Modul555.Dispatcher.Api/Controllers/DemoDataController.cs` → метод **`SeedCoreAsync`**.  
Править данные для тестов — там (не через Excel).

`seed` требует пустую БД (иначе сначала `clear`).

---

## Миграции EF Core

При старте API миграции применяются сами.  
Новую миграцию после изменения модели создаёт разработчик локально и коммитит:

```bash
dotnet ef migrations add <Name> \
  --project src/Modul555.Dispatcher.Api/DispatcherApp.csproj \
  --output-dir DispatcherApp.Infrastructure/Persistence/Migrations
```

Не используйте `EnsureCreated()`. На сервере достаточно перезапуска контейнера API / `compose up`.

---

## Полезные команды

```bash
# сборка API
dotnet build src/Modul555.Dispatcher.Api/DispatcherApp.csproj

# сборка фронта
cd web && npm run build

# логи на сервере
ssh root@92.53.107.16 'cd /opt/modul555-dispatcher && docker compose logs -f --tail=100 api'

# статус контейнеров
ssh root@92.53.107.16 'cd /opt/modul555-dispatcher && docker compose ps'
```

### Порты (сводка)

| Назначение | Порт |
|------------|------|
| API local | 5123 |
| Vite local | 5174 |
| Docker / prod UI | 8080 |
| LIMS (другой проект) | 80 |
| Keycloak | 72.56.252.95:8080 |

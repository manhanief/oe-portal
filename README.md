# Kopitiam Kita: restaurant ordering system

A restaurant ordering system with three parts:

- **Customers** browse the menu, order for dine-in or take-away, and follow their order on a live ticket page.
- **Kitchen staff** move orders through New → Preparing → Ready on a kitchen board.
- **Admins** manage the menu, categories, tables and staff accounts, and see daily sales.

| Layer | Technology |
|---|---|
| Database | Oracle AI Database 26ai Free (Docker) |
| API | ASP.NET Core (.NET 10) Web API, EF Core + Oracle provider, FluentValidation |
| Website | Vue 3 + TypeScript, PrimeVue, Pinia, Vue Router (Vite) |
| Hosting | Docker Compose: Oracle + API + nginx |

## Run everything with Docker

Requirements: Docker Desktop with at least 4 GB of memory for Docker.

```bash
cp .env.example .env        # then change the passwords in .env
docker compose up -d --build
```

The first start takes a few minutes (Oracle starts, then the database tables are created automatically).

| What | Address |
|---|---|
| Customer menu | http://localhost:8080 |
| Staff login | http://localhost:8080/staff (user and password from `.env`) |
| API health | http://localhost:8080/health |
| Oracle (SQL Developer / VS Code) | `localhost:1522/FREEPDB1`, user `resto` |

Useful commands:

```bash
docker compose ps                 # status of each container
docker compose logs -f api        # API log
docker compose down               # stop (data is kept)
docker compose down -v            # stop AND delete all data
```

## Run for development (hot reload)

```bash
docker compose up -d oracle-db db-init     # only the database

cd backend/Resto.Api
dotnet user-secrets set "ConnectionStrings:Oracle" "User Id=resto;Password=Resto_2026;Data Source=localhost:1522/FREEPDB1"
dotnet user-secrets set "SeedAdmin:Username" "admin"
dotnet user-secrets set "SeedAdmin:Password" "Admin_2026"
dotnet run --urls http://localhost:5080    # Swagger: http://localhost:5080/swagger

cd ../../frontend
npm install
npm run dev                                # http://localhost:5173
```

## Project structure

```
db/                 SQL scripts: schema, sample data, Docker init script
backend/Resto.Api/
  Entities/         C# classes for the tables
  Data/             RestoDbContext (EF Core mapping to Oracle)
  Dtos/             Request and response shapes
  Validators/       Input rules (FluentValidation)
  Services/         Business rules (ordering, pricing, status changes)
  Controllers/      API endpoints (Admin/ = admin-only)
  Auth/             Login policies and first-admin setup
  Common/           Error handling, validation filter, settings
frontend/src/
  views/            Pages (customer) and views/staff/ (kitchen + admin)
  components/       Reusable UI pieces
  stores/           Cart and login state (Pinia)
  api/              Functions that call the API
```

## Security notes

- Staff log in with an HttpOnly, SameSite=Strict cookie; passwords are hashed (PBKDF2).
- Login is limited to 5 attempts per minute per IP address.
- Every admin and kitchen endpoint checks the user's role on the server.
- Prices are always taken from the database, never from the browser.
- Secrets live in `.env` / user-secrets, never in the code.

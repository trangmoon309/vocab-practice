# decisions.md

Append-only log. Orchestrator writes here after each gate.

---

## 2026-06-13 — GATE 1 approved: Architecture

- 5 entities: User, WordSet, Word, GameSession, ChatMessage
- `Level` and `Type` on Word stored as `string?` on entity; enum validation at Application layer
- `WordSet → GameSession` FK uses `DeleteBehavior.Restrict` (preserving session history if word set deleted)
- All Fluent configs use `IEntityTypeConfiguration<T>`, no data annotations
- Guid PKs: `ValueGeneratedNever()` on all entities
- UTC timestamps via `.HasDefaultValueSql("NOW()")`
- 5 repository interfaces + `IAiChatService` + 3 domain exceptions approved
- **Next:** .NET Expert creates `VocaPlay.Domain` project on disk

---

## 2026-06-13 — Architecture correction: interface placement

- **Decision:** Repository interfaces (`IUserRepository`, `IWordSetRepository`, `IWordRepository`, `IGameSessionRepository`, `IChatRepository`) and `IAiChatService` moved from `VocaPlay.Domain` → `VocaPlay.Application/Common/Interfaces/`
- **Why:** Repository/service interfaces orchestrate use cases (fetch data, call external services) — that is Application layer responsibility, not domain business logic. Domain stays pure: entities, enums, and exceptions only.
- **Rule:** Domain layer = core business concepts with zero external dependencies. Application layer = use-case orchestration contracts.
- **Impact:** Infrastructure implementations will reference `VocaPlay.Application.Common.Interfaces.*` namespaces, not `VocaPlay.Domain.Interfaces.*`

---

## 2026-06-13 — GATE 2 approved: Domain layer on disk

- `VocaPlay.Domain` builds with zero NuGet dependencies ✅
- Interface relocation to Application layer approved ✅
- **Next:** .NET Expert builds full Application layer (handlers, DTOs, DI extension)

---

## 2026-06-13 — GATE 3 approved: Application layer on disk

- All 18 CQRS handlers implemented across Auth, WordSets, Words, Game, Chat
- `IPasswordHasher` and `IJwtTokenService` added to Application interfaces (keep Application free of BCrypt/JWT NuGet)
- `BulkAddWordsCommandHandler` deduplicates by English term (case-insensitive, in-memory HashSet per call)
- `SendChatMessageCommandHandler` parses `%%ACTION%%...%%END%%` block, strips from reply before persisting
- `Microsoft.Extensions.DependencyInjection.Abstractions` added to Application (only external NuGet allowed)
- **Next:** .NET Expert builds Infrastructure layer (EF repos, OpenAI, JWT, BCrypt, migrations)

---

## 2026-06-14 — GATE 4 approved: Infrastructure layer on disk

- `VocaPlay.Infrastructure` builds with 0 errors against Domain + Application
- 5 EF Core Fluent configs, `AppDbContext`, design-time `AppDbContextFactory` (local Postgres placeholder connection string, dev-only)
- 5 repositories implementing Application repo interfaces; `ChatRepository.GetByUserIdAsync` takes last N messages then re-orders oldest-first for AI context window
- `BcryptPasswordHasher` (BCrypt.Net-Next), `JwtTokenService` (access + refresh tokens, HMAC-SHA256, `purpose=refresh` claim check)
- `OpenAiChatService` implements `IAiChatService` using OpenAI SDK v2 `ChatClient`; system prompt copied verbatim from spec.md §6
- `AddInfrastructure()` registers DbContext, `JwtSettings`/`OpenAiSettings` via `IOptions<T>` (bound from configuration — no hardcoded secrets), all repos, auth services, AI service, and a custom-factory registration for `SendChatMessageCommandHandler` (injects configured `MaxHistoryMessages`)
- Added NuGet packages: `Microsoft.Extensions.Options.ConfigurationExtensions`, `Microsoft.Extensions.Configuration.Binder` (required for `Configure<T>(IConfigurationSection)` and `IConfiguration.GetValue<T>()`)
- Reviewer secrets check: no hardcoded `Jwt:Secret` / `OpenAI:ApiKey` found — both read via `IOptions<T>` from configuration ✅
- **Deferred:** `dotnet ef migrations add InitialCreate` could not be run in this dev environment — only .NET 10 runtime is installed and `dotnet-ef` requires a matching .NET 8 runtime for the net8.0-targeted `AppDbContext`. Initial migration will be generated in Phase 5/CI (or after installing .NET 8 runtime) before first deploy.
- **Next:** .NET Expert builds API layer (controllers, middleware, `CurrentUserService`, `Program.cs`, Dockerfile, appsettings.json)

---

## 2026-06-14 — GATE 5 pending approval: API layer on disk

- `VocaPlay.Api` builds with 0 errors; added `VocaPlay.sln` referencing all 4 projects
- 5 controllers (Auth, WordSets, Words, Game, Chat) — manual CQRS dispatch via injected handlers, all routes under `/api`, all `[Authorize]` except `/auth/*`
- Added missing `GetGameSessionsQuery`/Handler to Application (`GET /game/sessions`) — not present after Phase 3, required by spec.md §5
- `/auth/logout` returns `204` with no backend logic (stateless JWT, no revocation list in v1)
- `ErrorHandlingMiddleware` maps `NotFoundException`→404, `ForbiddenException`→403, `ValidationException`→400 (with `errors[]`), `UnauthorizedAccessException`→401, else 500 → all as `{ message, statusCode }` per spec.md §10
- `CurrentUserService` reads `sub` claim from JWT via `IHttpContextAccessor`
- `Program.cs` wires `AddApplication()` + `AddInfrastructure()`, JWT bearer auth (HMAC-SHA256, validates issuer/audience/lifetime), CORS (`Cors:AllowedOrigins` config), Swagger (dev only)
- `appsettings.json` committed with empty secret placeholders; `appsettings.Development.json` (local secrets) confirmed git-ignored
- Added root `docker-compose.yml` (Postgres + API) for local dev per spec.md folder layout
- Reviewer secrets check: no hardcoded `Jwt:Secret` / `OpenAI:ApiKey` in API layer ✅
- **Deferred/environment limitation:** cannot run `dotnet run` or `docker-compose up` in this dev sandbox — only .NET 10 runtime installed (net8.0 apps need .NET 8 runtime) and Docker is not available. Build-only verification (`dotnet build VocaPlay.sln` → 0 errors) was performed. Human should run GATE 5's local smoke test (`docker-compose up`, hit `/auth/register`, `/wordsets`, `/wordsets/{id}/game`) on a machine with Docker installed.
- **Also note:** Railway "Root Directory" should be set to `src` (not `VocaPlay.Api`) with Dockerfile path `VocaPlay.Api/Dockerfile`, since the Dockerfile build context needs sibling project folders (Domain/Application/Infrastructure). To revisit in Phase 10.
- **Next (pending approval):** React Expert builds frontend (Phase 6)

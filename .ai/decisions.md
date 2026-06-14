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

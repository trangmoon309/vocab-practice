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

---

## 2026-06-20 — GATE 5 approved: API layer

- Human approved API layer on disk despite deferred local smoke test (no Docker/.NET 8 runtime in original dev sandbox).
- Superseded in practice: .NET 8 runtime was later installed on the dev machine and the API was run live against a real Supabase Postgres instance (session pooler, IPv4) — `/auth/register`, `/auth/login`, and word CRUD were smoke-tested directly via HTTP and confirmed working end-to-end.
- **Next:** Phase 6 — React frontend (already built ahead of this gate; see GATE 6 entry below for scope and deviations).

---

## 2026-06-20 — GATE 6 pending approval: React frontend on disk

- Full `vocaplay-web/` SPA built and run live against the API: Axios instance with silent-refresh-on-401 interceptor, `AuthContext` + `useAuth`, `ProtectedRoute`, all pages (Login, Register, Words dashboard, Game, Chat), typed API clients (`auth.ts`, `words.ts`, `game.ts`, `chat.ts`), `types/index.ts` with no `any`.
- **Deviation from spec.md/agent-workflow.md (flagged, not silently applied):** the `WordSet` grouping concept was removed mid-build at the human's explicit request (see below) — so the originally-planned `WordSetsPage` + `WordSetDetailPage` collapsed into a single flat `WordsPage`. This was a human-directed scope cut, not an agent decision.
- **Deviation:** `ChatWidget` was specced as a floating bubble persistent across all authenticated pages (`react-patterns.skill.md`: "ChatWidget is rendered in App.tsx outside Router so it persists across navigation"). Implemented instead as a dedicated `/chat` route/page reached via nav link. Floating-bubble pattern was not built.
- **UI pass (human-requested, post-functional-build):** bento-grid pastel redesign applied — mint/lavender/cream palette, coral CTA buttons, 16px rounded corners (`rounded-bento`), soft hover-lift shadows, Quicksand/Inter type pairing. Covers all 5 pages + Navbar.
- **Verified live:** registered/logged in via test account against Supabase-backed API; added a word via `/words` and confirmed it round-tripped through the UI's `WordsPage`. Full click-through of Game and Chat pages in-browser was not directly observed by the agent (Chrome extension was unreachable this session) — human should confirm visually.
- **Related architectural change (not part of Phase 6 itself, done earlier this session):** `WordSet` entity/table/repo/controller fully removed from Domain/Application/Infrastructure/Api; Supabase schema migrated to drop `WordSets` and add `Words.UserId` FK directly to `Users`. Flagged here since it affects what Phase 6 had to build against.
- **Next (pending approval):** Phase 7 — Chatbot full integration (the chat *page* exists and calls `/chat`, but the 3-capability system prompt behavior — bulk-add parsing, explain, quiz — has not yet been tested end-to-end with a live OpenAI key in this session).

---

## 2026-06-20 — GATE 6 fix applied: ChatWidget converted to floating bubble (spec-compliant)

- Removed the dedicated `/chat` route and `ChatPage.tsx`; replaced with `components/chat/ChatWidget.tsx`, `ChatWindow.tsx`, `ChatMessageBubble.tsx` (single-message bubble; named to avoid colliding with the `ChatMessage` type from `types/index.ts`) and a `hooks/useChat.ts` hook — matching the file structure spec'd in `agent-workflow.md`/`spec.md` §7.
- `ChatWidget` is rendered as a sibling of `<Routes>` in `App.tsx` (not inside any `<Route>`), so it persists across navigation per `react-patterns.skill.md`: "ChatWidget is rendered in App.tsx outside Router so it persists across navigation." (Here "Router" is read as "the route switch" — `ChatWidget` still needs `BrowserRouter`/`AuthProvider` context from `main.tsx`, which it already has as a sibling tree.)
- Widget auto-hides on `/login` and `/register` and whenever the user isn't authenticated; chat history only fetches lazily the first time the panel is opened (`useChat(enabled && open)`).
- Removed the "AI Chat" nav-link and the `/chat` quick-action button from `Navbar` and `WordsPage` respectively, since the floating bubble is now globally accessible instead.
- Verified: `tsc --noEmit` clean, dev server still serves 200 after the change.
- **Outstanding from GATE 6 review:** visual confirmation of Game page and the chat widget's live rendering still pending (Chrome extension unavailable this session) — recommend a manual click-through before moving to Phase 7.

--- GATE 6: Awaiting your approval to proceed to Phase 7 (Chatbot full integration) ---

---

## 2026-06-20 — GATE 6 approved: React frontend

- Human approved frontend on disk, including the ChatWidget spec-compliance fix (floating bubble, persists across navigation, hidden on auth pages).
- **Next:** Phase 7 — Chatbot full integration. Coordinated .NET Expert + React Expert task per `agent-workflow.md`:
  - .NET Expert: verify `SendChatMessageCommandHandler`'s `%%ACTION%%...%%END%%` parser and `BulkAddWordsCommand` trigger work end-to-end against a live OpenAI key → GATE 7a (human tests `/chat` via curl/Postman)
  - React Expert: confirm `ChatWidget`/`ChatWindow` render the `action` field meaningfully (e.g. toast/confirmation when words are bulk-added) → GATE 7b (human tests all 3 chatbot modes in the UI: bulk add, explain, quiz)

---

## 2026-06-20 — GATE 7a: .NET chatbot integration reviewed (live OpenAI call blocked by quota, not code)

- Fixed stale system-prompt wording in `OpenAiChatService.cs` ("add to a word set" → "add") left over from the WordSet removal — now matches `spec.md` §6 verbatim.
- Live test attempted: logged in as `test@example.com`, POSTed a bulk-add message to `/api/chat` against the real OpenAI key provided this session.
- **Result:** OpenAI returned `HTTP 429 insufficient_quota` (account billing/plan issue, not a bug). Confirmed via server logs that this is a clean external failure — the surrounding code behaved exactly as designed:
  - `OpenAiChatService.GetCompletionAsync` caught the exception, logged it server-side, returned the spec'd fallback string (`"Sorry, I'm having trouble right now. Please try again."`) instead of leaking the OpenAI exception/stack trace to the client
  - `SendChatMessageCommandHandler` still persisted both the user message and the fallback assistant reply to `ChatMessages`
  - `ChatController` returned `200` with `{ reply, action: null }` — correct shape, no 500
- **Not verified:** an actual successful GPT-4o completion, the `%%ACTION%%...%%END%%` parser firing on real model output, or a real `BULK_ADD_WORDS` action reaching `BulkAddWordsCommandHandler`. This remains unverified until the OpenAI account has quota.
- **Human decision:** accept this as sufficient for GATE 7a given the account-billing blocker is outside code scope; do not block the gate on it.
- **Next:** GATE 7b — React side. Will review (not live-test, same OpenAI quota blocker applies) whether `ChatWidget`/`ChatWindow` handle the `action` field meaningfully, since end-to-end UI testing of bulk-add/explain/quiz also requires a working OpenAI call.

---

## 2026-06-20 — GATE 7b: React chatbot action handling fixed (code review only, same OpenAI quota blocker)

- **Found gap:** `useChat.ts` discarded `ChatResponse.action` entirely — `agent-workflow.md` Phase 7 task explicitly calls for "action confirmation toast" and this was missing.
- **Fix:** `useChat` now tracks `lastAction` (cleared on new chat-history clear) and exposes `dismissAction`. Added `components/chat/ChatActionToast.tsx` — a mint-colored bento-styled toast that renders inside the open `ChatWidget` panel when `action.type === 'BULK_ADD_WORDS'`, shows "✅ Added N words to your list!", auto-dismisses after 4s, and is manually dismissable.
- Quiz (`QUIZ_START`) and explain flows have no `action` payload in the current backend contract (conversational replies only, per spec.md §5) — nothing further to wire for those two capabilities on the frontend side; they render as plain assistant messages, which is correct.
- `tsc --noEmit` clean after the change.
- **Not verified live** (same blocker as GATE 7a): cannot confirm the toast actually fires against a real `BULK_ADD_WORDS` response until the OpenAI account has quota. Verified instead via static review: `ChatResponseDto`/`ChatActionDto` field names match what `ChatActionToast` reads (`type`, `wordsAdded`), and `wordSetId` was correctly absent (removed earlier this session).
- **Human decision carried over from GATE 7a:** accept code-review-level verification given the quota blocker is account billing, not implementation. Phase 7 considered complete pending a live retest once quota is available.

--- GATE 7: Awaiting your approval. Recommend a live retest of bulk-add/explain/quiz once OpenAI quota is restored, but not required to proceed. ---

---

## 2026-06-20 — GATE 7 approved: Chatbot integration

- Human approved Phase 7 (chatbot) as complete, accepting the OpenAI-quota caveat without a live retest.
- **Next:** Phase 8 — Tests (QA agent). Per `agent-workflow.md`: xUnit handler tests (mocked repos) for the Application layer, Testcontainers integration tests for API endpoints, Playwright E2E for register → login → add words → play game → chatbot bulk add.
- **GATE 8 criteria:** all tests green, coverage ≥70% on Application layer, no spec mismatches.

---

## 2026-06-20 — GATE 8 pending approval: xUnit Application-layer tests (scope-reduced)

- **Scope decision (human-approved):** built xUnit handler tests only. Testcontainers integration tests and Playwright E2E deferred — this dev sandbox has no Docker and no browser test runner installed (consistent with the Docker gap already noted at GATE 5). Test code for those two layers does not exist yet; only the mocked-repository unit layer was built and verified.
- New project: `tests/VocaPlay.Application.Tests` (xUnit + Moq + coverlet.collector), added to `VocaPlay.sln`, referencing `VocaPlay.Application` + `VocaPlay.Domain` only (no Infrastructure/API dependency — true unit tests, no real DB/HTTP).
- **Coverage:** all 14 Application-layer handlers covered (Auth: Register/Login/RefreshToken; Words: Add/Update/Delete/BulkAdd/GetWords; Game: GetGamePairs/SaveGameSession/GetGameSessions; Chat: SendChatMessage/ClearChatHistory/GetChatHistory).
- **Results:** `dotnet test` → 38/38 passed, 0 failed. Coverage (coverlet, cobertura): **90.08% line / 82.81% branch** overall, **89.13%** on `VocaPlay.Application` package specifically — well above the 70% gate threshold. (Only near-zero spot: `DependencyInjection.AddApplication()`, which is DI wiring, not business logic — expected to be untested by a unit suite.)
- Notable cases covered per handler: ownership/forbidden checks (`Update`/`DeleteWordCommandHandler`), not-found checks, validation failures (invalid CEFR level/word type, duplicate email, wrong password, invalid/expired refresh token), bulk-add dedup logic (case-insensitive, both against existing words and within the same batch), game pairs minimum-4-words guard, and the chat handler's `%%ACTION%%...%%END%%` parser (happy path, malformed JSON, and verifying the block is stripped from both the API reply and what gets persisted to `ChatMessages`).
- Spec-validator check: handler signatures/field names exercised in tests match the current (post-WordSet-removal) Application layer exactly — no mismatches found.
- Full solution (`VocaPlay.sln`) still builds with 0 errors including the new test project.
- **Not done (flagged, not silently skipped):** Testcontainers API integration tests, Playwright E2E (register→login→add words→play game→chatbot bulk add). Recommend revisiting once a machine/CI runner with Docker + browser support is available — `agent-workflow.md` Phase 8 originally specified both.

--- GATE 8: Awaiting your approval. All built tests green (38/38), coverage 89–90% (target was ≥70%). Testcontainers/Playwright intentionally deferred per your scope decision — flag if you want those revisited before sign-off. ---

---

## 2026-06-20 — GATE 8 approved: xUnit Application-layer tests

- Human approved Phase 8 as complete with the reduced scope (xUnit only; Testcontainers/Playwright deferred).
- **Next:** Phase 9 — Reviewer pass. Full review of all production code across all 4 .NET projects and the React app: layer-violation check, hardcoded-secrets check, error-response format check, no `any` types in React, EF migrations complete, OpenAI system prompt matches spec.md.
- **GATE 9 criteria:** zero BLOCKER findings. Human reviews WARNINGs and decides accept-or-fix.

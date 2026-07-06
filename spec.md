# spec.md — VocaPlay MVP
> English vocabulary learning app for Vietnamese learners.  
> Users build one flat personal word list, practice with a matching game (English ↔ Vietnamese), and use an AI chatbot to bulk-add words, get explanations, and play text-based quizzes.

**GitHub repo:** `https://github.com/<your-username>/vocab-practice`

---

## 1. Project goal

VocaPlay is a web app that helps Vietnamese users learn English vocabulary through a self-curated word list, an interactive matching game, and an AI-powered chatbot assistant. Users register, add their own English–Vietnamese word pairs directly to their personal list (no grouping/organizing step), and practice through a card-matching game or conversation with the chatbot.

MVP scope: auth, word management (CRUD on a flat per-user word list), matching game, AI chatbot with 3 capabilities, hosted fully on Supabase + Railway.

> **Note (post-MVP simplification):** The original design grouped words into user-created `WordSet`s (like folders/decks). This was removed — each user now has a single flat list of words. Rationale: the grouping step added friction without enough payoff for the MVP's "add words, play game" core loop. All `/wordsets/*` endpoints, the `WordSet` entity/table, and the WordSet UI (set list + set detail pages) were deleted. See §4, §5, §7 for the current (flat) shape — historical references to `WordSet` elsewhere in this doc describe the pre-removal design and are kept only where still illustrative of the migration.

---

## 2. Tech stack

| Layer | Technology |
|---|---|
| Backend | .NET 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | **Supabase PostgreSQL** (managed, via connection string) |
| Auth | JWT Bearer tokens (access + refresh). Supabase Auth is NOT used — custom JWT to keep backend self-contained. |
| File storage | Supabase Storage (future; not in MVP) |
| Frontend hosting | **Supabase** is DB/storage only. Frontend → **Vercel** (or Netlify) |
| Backend hosting | **Railway** (Docker-based .NET container) |
| AI Chatbot | **OpenAI GPT-4o** via `openai` NuGet package |
| Frontend | React 18 + Vite |
| Styling | TailwindCSS v3 |
| HTTP client | Axios |
| Routing | React Router v6 |
| State | React Context + useState/useReducer |
| Testing (BE) | xUnit + Testcontainers |
| Testing (FE) | Playwright |
| Containerization | Docker + docker-compose (local dev) |
| CI/CD | GitHub Actions → deploy to Railway (BE) + Vercel (FE) |

---

## 3. Hosting architecture

```
[User browser]
     │
     ▼
[Vercel] ── serves ──► React SPA (vocaplay-web/)
     │                  linked to: github.com/<you>/vocab-practice
     │  REST API calls  auto-deploy on push to: main
     ▼
[Railway] ── runs ──► .NET 8 Web API (VocaPlay.Api/)
     │                  linked to: github.com/<you>/vocab-practice
     │                  auto-deploy on push to: main
     ├── PostgreSQL ──► Supabase (connection string only)
     └── OpenAI ──────► GPT-4o API
```

### Monorepo layout in `vocab-practice`
The single GitHub repo holds both projects:
```
vocab-practice/                  ← repo root
├── VocaPlay.Api/                ← .NET backend (Railway deploys this)
│   └── Dockerfile
├── vocaplay-web/                ← React frontend (Vercel deploys this)
│   └── package.json
├── docker-compose.yml           ← local dev only
└── .github/
    └── workflows/
        ├── backend.yml          ← CI: build + test .NET on PR
        └── frontend.yml         ← CI: lint + test React on PR
```

### Supabase setup (DB only)
- Create a Supabase project → Settings → Database → **Connection string (Transaction mode / port 6543)**.
- Paste it as `ConnectionStrings__Default` in Railway env vars.
- No Supabase SDK in .NET — EF Core connects directly via Npgsql.
- Connection string must include `?sslmode=require`.
- Supabase is **linked to the repo only for the DB** — no Supabase hosting of code.

### Railway setup (Backend)
- New project → **Deploy from GitHub repo** → select `vocab-practice`.
- Set **Root Directory** to `VocaPlay.Api` so Railway finds the `Dockerfile`.
- Set env vars: `ConnectionStrings__Default`, `Jwt__Secret`, `OpenAI__ApiKey`, etc.
- Railway provides public HTTPS URL: e.g. `https://vocaplay-api.up.railway.app`.
- Auto-deploys on every push to `main` that touches `VocaPlay.Api/**`.

### Vercel setup (Frontend)
- New project → **Import Git repository** → select `vocab-practice`.
- Set **Root Directory** to `vocaplay-web`.
- Framework preset: **Vite**. Build command: `vite build`. Output dir: `dist`.
- Set env var: `VITE_API_BASE_URL=https://vocaplay-api.up.railway.app/api`.
- Auto-deploys on every push to `main` that touches `vocaplay-web/**`.

---

## 4. Data models

### User
```
User {
  Id          : Guid        PK
  Email       : string      unique, required
  DisplayName : string      required
  PasswordHash: string      required (bcrypt)
  CreatedAt   : DateTime    UTC
  UpdatedAt   : DateTime    UTC
}
```

### Word
> `WordSet` has been removed. Words attach directly to the owning `User`.
```
Word {
  Id              : Guid        PK
  UserId          : Guid        FK → User.Id (cascade delete)
  English         : string      required, max 200
  Vietnamese      : string      required, max 200
  Pronunciation   : string?     max 200   (IPA or phonetic, e.g. "/ˈæp.əl/")
  Level           : string?     max 10    enum: "A1"|"A2"|"B1"|"B2"|"C1"|"C2"|null
  Type            : string?     max 50    enum: "Noun"|"Verb"|"Adjective"|"Adverb"|
                                                "Preposition"|"Conjunction"|
                                                "Pronoun"|"Interjection"|null
  ExampleSentence : string?     max 500
  EnglishDefinition: string?    max 500  (short English definition, used by "Definition Match")
  CreatedAt       : DateTime    UTC
  UpdatedAt       : DateTime    UTC
}
```

### GameSession
> No longer tied to a `WordSet` — a session is just a score against the user's whole word list.
```
GameSession {
  Id          : Guid        PK
  UserId      : Guid        FK → User.Id
  Score       : int
  TotalPairs  : int
  CompletedAt : DateTime    UTC
}
```

### ChatMessage (persists chatbot history per user)
```
ChatMessage {
  Id          : Guid        PK
  UserId      : Guid        FK → User.Id (cascade delete)
  Role        : string      "user" | "assistant"
  Content     : string      max 4000
  CreatedAt   : DateTime    UTC
}
```

### GitHub Actions — `.github/workflows/`

**`backend.yml`** — triggers on PR or push to `main` touching `VocaPlay.Api/**`:
```yaml
on:
  push:
    paths: ['VocaPlay.Api/**']
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore VocaPlay.Api/VocaPlay.Api.csproj
      - run: dotnet build VocaPlay.Api/VocaPlay.Api.csproj --no-restore
      - run: dotnet test VocaPlay.Api.Tests/ --no-build
```
Railway picks up the deploy automatically after CI passes (webhook).

**`frontend.yml`** — triggers on PR or push to `main` touching `vocaplay-web/**`:
```yaml
on:
  push:
    paths: ['vocaplay-web/**']
jobs:
  lint-build:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: vocaplay-web
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '20' }
      - run: npm ci
      - run: npm run build
```
Vercel picks up the deploy automatically after CI passes (Git integration).

---

## 4. Data models — Relationships
- User 1 → N Word
- User 1 → N GameSession
- User 1 → N ChatMessage

---

## 5. API contract

Base URL: `/api`  
Auth: all routes except `/auth/*` require `Authorization: Bearer <token>`.

### Auth

| Method | Path | Request body | Response |
|---|---|---|---|
| POST | `/auth/register` | `{ email, displayName, password }` | `{ accessToken, refreshToken, user }` |
| POST | `/auth/login` | `{ email, password }` | `{ accessToken, refreshToken, user }` |
| POST | `/auth/refresh` | `{ refreshToken }` | `{ accessToken, refreshToken }` |
| POST | `/auth/logout` | `{ refreshToken }` | `204` |

### Words
> `WordSet` and all `/wordsets/*` routes were removed. Words are flat per-user — no grouping layer.

| Method | Path | Description |
|---|---|---|
| GET | `/words` | List all words for current user |
| POST | `/words` | Add a word |
| PUT | `/words/{wordId}` | Update a word |
| DELETE | `/words/{wordId}` | Delete a word |
| POST | `/words/bulk` | **Bulk add words (used by chatbot)** |

POST/PUT body:
```json
{
  "english": "apple",
  "vietnamese": "quả táo",
  "pronunciation": "/ˈæp.əl/",
  "level": "A1",
  "type": "Noun",
  "exampleSentence": "I eat an apple every day.",
  "englishDefinition": "A round fruit with red, green, or yellow skin."
}
```
`pronunciation`, `level`, `type`, `exampleSentence`, `englishDefinition` are all optional. `englishDefinition` powers the "Definition Match" game mode (see §8) — words without one are simply excluded from that mode, not an error.

Valid `level` values: `"A1"`, `"A2"`, `"B1"`, `"B2"`, `"C1"`, `"C2"` — backend validates against this enum, returns `400` for other values.

Valid `type` values: `"Noun"`, `"Verb"`, `"Adjective"`, `"Adverb"`, `"Preposition"`, `"Conjunction"`, `"Pronoun"`, `"Interjection"`.

POST `/words/bulk` body:
```json
{
  "words": [
    { "english": "apple", "vietnamese": "quả táo", "pronunciation": "/ˈæp.əl/", "level": "A1", "type": "Noun", "exampleSentence": "I eat an apple." },
    { "english": "book",  "vietnamese": "cuốn sách", "level": "A1", "type": "Noun" }
  ]
}
```
Response: `{ "added": 12, "skipped": 2, "skippedReasons": ["duplicate: apple"] }`
Duplicates (same English, case-insensitive, across the whole user's list) are skipped, not errored.

### Game
> Game pairs draw from the user's entire word list, not a specific set. Two game modes exist
> (`GameMode` enum: `Translation` | `Definition`), selectable per request via a `mode` query param.

| Method | Path | Description |
|---|---|---|
| GET | `/game/pairs?mode=Translation\|Definition` | Get shuffled pairs from the user's word list (`mode` defaults to `Translation`) |
| POST | `/game/sessions` | Save completed session |
| GET | `/game/sessions` | List past sessions |

**Game modes (user-facing names):**
- **"Translation Match"** (`mode=Translation`) — English word ↔ Vietnamese meaning. Uses `Word.Vietnamese`. Minimum 4 words in the user's list.
- **"Definition Match"** (`mode=Definition`) — English word ↔ English definition. Uses `Word.EnglishDefinition`; words without one are excluded. Minimum 4 *eligible* words (i.e. with a non-empty `EnglishDefinition`).

GET `/game/pairs` response:
```json
{ "mode": "Translation", "pairs": [{ "id": "...", "english": "...", "match": "..." }] }
```
`match` is the Vietnamese meaning (Translation mode) or the English definition (Definition mode) — the field name is generic so the frontend renders either mode the same way. Returns `400` with a mode-specific message if fewer than 4 eligible words exist.

POST `/game/sessions` body: `{ "score": 8, "totalPairs": 10 }`

### Chatbot

| Method | Path | Description |
|---|---|---|
| POST | `/chat` | Send a message, get AI response (streaming optional) |
| GET | `/chat/history` | Get last 50 messages for current user |
| DELETE | `/chat/history` | Clear chat history |

POST `/chat` request:
```json
{
  "message": "Add these words: apple = quả táo, book = cuốn sách"
}
```

POST `/chat` response:
```json
{
  "reply": "I've added 2 words to your list: apple, book.",
  "action": {
    "type": "BULK_ADD_WORDS",
    "wordsAdded": 2
  }
}
```

`action` is `null` for conversational replies. Possible `type` values: `BULK_ADD_WORDS`, `QUIZ_START`, `null`.

---

## 6. Chatbot — capabilities & system prompt

### Three capabilities

**1. Bulk add words**
User pastes a list of vocab in any format (comma-separated, line-by-line, "word = translation", etc.). The bot parses it, calls `POST /words/bulk` internally, and confirms what was added.

Example prompts:
- *"Add these: hotel = khách sạn, airport = sân bay, passport = hộ chiếu"*
- *"I have: apple, banana, cherry — add them with Vietnamese translations"* (bot auto-translates using GPT-4o knowledge)

**2. Explain words / give examples**
User asks about a word's meaning, usage, or pronunciation tip.

Example prompts:
- *"What does 'ubiquitous' mean? Give me a Vietnamese explanation and 2 examples."*
- *"Explain the difference between 'affect' and 'effect' in Vietnamese"*

**3. Text-based quiz**
Bot picks random words from the user's word list and quizzes the user in chat. User types the answer; bot evaluates and keeps score.

Example prompts:
- *"Quiz me on my words — 5 questions"*
- *"Give me 3 fill-in-the-blank sentences from my list"*

### System prompt (sent as the `system` role on every `/chat` call)

```
You are VocaPlay Assistant, an AI tutor helping Vietnamese learners build English vocabulary.
You respond in a mix of English and Vietnamese — use Vietnamese to explain meanings and grammar,
use English for the vocab terms themselves.

You have three jobs:
1. BULK ADD WORDS: When the user gives you a list of words to add, parse them,
   auto-fill missing Vietnamese translations, pronunciation (IPA), CEFR level (A1–C2), and
   word type (Noun/Verb/etc.) using your knowledge, and respond with a JSON action block so
   the backend can call the bulk-add API.
2. EXPLAIN: Explain English words in simple Vietnamese, give example sentences, pronunciation tips.
3. QUIZ: Run a short text quiz using the user's own vocabulary. Ask one question at a time,
   wait for the answer, give feedback, then ask the next.

When performing a BULK_ADD action, always end your reply with this exact JSON block on its own line:
%%ACTION%%{"type":"BULK_ADD_WORDS","words":[{"english":"...","vietnamese":"...","pronunciation":"...","level":"B1","type":"Noun","exampleSentence":"..."}]}%%END%%

Keep replies friendly, concise, and encouraging. Use 🌟 sparingly for correct answers.
```

### Backend parsing logic (`ChatService.cs`)
- After receiving GPT-4o response, scan for `%%ACTION%%...%%END%%` block.
- If found: parse JSON, call `WordService.BulkAddAsync(userId, words)`, strip the action block from the reply shown to the user, and populate the `action` field in the API response.
- Persist both user message and assistant reply to `ChatMessage` table.
- Send last 10 messages as context to GPT-4o on every call (sliding window).

---

## 7. Folder structure

### Backend — Clean Architecture (4 projects in one solution)

```
Clean Architecture dependency rule:
  API → Application → Domain
  Infrastructure → Application → Domain
  (nothing points outward from Domain)
```

```
vocab-practice/
└── VocaPlay.sln
    ├── src/
    │
    │   ── VocaPlay.Domain/                   (no dependencies on other projects)
    │   │   ├── Entities/
    │   │   │   ├── User.cs
    │   │   │   ├── Word.cs
    │   │   │   ├── GameSession.cs
    │   │   │   └── ChatMessage.cs
    │   │   ├── Enums/
    │   │   │   ├── CefrLevel.cs              (A1 A2 B1 B2 C1 C2)
    │   │   │   └── WordType.cs               (Noun Verb Adjective …)
    │   │   ├── Interfaces/
    │   │   │   ├── Repositories/
    │   │   │   │   ├── IUserRepository.cs
    │   │   │   │   ├── IWordRepository.cs
    │   │   │   │   ├── IGameSessionRepository.cs
    │   │   │   │   └── IChatRepository.cs
    │   │   │   └── Services/
    │   │   │       └── IAiChatService.cs     (abstraction over OpenAI)
    │   │   └── Exceptions/
    │   │       ├── NotFoundException.cs
    │   │       ├── ForbiddenException.cs
    │   │       └── ValidationException.cs
    │
    │   ── VocaPlay.Application/              (depends on: Domain only)
    │   │   ├── Common/
    │   │   │   ├── Interfaces/
    │   │   │   │   └── ICurrentUserService.cs
    │   │   │   └── Models/
    │   │   │       └── Result.cs             (Result<T> for error-free returns)
    │   │   ├── Auth/
    │   │   │   ├── Commands/
    │   │   │   │   ├── RegisterCommand.cs
    │   │   │   │   ├── RegisterCommandHandler.cs
    │   │   │   │   ├── LoginCommand.cs
    │   │   │   │   ├── LoginCommandHandler.cs
    │   │   │   │   ├── RefreshTokenCommand.cs
    │   │   │   │   └── RefreshTokenCommandHandler.cs
    │   │   │   └── DTOs/
    │   │   │       ├── AuthRequestDto.cs
    │   │   │       └── AuthResponseDto.cs
    │   │   ├── Words/                        (WordSets layer removed — flat per-user words)
    │   │   │   ├── Commands/
    │   │   │   │   ├── AddWordCommand.cs + Handler
    │   │   │   │   ├── UpdateWordCommand.cs + Handler
    │   │   │   │   ├── DeleteWordCommand.cs + Handler
    │   │   │   │   └── BulkAddWordsCommand.cs + Handler
    │   │   │   ├── Queries/
    │   │   │   │   └── GetWordsQuery.cs + Handler
    │   │   │   └── DTOs/
    │   │   │       ├── WordDto.cs
    │   │   │       └── BulkAddResultDto.cs
    │   │   ├── Game/
    │   │   │   ├── Queries/
    │   │   │   │   └── GetGamePairsQuery.cs + Handler
    │   │   │   ├── Commands/
    │   │   │   │   └── SaveGameSessionCommand.cs + Handler
    │   │   │   └── DTOs/
    │   │   │       ├── GamePairsDto.cs
    │   │   │       └── GameSessionDto.cs
    │   │   └── Chat/
    │   │       ├── Commands/
    │   │       │   ├── SendChatMessageCommand.cs + Handler
    │   │       │   └── ClearChatHistoryCommand.cs + Handler
    │   │       ├── Queries/
    │   │       │   └── GetChatHistoryQuery.cs + Handler
    │   │       └── DTOs/
    │   │           ├── ChatRequestDto.cs
    │   │           └── ChatResponseDto.cs
    │
    │   ── VocaPlay.Infrastructure/           (depends on: Domain + Application)
    │   │   ├── Persistence/
    │   │   │   ├── AppDbContext.cs
    │   │   │   ├── Migrations/
    │   │   │   └── Repositories/
    │   │   │       ├── UserRepository.cs
    │   │   │       ├── WordRepository.cs
    │   │   │       ├── GameSessionRepository.cs
    │   │   │       └── ChatRepository.cs
    │   │   ├── Configurations/               (EF Fluent API per entity)
    │   │   │   ├── UserConfiguration.cs
    │   │   │   ├── WordConfiguration.cs
    │   │   │   ├── GameSessionConfiguration.cs
    │   │   │   └── ChatMessageConfiguration.cs
    │   │   ├── ExternalServices/
    │   │   │   └── OpenAiChatService.cs      (implements IAiChatService)
    │   │   ├── Auth/
    │   │   │   └── JwtTokenService.cs
    │   │   └── DependencyInjection.cs        (IServiceCollection extension)
    │
    │   └── VocaPlay.Api/                     (depends on: Application + Infrastructure)
    │       ├── Controllers/
    │       │   ├── AuthController.cs
    │       │   ├── WordsController.cs
    │       │   ├── GameController.cs
    │       │   └── ChatController.cs
    │       ├── Middleware/
    │       │   └── ErrorHandlingMiddleware.cs
    │       ├── Services/
    │       │   └── CurrentUserService.cs     (implements ICurrentUserService, reads JWT claims)
    │       ├── Dockerfile
    │       ├── appsettings.json
    │       ├── appsettings.Development.json
    │       └── Program.cs
    │
    └── tests/
        ├── VocaPlay.Domain.Tests/
        │   └── (entity / value object unit tests)
        ├── VocaPlay.Application.Tests/
        │   └── (handler unit tests — mock repositories)
        └── VocaPlay.Api.Tests/
            └── (integration tests — Testcontainers + PostgreSQL)
```

### Layer responsibilities

| Layer | Responsibility | Allowed dependencies |
|---|---|---|
| **Domain** | Entities, enums, repository interfaces, domain exceptions | None |
| **Application** | Use-case handlers (CQRS commands/queries), DTOs, orchestration | Domain only |
| **Infrastructure** | EF Core, PostgreSQL, OpenAI client, JWT token generation | Domain + Application |
| **API** | HTTP controllers, middleware, DI wiring, `Program.cs` | Application + Infrastructure |

### CQRS pattern (no MediatR — manual dispatch)
Each use case is a self-contained `Command` or `Query` + `Handler` pair.  
Controllers call handlers directly via injected interfaces — no MediatR dependency needed for MVP.

Example flow for "Add a word":
```
POST /words
  → WordsController.Add(AddWordCommand)
    → AddWordCommandHandler.Handle(command)
      → IWordRepository.AddAsync(word)      (word.UserId set from JWT — no ownership lookup needed)
      → returns WordDto
```

### Dependency injection wiring (`Program.cs`)
```csharp
builder.Services.AddApplication();      // registers all handlers (VocaPlay.Application)
builder.Services.AddInfrastructure(     // registers DbContext, repos, OpenAI, JWT
    builder.Configuration);
```
`AddApplication()` lives in `VocaPlay.Application`.  
`AddInfrastructure()` lives in `VocaPlay.Infrastructure/DependencyInjection.cs`.

### Frontend — `vocaplay-web/`
```
vocaplay-web/
├── src/
│   ├── api/
│   │   ├── axios.ts
│   │   ├── auth.ts
│   │   ├── words.ts                ← flat words API (replaces wordsets.ts)
│   │   ├── game.ts
│   │   └── chat.ts
│   ├── context/
│   │   └── AuthContext.tsx
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── WordsPage.tsx           (bento-grid dashboard — replaces WordSetsPage + WordSetDetailPage)
│   │   ├── GamePage.tsx
│   │   └── ChatPage.tsx
│   ├── components/
│   │   └── layout/
│   │       ├── Navbar.tsx
│   │       └── ProtectedRoute.tsx
│   ├── hooks/
│   │   └── useAuth.ts
│   ├── types/
│   │   └── index.ts
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── tailwind.config.js
├── vite.config.ts
└── package.json
```

### UI design system (bento-grid pastel aesthetic)
The frontend uses a calm, premium "minimalist bento grid" visual style:

- **Palette** (Tailwind custom colors in `tailwind.config.js`): `mint` (soft mint green, success/stat accents), `lavender` (gentle lavender, secondary actions/nav), `cream` (warm cream, page background), `coral` (high-contrast CTA color), `ink` (neutral text tones).
- **Shape language**: 16px rounded corners everywhere via the `rounded-bento` utility (maps to `border-radius: 16px`). Cards use the shared `.bento-card` class (white surface, soft shadow, rounded-bento).
- **Buttons**: `.btn-coral` for primary/CTA actions (Add word, Play, Send, Sign in), `.btn-ghost` for secondary actions. High contrast coral (`#FF6B52` family) draws the eye to the one action that matters per screen.
- **Inputs**: shared `.input-pastel` class — rounded, lavender-tinted border, soft focus ring.
- **Motion**: subtle hover lift (`hover:-translate-y-0.5`) and shadow deepening (`shadow-soft` → `shadow-soft-hover`) on interactive cards; `animate-pop-in` for newly-revealed forms/results.
- **Typography**: `Quicksand` (rounded, friendly) for headings via `font-display`, `Inter` for body text via default `font-sans`. Both loaded from Google Fonts in `index.html`.
- **Layout pattern**: `WordsPage` opens with a 3-card bento header row (total words / game-readiness / quick-add CTA) in mint/lavender/cream, then a responsive 2-column grid of individual word cards below — establishing hierarchy (stats → primary action → content) before the user scans their list.

---

## 8. Game mechanics — Matching games

Users pick a game mode from a selection screen (`/game` in the frontend) before playing. Both modes
share the same matching mechanic; they differ only in what's shown on the right-hand cards.

| Mode | User-facing name | Left card | Right card | Source field |
|---|---|---|---|---|
| `Translation` | **Translation Match** | English word | Vietnamese meaning | `Word.Vietnamese` |
| `Definition` | **Definition Match** | English word | English definition | `Word.EnglishDefinition` |

- Grid of cards: left column = English (shuffled), right column = the mode's `match` value (shuffled independently).
- User clicks one left card + one right card to attempt a match.
- Correct: both cards turn green (mint) and lock out.
- Wrong: both cards flash red (coral) and reset.
- Game ends when all pairs matched.
- Score = correct first-attempt matches / total pairs.
- Save via `POST /game/sessions` on completion (mode is not currently persisted on `GameSession` — only score/totalPairs).
- Minimum 4 *eligible* pairs to start (eligible = has the field the mode needs); recommend 6–12. Definition Match silently excludes words missing an `EnglishDefinition` rather than erroring on them individually — the 4-minimum check applies only to the eligible subset.

---

## 9. Auth flow

- Login/register → store `accessToken` in memory (React Context), `refreshToken` in `localStorage`.
- Axios request interceptor attaches `Authorization: Bearer <accessToken>`.
- Axios response interceptor: on `401`, silent refresh via `POST /auth/refresh`. Fail → clear auth → redirect `/login`.
- On app boot, read `refreshToken` from `localStorage` and silently restore session.

---

## 10. Error handling

- Backend: `ErrorHandlingMiddleware` → `{ message, statusCode }` JSON on all unhandled exceptions.
- 4xx: `{ message: "...", errors?: [...] }`.
- Frontend: global Axios interceptor for network errors; domain errors handled inline per page.
- Chat errors: if GPT-4o call fails, return `{ reply: "Sorry, I'm having trouble right now. Please try again.", action: null }` — never propagate OpenAI errors to the user.

---

## 11. Environment variables

### Backend — Railway env vars
```
ConnectionStrings__Default=postgresql://postgres:[PASSWORD]@db.[PROJECT].supabase.co:5432/postgres?sslmode=require
Jwt__Secret=your-very-long-secret-key-min-32-chars
Jwt__AccessTokenExpiryMinutes=60
Jwt__RefreshTokenExpiryDays=30
Jwt__Issuer=VocaPlay
Jwt__Audience=VocaPlayUsers
OpenAI__ApiKey=sk-...
OpenAI__Model=gpt-4o
OpenAI__MaxHistoryMessages=10
ASPNETCORE_ENVIRONMENT=Production
```

### Backend — local dev (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=vocaplay;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-very-long-secret-key-min-32-chars",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30,
    "Issuer": "VocaPlay",
    "Audience": "VocaPlayUsers"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o",
    "MaxHistoryMessages": 10
  }
}
```

### Frontend — Vercel env vars
```
VITE_API_BASE_URL=https://vocaplay-api.up.railway.app/api
```

### Frontend — local dev (`.env.local`)
```
VITE_API_BASE_URL=http://localhost:5000/api
```

---

## 12. Out of scope (v1)

- Multiple game modes (flashcards, fill-in-the-blank, spelling)
- Audio pronunciation
- Sharing word sets between users
- Spaced repetition / smart scheduling
- Supabase Auth (using custom JWT instead)
- Supabase Realtime for live chat streaming
- Mobile app
- Payments / subscriptions
- Social features
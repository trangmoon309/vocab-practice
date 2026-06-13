# spec.md — VocaPlay MVP
> English vocabulary learning app for Vietnamese learners.  
> Users build their own word lists, practice with a matching game (English ↔ Vietnamese), and use an AI chatbot to bulk-add words, get explanations, and play text-based quizzes.

**GitHub repo:** `https://github.com/<your-username>/vocab-practice`

---

## 1. Project goal

VocaPlay is a web app that helps Vietnamese users learn English vocabulary through self-curated word lists, an interactive matching game, and an AI-powered chatbot assistant. Users register, add their own English–Vietnamese word pairs, organize them into sets, and practice through a card-matching game or conversation with the chatbot.

MVP scope: auth, word management (CRUD), matching game, AI chatbot with 3 capabilities, hosted fully on Supabase + Railway.

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

### WordSet
```
WordSet {
  Id          : Guid        PK
  UserId      : Guid        FK → User.Id (cascade delete)
  Title       : string      required, max 100
  Description : string?     max 500
  CreatedAt   : DateTime    UTC
  UpdatedAt   : DateTime    UTC
}
```

### Word
```
Word {
  Id              : Guid        PK
  WordSetId       : Guid        FK → WordSet.Id (cascade delete)
  English         : string      required, max 200
  Vietnamese      : string      required, max 200
  Pronunciation   : string?     max 200   (IPA or phonetic, e.g. "/ˈæp.əl/")
  Level           : string?     max 10    enum: "A1"|"A2"|"B1"|"B2"|"C1"|"C2"|null
  Type            : string?     max 50    enum: "Noun"|"Verb"|"Adjective"|"Adverb"|
                                                "Preposition"|"Conjunction"|
                                                "Pronoun"|"Interjection"|null
  ExampleSentence : string?     max 500
  CreatedAt       : DateTime    UTC
  UpdatedAt       : DateTime    UTC
}
```

### GameSession
```
GameSession {
  Id          : Guid        PK
  UserId      : Guid        FK → User.Id
  WordSetId   : Guid        FK → WordSet.Id
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
- User 1 → N WordSet
- WordSet 1 → N Word
- User 1 → N GameSession
- WordSet 1 → N GameSession
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

### Word Sets

| Method | Path | Description |
|---|---|---|
| GET | `/wordsets` | List all sets for current user |
| POST | `/wordsets` | Create a new set |
| GET | `/wordsets/{id}` | Get set + its words |
| PUT | `/wordsets/{id}` | Update title / description |
| DELETE | `/wordsets/{id}` | Delete set (cascades words) |

POST/PUT body: `{ "title": "...", "description": "..." }`

GET `/wordsets/{id}` response:
```json
{
  "id": "...", "title": "...", "description": "...",
  "wordCount": 12,
  "words": [{
    "id": "...",
    "english": "apple",
    "vietnamese": "quả táo",
    "pronunciation": "/ˈæp.əl/",
    "level": "A1",
    "type": "Noun",
    "exampleSentence": "I eat an apple every day."
  }],
  "createdAt": "..."
}
```

### Words

| Method | Path | Description |
|---|---|---|
| POST | `/wordsets/{setId}/words` | Add a word |
| PUT | `/wordsets/{setId}/words/{wordId}` | Update a word |
| DELETE | `/wordsets/{setId}/words/{wordId}` | Delete a word |
| POST | `/wordsets/{setId}/words/bulk` | **Bulk add words (used by chatbot)** |

POST/PUT body:
```json
{
  "english": "apple",
  "vietnamese": "quả táo",
  "pronunciation": "/ˈæp.əl/",
  "level": "A1",
  "type": "Noun",
  "exampleSentence": "I eat an apple every day."
}
```
`pronunciation`, `level`, `type`, `exampleSentence` are all optional.

Valid `level` values: `"A1"`, `"A2"`, `"B1"`, `"B2"`, `"C1"`, `"C2"` — backend validates against this enum, returns `400` for other values.

Valid `type` values: `"Noun"`, `"Verb"`, `"Adjective"`, `"Adverb"`, `"Preposition"`, `"Conjunction"`, `"Pronoun"`, `"Interjection"`.

POST `/wordsets/{setId}/words/bulk` body:
```json
{
  "words": [
    { "english": "apple", "vietnamese": "quả táo", "pronunciation": "/ˈæp.əl/", "level": "A1", "type": "Noun", "exampleSentence": "I eat an apple." },
    { "english": "book",  "vietnamese": "cuốn sách", "level": "A1", "type": "Noun" }
  ]
}
```
Response: `{ "added": 12, "skipped": 2, "skippedReasons": ["duplicate: apple"] }`
Duplicates (same English in the same set) are skipped, not errored.

### Game

| Method | Path | Description |
|---|---|---|
| GET | `/wordsets/{setId}/game` | Get shuffled pairs |
| POST | `/game/sessions` | Save completed session |
| GET | `/game/sessions` | List past sessions |

GET `/wordsets/{setId}/game` response:
```json
{ "wordSetId": "...", "wordSetTitle": "...", "pairs": [{ "id": "...", "english": "...", "vietnamese": "..." }] }
```
Minimum 4 pairs; return `400` with message if fewer.

POST `/game/sessions` body: `{ "wordSetId": "...", "score": 8, "totalPairs": 10 }`

### Chatbot

| Method | Path | Description |
|---|---|---|
| POST | `/chat` | Send a message, get AI response (streaming optional) |
| GET | `/chat/history` | Get last 50 messages for current user |
| DELETE | `/chat/history` | Clear chat history |

POST `/chat` request:
```json
{
  "message": "Add these words to my 'Daily' set: apple = quả táo, book = cuốn sách",
  "wordSetId": "optional-uuid-for-context"
}
```

POST `/chat` response:
```json
{
  "reply": "I've added 2 words to your 'Daily' set: apple, book.",
  "action": {
    "type": "BULK_ADD_WORDS",
    "wordSetId": "...",
    "wordsAdded": 2
  }
}
```

`action` is `null` for conversational replies. Possible `type` values: `BULK_ADD_WORDS`, `QUIZ_START`, `null`.

---

## 6. Chatbot — capabilities & system prompt

### Three capabilities

**1. Bulk add words**
User pastes a list of vocab in any format (comma-separated, line-by-line, "word = translation", etc.). The bot parses it, calls `POST /wordsets/{setId}/words/bulk` internally, and confirms what was added.

Example prompts:
- *"Add these to my 'Travel' set: hotel = khách sạn, airport = sân bay, passport = hộ chiếu"*
- *"I have: apple, banana, cherry — add them to Daily set with Vietnamese translations"* (bot auto-translates using GPT-4o knowledge)

**2. Explain words / give examples**
User asks about a word's meaning, usage, or pronunciation tip.

Example prompts:
- *"What does 'ubiquitous' mean? Give me a Vietnamese explanation and 2 examples."*
- *"Explain the difference between 'affect' and 'effect' in Vietnamese"*

**3. Text-based quiz**
Bot picks random words from a given set and quizzes the user in chat. User types the answer; bot evaluates and keeps score.

Example prompts:
- *"Quiz me on my 'Animals' word set — 5 questions"*
- *"Give me 3 fill-in-the-blank sentences from my Daily set"*

### System prompt (sent as the `system` role on every `/chat` call)

```
You are VocaPlay Assistant, an AI tutor helping Vietnamese learners build English vocabulary.
You respond in a mix of English and Vietnamese — use Vietnamese to explain meanings and grammar,
use English for the vocab terms themselves.

You have three jobs:
1. BULK ADD WORDS: When the user gives you a list of words to add to a word set, parse them,
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
- If found: parse JSON, call `WordService.BulkAddAsync(wordSetId, words)`, strip the action block from the reply shown to the user, and populate the `action` field in the API response.
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
    │   │   │   ├── WordSet.cs
    │   │   │   ├── Word.cs
    │   │   │   ├── GameSession.cs
    │   │   │   └── ChatMessage.cs
    │   │   ├── Enums/
    │   │   │   ├── CefrLevel.cs              (A1 A2 B1 B2 C1 C2)
    │   │   │   └── WordType.cs               (Noun Verb Adjective …)
    │   │   ├── Interfaces/
    │   │   │   ├── Repositories/
    │   │   │   │   ├── IUserRepository.cs
    │   │   │   │   ├── IWordSetRepository.cs
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
    │   │   ├── WordSets/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateWordSetCommand.cs + Handler
    │   │   │   │   ├── UpdateWordSetCommand.cs + Handler
    │   │   │   │   └── DeleteWordSetCommand.cs + Handler
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetWordSetsQuery.cs + Handler
    │   │   │   │   └── GetWordSetByIdQuery.cs + Handler
    │   │   │   └── DTOs/
    │   │   │       ├── WordSetDto.cs
    │   │   │       └── WordSetDetailDto.cs
    │   │   ├── Words/
    │   │   │   ├── Commands/
    │   │   │   │   ├── AddWordCommand.cs + Handler
    │   │   │   │   ├── UpdateWordCommand.cs + Handler
    │   │   │   │   ├── DeleteWordCommand.cs + Handler
    │   │   │   │   └── BulkAddWordsCommand.cs + Handler
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
    │   │   │       ├── WordSetRepository.cs
    │   │   │       ├── WordRepository.cs
    │   │   │       ├── GameSessionRepository.cs
    │   │   │       └── ChatRepository.cs
    │   │   ├── Configurations/               (EF Fluent API per entity)
    │   │   │   ├── UserConfiguration.cs
    │   │   │   ├── WordSetConfiguration.cs
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
    │       │   ├── WordSetsController.cs
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
POST /wordsets/{setId}/words
  → WordsController.AddWord(AddWordCommand)
    → AddWordCommandHandler.Handle(command)
      → IWordSetRepository.GetByIdAsync()   (ownership check)
      → IWordRepository.AddAsync(word)
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
│   │   ├── wordsets.ts
│   │   ├── game.ts
│   │   └── chat.ts                 ← new
│   ├── context/
│   │   └── AuthContext.tsx
│   ├── pages/
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── DashboardPage.tsx
│   │   ├── WordSetDetailPage.tsx
│   │   └── GamePage.tsx
│   ├── components/
│   │   ├── layout/
│   │   │   ├── Navbar.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   ├── wordset/
│   │   │   ├── WordSetCard.tsx
│   │   │   ├── WordSetForm.tsx
│   │   │   └── WordTable.tsx       (columns: English, Vietnamese, Pronunciation, Level, Type, Example)
│   │   ├── word/
│   │   │   └── WordForm.tsx
│   │   ├── game/
│   │   │   ├── MatchingGame.tsx
│   │   │   └── GameResult.tsx
│   │   └── chat/                   ← new
│   │       ├── ChatWidget.tsx      (floating bubble, visible on all pages)
│   │       ├── ChatWindow.tsx      (message list + input)
│   │       └── ChatMessage.tsx     (single message bubble)
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   └── useChat.ts              ← new
│   ├── types/
│   │   └── index.ts
│   ├── App.tsx
│   └── main.tsx
├── index.html
├── tailwind.config.ts
├── vite.config.ts
└── package.json
```

---

## 8. Game mechanics — Matching game

- Grid of cards: left column = English (shuffled), right column = Vietnamese (shuffled independently).
- User clicks one English card + one Vietnamese card to attempt a match.
- Correct: both cards turn green and lock out.
- Wrong: both cards flash red and reset.
- Game ends when all pairs matched.
- Score = correct first-attempt matches / total pairs.
- Save via `POST /game/sessions` on completion.
- Minimum 4 pairs to start; recommend 6–12.

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
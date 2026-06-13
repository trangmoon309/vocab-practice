# agent-workflow.md — VocaPlay Agentic Development Workflow
> How humans and AI agents collaborate to build the vocab-practice repo.  
> Every phase ends with a **human approval gate** before the next phase begins.

---

## Overview

```
Human (Project Owner)
  └── Orchestrator Agent          ← coordinates all agents, enforces gates
        ├── Architect Agent       ← domain design, ERD, folder structure
        ├── .NET Expert Agent     ← Domain + Application + Infrastructure layers
        ├── React Expert Agent    ← Frontend SPA
        ├── QA Agent              ← tests, coverage, spec compliance
        └── Reviewer Agent        ← cross-cutting: security, perf, conventions
```

All agents share:
- `spec.md` — the single source of truth (always attached)
- `decisions.md` — running log of architectural decisions
- `agent.md` — their individual identity, tools, and permission scope
- `skill.md` files — reusable techniques they can call

---

## File structure (repo root)

```
vocab-practice/
├── .ai/                          ← all agent scaffolding lives here
│   ├── orchestrator.md           ← master workflow instructions
│   ├── agents/
│   │   ├── architect.agent.md
│   │   ├── dotnet-expert.agent.md
│   │   ├── react-expert.agent.md
│   │   ├── qa.agent.md
│   │   └── reviewer.agent.md
│   ├── skills/
│   │   ├── clean-architecture.skill.md
│   │   ├── ef-core-fluent.skill.md
│   │   ├── react-patterns.skill.md
│   │   ├── openai-integration.skill.md
│   │   └── supabase-connection.skill.md
│   ├── plugins/
│   │   ├── context7.plugin.md    ← fetch live docs (MS Docs, React docs, etc.)
│   │   └── spec-validator.plugin.md ← checks output against spec.md
│   ├── hooks/
│   │   ├── pre-phase.hook.md     ← runs before any phase starts
│   │   └── post-phase.hook.md    ← runs after phase output, before gate
│   └── decisions.md              ← append-only log
├── spec.md
├── VocaPlay.Api/                 ← .NET solution root
└── vocaplay-web/                 ← React app
```

---

## Agent definitions

### `orchestrator.md`
```
ROLE: Orchestrator
You coordinate the full build. You do NOT write code yourself.
Your job:
1. Read spec.md and decide which agent to activate for each task.
2. Attach the correct context (spec.md + relevant skill.md files) to each agent call.
3. After each agent responds, run the post-phase hook and present output to the human.
4. STOP and wait for explicit human approval ("approved", "lgtm", "yes") before proceeding.
5. If rejected, collect the human's feedback and re-run the agent with corrections.
6. Append every significant decision to decisions.md with date + rationale.

APPROVAL GATE PROTOCOL:
- Always end your message with: "--- GATE: Awaiting your approval to proceed to [next phase] ---"
- Never proceed past a gate on your own, even if the output looks correct.
- If the human says "skip" or "auto-approve", ask once to confirm before proceeding.
```

### `architect.agent.md`
```
ROLE: Architect
SKILLS: clean-architecture.skill.md, ef-core-fluent.skill.md
PLUGINS: context7 (for EF Core docs), spec-validator
PERMISSIONS:
  - READ: spec.md, decisions.md
  - WRITE: decisions.md (append only)
  - CANNOT: write code files, run commands

RESPONSIBILITIES:
- Produce EF Core entity classes with navigation properties
- Produce Fluent API configurations per entity
- Define AppDbContext
- Validate folder structure against spec.md
- Output ERD (mermaid erDiagram syntax)

OUTPUT FORMAT: markdown with clearly labelled code blocks per file.
Always end with: "Ready for Architect review gate."
```

### `dotnet-expert.agent.md`
```
ROLE: .NET Expert
SKILLS: clean-architecture.skill.md, ef-core-fluent.skill.md, openai-integration.skill.md, supabase-connection.skill.md
PLUGINS: context7 (MS Docs, NuGet), spec-validator
PERMISSIONS:
  - READ: spec.md, decisions.md, architect output
  - WRITE: VocaPlay.Domain/**, VocaPlay.Application/**, VocaPlay.Infrastructure/**, VocaPlay.Api/**
  - RUN: dotnet build, dotnet test (read results, do not auto-fix failures silently)
  - CANNOT: modify spec.md, modify React files

RESPONSIBILITIES:
- Implement in layer order: Domain → Application → Infrastructure → API
- Follow Clean Architecture strictly — no layer violations
- One file per response turn unless instructed otherwise
- Add XML doc comments on all public interfaces and handlers
- Never expose OpenAI errors to API consumers

OUTPUT FORMAT: one code block per file with full path as a comment on line 1.
Always end with: "Ready for .NET review gate."
```

### `react-expert.agent.md`
```
ROLE: React Expert
SKILLS: react-patterns.skill.md
PLUGINS: context7 (React docs, TailwindCSS docs), spec-validator
PERMISSIONS:
  - READ: spec.md, decisions.md, API contract section of spec.md
  - WRITE: vocaplay-web/src/**
  - CANNOT: modify .NET files, modify spec.md

RESPONSIBILITIES:
- Build pages and components matching spec.md folder structure
- Axios instance with auth interceptors (silent refresh on 401)
- ChatWidget as a floating bubble visible on all authenticated pages
- TypeScript strict mode — no `any` types
- Match API contract exactly (field names, endpoints, HTTP methods)

OUTPUT FORMAT: one code block per file with full path as comment on line 1.
Always end with: "Ready for React review gate."
```

### `qa.agent.md`
```
ROLE: QA Engineer
SKILLS: clean-architecture.skill.md, react-patterns.skill.md
PLUGINS: spec-validator
PERMISSIONS:
  - READ: all source files, spec.md
  - WRITE: VocaPlay.Api.Tests/**, VocaPlay.Application.Tests/**, e2e/**
  - RUN: dotnet test, npx playwright test (report results only)
  - CANNOT: modify production source files

RESPONSIBILITIES:
- xUnit handler tests (mock repositories via Moq)
- Testcontainers integration tests for API endpoints
- Playwright E2E for: register, login, add word, play game, use chatbot
- Flag any spec contract mismatches found in production code
- Report coverage percentage

OUTPUT FORMAT: test files + a QA report markdown summarising pass/fail and mismatches.
Always end with: "Ready for QA review gate."
```

### `reviewer.agent.md`
```
ROLE: Reviewer
SKILLS: clean-architecture.skill.md, ef-core-fluent.skill.md, react-patterns.skill.md
PLUGINS: spec-validator
PERMISSIONS:
  - READ: all files
  - WRITE: none (review comments only)
  - CANNOT: modify any file directly

RESPONSIBILITIES:
- Check Clean Architecture layer dependency violations
- Check JWT secret and API key are never hardcoded
- Check all API error responses return { message, statusCode } not stack traces
- Check React has no `any` types, no hardcoded API URLs (must use VITE_API_BASE_URL)
- Check EF migrations exist for all new entity fields
- Check bulk add deduplication logic is correct
- Check OpenAI system prompt matches spec.md

OUTPUT FORMAT: numbered list of findings with severity (BLOCKER / WARNING / SUGGESTION).
No blockers = approved. Blockers must be fixed before gate passes.
Always end with: "Reviewer verdict: [APPROVED / NEEDS FIXES]"
```

---

## Skills

### `clean-architecture.skill.md`
```
SKILL: Clean Architecture for .NET
When implementing a new feature, follow this checklist:
1. Define entity in Domain/Entities/ — no EF or external refs
2. Define repository interface in Domain/Interfaces/Repositories/
3. Define DTO in Application/{Feature}/DTOs/
4. Write Command or Query + Handler in Application/{Feature}/Commands or Queries/
5. Implement repository in Infrastructure/Persistence/Repositories/
6. Add EF config in Infrastructure/Configurations/
7. Add thin controller method in API/Controllers/ — call handler, return DTO
8. Register in DI: Application.AddApplication(), Infrastructure.AddInfrastructure()

LAYER VIOLATION CHECKS:
- Domain must not reference Application, Infrastructure, or API namespaces
- Application must not reference Infrastructure or API namespaces
- Infrastructure may reference Application (for interfaces) and Domain only
- API may reference Application and Infrastructure (for DI wiring only)
```

### `ef-core-fluent.skill.md`
```
SKILL: EF Core Fluent API configuration
- Always use IEntityTypeConfiguration<T> per entity, never OnModelCreating directly
- Guid PKs: builder.HasKey(e => e.Id); builder.Property(e => e.Id).ValueGeneratedNever();
- Required strings: .IsRequired().HasMaxLength(N)
- Optional strings: .HasMaxLength(N) (no IsRequired)
- Enums stored as strings: .HasConversion<string>()
- UTC datetimes: .HasDefaultValueSql("NOW()") on CreatedAt
- Cascade deletes: .OnDelete(DeleteBehavior.Cascade) explicitly on all FKs
- Unique index: builder.HasIndex(e => e.Email).IsUnique()
- Never use data annotations on entity classes — Fluent API only
```

### `react-patterns.skill.md`
```
SKILL: React patterns for VocaPlay
- All API calls go through src/api/*.ts — never fetch() directly in components
- useAuth() hook from context/AuthContext — never read localStorage directly in components
- Protected routes wrap all authenticated pages via ProtectedRoute component
- Loading states: show spinner while API call in-flight, never render partial data
- Error states: show inline error message below the form field or at top of page
- ChatWidget is rendered in App.tsx outside Router so it persists across navigation
- No prop drilling deeper than 2 levels — use context or co-locate state
- TypeScript: all API response shapes typed in src/types/index.ts
```

### `openai-integration.skill.md`
```
SKILL: OpenAI GPT-4o integration in .NET
- Use official OpenAI NuGet: OpenAI (v2+)
- Inject IOpenAIClient via DI — never instantiate directly in services
- Always wrap OpenAI calls in try/catch — return graceful fallback message on failure
- Sliding window: send last N messages from ChatMessage table as context
- Parse %%ACTION%%...%%END%% blocks AFTER stripping from user-visible reply
- Log OpenAI token usage to console in Development environment only
- Never log the full message content (PII risk)
```

### `supabase-connection.skill.md`
```
SKILL: Supabase PostgreSQL connection from .NET
- Use Npgsql EF Core provider: Npgsql.EntityFrameworkCore.PostgreSQL
- Connection string format: Host=db.[ref].supabase.co;Port=5432;Database=postgres;
  Username=postgres;Password=[pw];SSL Mode=Require;Trust Server Certificate=true
- For Railway: use Transaction mode port 6543 to avoid connection limits
- Enable connection pooling: Minimum Pool Size=1;Maximum Pool Size=20
- Run migrations at startup: app.Services.CreateScope() → dbContext.Database.MigrateAsync()
- Never store connection string in source — always env var
```

---

## Plugins

### `context7.plugin.md`
```
PLUGIN: Context7
PURPOSE: Fetch up-to-date documentation for libraries used in this project.
USE WHEN: An agent needs accurate API signatures, package versions, or breaking-change info.

Invocation pattern:
  [PLUGIN: context7] fetch docs for: <library> <version> <topic>

Examples:
  [PLUGIN: context7] fetch docs for: Microsoft.EntityFrameworkCore 8.0 HasConversion
  [PLUGIN: context7] fetch docs for: OpenAI dotnet v2 ChatCompletion streaming
  [PLUGIN: context7] fetch docs for: React Router v6 useNavigate

Agents that may invoke this: Architect, .NET Expert, React Expert
```

### `spec-validator.plugin.md`
```
PLUGIN: Spec Validator
PURPOSE: Check that agent output is consistent with spec.md before gate submission.
USE WHEN: Any agent is about to submit output for human review.

Checks performed:
  - API endpoint paths match spec.md API contract table
  - DTO field names match spec.md request/response shapes
  - Entity field names and types match spec.md data models
  - Enum values match spec.md (CefrLevel, WordType)
  - Folder paths match spec.md folder structure

Invocation pattern:
  [PLUGIN: spec-validator] validate: <description of output>

Output: list of PASS / FAIL items. Any FAIL must be fixed before gate.
Agents that may invoke this: all agents
```

---

## Hooks

### `pre-phase.hook.md`
```
HOOK: Pre-phase
RUNS: Before every agent activation

Actions:
1. Re-read spec.md (confirm no drift from last phase)
2. Re-read decisions.md (load all prior architectural choices)
3. Confirm the correct skill.md files are attached for this agent
4. State explicitly: "Starting [Phase N] — [Agent Name]. Spec version: [date of last spec edit]."
```

### `post-phase.hook.md`
```
HOOK: Post-phase
RUNS: After agent produces output, before presenting to human

Actions:
1. Run spec-validator plugin on the output
2. Run reviewer agent in lightweight mode (blockers only, no suggestions)
3. Summarise output for human: what was produced, file count, any auto-detected issues
4. Present the approval gate prompt:
   "--- GATE [N/total]: [Phase name] complete. Review above output.
    Type 'approved' to continue, or describe changes needed. ---"
```

---

## Orchestration phases + approval gates

```
PHASE 0 — Setup              → GATE 0
PHASE 1 — Architecture       → GATE 1
PHASE 2 — Domain layer        → GATE 2
PHASE 3 — Application layer   → GATE 3
PHASE 4 — Infrastructure      → GATE 4
PHASE 5 — API layer           → GATE 5
PHASE 6 — React frontend      → GATE 6
PHASE 7 — Chatbot             → GATE 7
PHASE 8 — Tests               → GATE 8
PHASE 9 — Reviewer pass       → GATE 9
PHASE 10 — DevOps / deploy    → GATE 10
```

### PHASE 0 — Setup
Agent: Orchestrator  
Task: Scaffold `.ai/` folder, confirm repo exists at `github.com/<you>/vocab-practice`, confirm Supabase project is created, confirm Railway + Vercel are linked.  
Output: checklist of prerequisites  
**GATE 0: Human confirms all infra is ready.**

---

### PHASE 1 — Architecture
Agent: Architect  
Skills: `clean-architecture.skill.md`, `ef-core-fluent.skill.md`  
Plugins: `context7`, `spec-validator`  
Task:
- ERD as mermaid `erDiagram`
- All 5 entity classes (`User`, `WordSet`, `Word`, `GameSession`, `ChatMessage`)
- All 5 EF Fluent API configs
- `AppDbContext.cs`
- `CefrLevel.cs` and `WordType.cs` enums
- All 5 repository interfaces (`IUserRepository`, etc.)
- `IAiChatService.cs` interface

Output: markdown with all files as code blocks  
**GATE 1: Human reviews entities, relationships, enum values. Must match spec.md data models exactly.**

---

### PHASE 2 — Domain layer
Agent: .NET Expert  
Skills: `clean-architecture.skill.md`  
Task: Create `VocaPlay.Domain` project. Copy approved entity classes, enums, interfaces, and exceptions from Phase 1. Add `Result<T>` wrapper. Confirm zero external dependencies in `.csproj`.  
Output: `VocaPlay.Domain.csproj` + all files  
**GATE 2: Human verifies no forbidden namespace imports. Domain must have zero NuGet dependencies.**

---

### PHASE 3 — Application layer
Agent: .NET Expert  
Skills: `clean-architecture.skill.md`  
Task: Create `VocaPlay.Application` project. Implement all Command/Query handlers for: Auth, WordSets, Words (including BulkAdd), Game, Chat. DTOs per feature. `ICurrentUserService` interface. `AddApplication()` DI extension.  
Output: all handler + DTO files  
**GATE 3: Human spot-checks 2-3 handlers for correctness. Reviewer runs layer violation check.**

---

### PHASE 4 — Infrastructure layer
Agent: .NET Expert  
Skills: `ef-core-fluent.skill.md`, `openai-integration.skill.md`, `supabase-connection.skill.md`  
Plugins: `context7`  
Task: Create `VocaPlay.Infrastructure` project. Implement all repositories. `OpenAiChatService.cs`. `JwtTokenService.cs`. EF migrations. `AddInfrastructure()` DI extension.  
Output: all infrastructure files + initial migration  
**GATE 4: Human checks migration file matches approved entities. Reviewer checks no secrets in code.**

---

### PHASE 5 — API layer
Agent: .NET Expert  
Skills: `clean-architecture.skill.md`  
Task: Create `VocaPlay.Api` project. All 5 controllers (Auth, WordSets, Words, Game, Chat). `ErrorHandlingMiddleware`. `CurrentUserService`. `Program.cs` with full DI wiring. `Dockerfile`. `appsettings.json`.  
Output: all API files  
**GATE 5: Human tests locally with `docker-compose up`. Smoke-test 3 endpoints.**

---

### PHASE 6 — React frontend
Agent: React Expert  
Skills: `react-patterns.skill.md`  
Plugins: `context7`, `spec-validator`  
Task: All pages, components, hooks, and API service files per spec.md folder structure. Axios instance with interceptors. Auth context. `ChatWidget` floating bubble. TypeScript types.  
Output: all frontend files  
**GATE 6: Human runs `npm run dev`, logs in, adds a word. Verify ChatWidget appears.**

---

### PHASE 7 — Chatbot (full integration)
Agents: .NET Expert + React Expert (coordinated)  
Skills: `openai-integration.skill.md`, `react-patterns.skill.md`  
Task:
- .NET Expert: `ChatService.cs` with GPT-4o call, action block parser, bulk-add trigger
- React Expert: `ChatWindow.tsx` message rendering, optimistic UI, action confirmation toast

Order: .NET Expert first → GATE 7a (human tests chatbot endpoint via Postman/curl) → React Expert → GATE 7b (human tests chatbot in UI)  
**GATE 7b: Human tests all 3 chatbot modes (bulk add, explain, quiz). Verify words appear in word set.**

---

### PHASE 8 — Tests
Agent: QA  
Skills: `clean-architecture.skill.md`, `react-patterns.skill.md`  
Plugins: `spec-validator`  
Task:
- xUnit handler tests for all Application layer handlers (mocked repos)
- Testcontainers integration tests for all API endpoints
- Playwright E2E: register → login → create word set → add words → play game → chatbot bulk add

Output: all test files + QA report (pass/fail + coverage %)  
**GATE 8: All tests green. Coverage ≥ 70% on Application layer. No spec mismatches.**

---

### PHASE 9 — Reviewer pass
Agent: Reviewer  
Plugins: `spec-validator`  
Task: Full review of all production code across all 4 .NET projects and React app.  
Checks: layer violations, hardcoded secrets, error response format, no `any` types, migrations complete, system prompt matches spec.md  
Output: numbered findings with severity  
**GATE 9: Zero BLOCKER findings. Human reviews all WARNINGs and decides to accept or fix.**

---

### PHASE 10 — DevOps / deploy
Agent: .NET Expert (Dockerfile) + Orchestrator (CI/CD)  
Task:
- Verify `Dockerfile` in `VocaPlay.Api/`
- Verify `docker-compose.yml` for local dev
- Verify `.github/workflows/backend.yml` and `frontend.yml`
- Provide Railway env var checklist
- Provide Vercel env var checklist
- Provide Supabase migration run instructions

Output: deployment checklist markdown  
**GATE 10: Human deploys to Railway + Vercel. Smoke-tests production URL. Project complete.**

---

## Permissions matrix

| Agent | Read spec | Write code | Run commands | Modify spec | Call plugins |
|---|---|---|---|---|---|
| Orchestrator | ✅ | ❌ | ❌ | ❌ | ✅ |
| Architect | ✅ | decisions.md only | ❌ | ❌ | context7, spec-validator |
| .NET Expert | ✅ | .NET files only | dotnet build/test | ❌ | context7, spec-validator |
| React Expert | ✅ | React files only | npm run build | ❌ | context7, spec-validator |
| QA | ✅ | test files only | test runners | ❌ | spec-validator |
| Reviewer | ✅ | ❌ | ❌ | ❌ | spec-validator |

---

## Approval gate cheatsheet

| Gate | What you're approving | Blocking criteria |
|---|---|---|
| 0 | Infra is ready | Supabase / Railway / Vercel not linked |
| 1 | Entity design | Wrong field types, missing relations |
| 2 | Domain project builds | Any external NuGet in Domain.csproj |
| 3 | Handlers are correct | Wrong business logic, missing auth check |
| 4 | Infrastructure + migrations | Missing migration, secret in code |
| 5 | API runs locally | 500 errors, wrong response shape |
| 6 | Frontend runs | Auth flow broken, ChatWidget missing |
| 7b | Chatbot works end-to-end | Bulk-add doesn't persist, quiz broken |
| 8 | All tests green | Any failing test, coverage < 70% |
| 9 | Code quality | Any BLOCKER from Reviewer |
| 10 | Production live | 500 on prod, env vars missing |
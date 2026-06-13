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

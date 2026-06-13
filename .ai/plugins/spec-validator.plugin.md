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

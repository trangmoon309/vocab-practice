HOOK: Post-phase
RUNS: After agent produces output, before presenting to human

Actions:
1. Run spec-validator plugin on the output
2. Run reviewer agent in lightweight mode (blockers only, no suggestions)
3. Summarise output for human: what was produced, file count, any auto-detected issues
4. Present the approval gate prompt:
   "--- GATE [N/total]: [Phase name] complete. Review above output.
    Type 'approved' to continue, or describe changes needed. ---"

## Agent Artifacts — Vision & Principles

Agent Artifacts are durable, structured outputs that agents create, read, reference, and evolve over time to accomplish goals. They turn ephemeral agent interactions into composable, reviewable, and auditable building blocks that teams and agents can trust.

### What is an Agent Artifact?
An Agent Artifact is a typed, versioned record with metadata and content that captures a meaningful unit of work or knowledge. Examples include plans, tasks, decisions, datasets, evaluations, code diffs, reports, and change proposals. Artifacts are:

- **Structured**: machine-parseable schemas for reliable automation
- **Readable**: human-friendly content for collaboration and review
- **Versioned**: immutable history with explicit revisions and lineage
- **Addressable**: stable IDs/URIs for linking and retrieval
- **Composable**: artifacts can reference and build on each other
- **Auditable**: provenance, signatures, and validation enable trust

### Why Agent Artifacts?
- **Reproducibility**: Re-run, simulate, or audit decisions with captured context.
- **Collaboration**: Hand off work between agents and humans with shared, reviewable state.
- **Autonomy with control**: Allow agents to act while preserving checkpoints and approvals.
- **Observability**: Understand what changed, why, and by whom across time.
- **Tool interoperability**: Consistent shape for tools, pipelines, and UIs to integrate.

### Core Capabilities
- **Typing & Schemas**: Each artifact declares `type`, `schemaVersion`, and validates `content`.
- **Lineage & References**: `parents`, `related`, and `references` form a provenance graph.
- **States & Workflow**: `status` lifecycles (e.g., draft → proposed → approved → executed → archived).
- **Signatures & Integrity**: Optional author signatures, checksums, and verification.
- **Attachments**: Binary or large payloads stored out-of-line with content-addressed links.
- **Policies**: Visibility, retention, and PII/secret handling baked into metadata.
- **Execution Hints**: Optional `runnable` blocks or `toolBindings` to enable automated follow-up.

### How Agents Use Artifacts
1. **Plan**: Produce a `Plan` artifact describing goals, constraints, and steps.
2. **Propose**: Suggest changes or actions as `ChangeProposal` artifacts with diffs or commands.
3. **Execute**: Emit `ExecutionLog`/`Task` artifacts as steps are performed with inputs/outputs.
4. **Evaluate**: Create `Evaluation` artifacts capturing tests, metrics, and results.
5. **Decide**: Record `Decision` artifacts explaining trade-offs and chosen paths.
6. **Publish**: Produce `Report`/`Summary` artifacts for stakeholders.
7. **Link**: Reference prior artifacts to carry context forward and enable traceability.

Agents prefer appending new artifacts over mutating existing ones. Updates are modeled as new versions with `parents: [prior-id]`. This preserves a clear audit trail and simplifies revert/rollback.

### Suggested Minimal Schema (conceptual)
This is a conceptual shape to guide implementations; exact fields can vary by system.

```json
{
  "id": "artifact://...",              
  "type": "Plan|Task|Decision|...",
  "schemaVersion": "1.0",
  "title": "Concise, human-readable title",
  "author": { "kind": "agent|human", "id": "...", "display": "..." },
  "createdAt": "2025-10-28T00:00:00Z",
  "status": "draft|proposed|approved|executed|archived",
  "parents": ["artifact://..."],
  "related": ["artifact://..."],
  "references": [{ "uri": "...", "label": "..." }],
  "content": { /* type-specific structured body */ },
  "attachments": [{ "name": "...", "uri": "blob://...", "sha256": "..." }],
  "integrity": { "checksum": "...", "signature": "..." },
  "policy": { "visibility": "public|internal|private", "retentionDays": 90 },
  "toolBindings": [{ "tool": "...", "operation": "...", "args": { } }],
  "labels": { "project": "...", "env": "prod|staging", "topic": "..." }
}
```

### Common Artifact Types
- **Plan**: Goals, constraints, steps, acceptance criteria.
- **Task**: A unit of executable work with inputs/outputs and completion state.
- **ChangeProposal**: Suggested code/config/content changes with diffs and rationale.
- **ExecutionLog**: Structured record of commands, tool calls, and results.
- **Evaluation**: Tests, metrics, and verdicts for quality gates.
- **Report**: Human-facing summary with links to underlying artifacts and evidence.

### Lifecycle & Governance
- **Creation**: Artifact is drafted by an agent or human.
- **Review**: Peers or governance agents validate, comment, and approve.
- **Execution**: Bound tools or agents act based on the artifact, generating follow-on artifacts.
- **Archival**: Final state is preserved for audit; sensitive data is redacted per policy.

Governance strategies may include:
- Required approvers by `type` or `labels`.
- Pre-merge quality checks (schema validation, tests, policy enforcement).
- Attestation (signatures) for critical changes.

### Trust, Safety, and Provenance
- **Integrity**: Content-addressed IDs or checksums detect tampering.
- **Attribution**: Authors and toolchains are recorded for accountability.
- **Least Exposure**: PII/secret minimization and redaction supported at the artifact layer.
- **Reproducibility**: Captured environment/tool versions enable re-running steps.

### Interoperability Patterns
- Store artifacts as JSON with clear schemas and stable IDs.
- Expose artifact registries via APIs for search, fetch, and lineage queries.
- Use hyperlinks/URIs to reference artifacts across systems and repos.
- Provide UIs that render artifacts richly while preserving raw access for automation.

### Example Scenario (End-to-End)
1. An agent generates a `Plan` artifact for “Migrate logging to Serilog”.
2. It creates a `ChangeProposal` with diffs and rationale, referencing the plan.
3. A governance agent validates tests and policies, emitting an `Evaluation` artifact.
4. A human approves; an execution agent applies changes, emitting `ExecutionLog` artifacts.
5. The agent produces a `Report` summarizing outcomes with links to all artifacts.

### Success Criteria
- Teams and agents can answer: What changed? Why? By whom? Based on what evidence?
- Artifacts enable safe autonomy: reversible actions with clear checkpoints.
- Observability improves: fewer “black-box” agent actions, more explainable outcomes.

### Next Steps
- Finalize core schemas for our most common artifact types.
- Stand up a lightweight registry and browser for search and lineage.
- Integrate artifact creation and linking into agent workflows.
- Add validations and policy checks for critical types (e.g., ChangeProposal, Evaluation).



## Why
Agents (LLMs and automation) need a safe, compact, and testable interface to read and manipulate hierarchical trees and their views. Today, we have strong primitives (model, view, controller, serializers) but no agent-facing contract for tools, safety, pagination, or audit.

## What Changes
- Add a new capability `agent-interface` defining agent-callable operations (read, search, expand/collapse, mutate, view export/import)
- Define JSON-serializable inputs/outputs, an error model, pagination and depth controls
- Add safety requirements: read-only mode, property update guards, role-based permissions, rate limits, batch limits
- Add concurrency requirements: version tokens, idempotency keys for mutating ops
- Add observability: audit logging for agent actions
- Transport-agnostic contract with a required in-process API for v1

## Impact
- Affected specs: `agent-interface` (new)
- Affected code: future API surface in controller/gateway layer (to be implemented under a separate change after approval)


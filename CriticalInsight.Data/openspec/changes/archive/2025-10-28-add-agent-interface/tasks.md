## 1. Specification
- [x] 1.1 Author `specs/agent-interface/spec.md` with v1 requirements and scenarios
- [x] 1.2 Validate spec with `openspec validate add-agent-interface --strict`

## 2. Design (optional for v1)
- [x] 2.1 Define transport notes (in-process API, optional HTTP gateway in future)

## 3. Implementation (separate change after approval)
- [x] 3.1 Expose agent tools surface (in-process API) mapping to controller/view/serializer
- [x] 3.2 Add permission checks (roles) and read-only mode
- [x] 3.3 Add version tokens and idempotency keys handling
- [x] 3.4 Add pagination/depth limits and payload shaping
- [x] 3.5 Add audit logging hooks
- [x] 3.6 Tests for all tool functions and safety constraints


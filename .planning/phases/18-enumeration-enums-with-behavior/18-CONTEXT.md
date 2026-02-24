# Phase 18: Enumeration - Enums with Behavior - Context

**Gathered:** 2026-02-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Replace plain C# enums (OrderStatus, ProductStatus) with Ardalis.SmartEnum types that encapsulate state transition rules and validation. Add generic EF Core and JSON converters in BuildingBlocks. Frontend API contracts remain unchanged (string values).

</domain>

<decisions>
## Implementation Decisions

### Order State Transitions
- Strictly linear happy path: Submitted → StockReserved → Paid → Confirmed → Shipped → Delivered
- Failed is reachable only from Submitted and StockReserved (early failures only)
- Cancelled is reachable from Submitted, StockReserved, and Paid (before Confirmed)
- Failed, Cancelled, and Delivered are terminal states (no outbound transitions)
- Only CanTransitionTo() method — no semantic helpers (IsCancellable, IsTerminal, etc.)

### Product Lifecycle Rules
- Draft → Published (publish)
- Published → Draft (unpublish for edits)
- Published → Archived (retire product)
- Draft → Archived (discard draft)
- Archived is terminal — cannot be re-published or returned to draft
- Transition rules only — no behavior helpers like IsVisibleToCustomers

### Invalid Transition Handling
- Throw a domain exception (not Result failure) on invalid transitions
- Exception message includes current state, attempted target, and list of valid transitions from current state
- Validation logic lives in the SmartEnum itself via TransitionTo(target) method
- Both paths available: TransitionTo() for validated transitions, direct set for edge cases (admin overrides, data migration)

### Enum Extensibility Pattern
- Use Ardalis.SmartEnum package (not custom-built)
- SmartEnum types are open (not sealed) — allow potential subclassing
- Generic EF Core ValueConverter and JsonConverter in BuildingBlocks/Common — any future SmartEnum works automatically
- Database storage details are not a concern for this phase — ignore migration complexity

### Claude's Discretion
- SmartEnum integer backing values (preserve existing or reassign)
- Exact placement of converters within BuildingBlocks structure
- How TransitionTo() interacts with existing entity methods
- Test organization and coverage strategy

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard Ardalis.SmartEnum approaches and patterns.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 18-enumeration-enums-with-behavior*
*Context gathered: 2026-02-24*

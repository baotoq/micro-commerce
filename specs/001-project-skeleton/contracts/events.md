# Integration Event Contract: Project Skeleton

Transport: the Dapr pub/sub component `pubsub`, backed by the Aspire Redis resource. Messages are
CloudEvents 1.0 (`datacontenttype: application/json`). Delivery is **at least once**, so consumers
MUST de-duplicate on `data.eventId` (Principle V, FR-012b).

## `sample-process-completed`

The `PublishSampleProcessCompleted` activity publishes this event after a `SampleProcess` is saved
as `Completed`.

`data` (`SampleProcessCompletedV1`):

```json
{
  "eventId": "sample-process-completed:0193f1c2-...",
  "sampleProcessId": "0193f1c2-...",
  "completedAt": "2026-11-05T09:00:10Z"
}
```

| Field | Type | Rules |
|-------|------|-------|
| `eventId` | string | Deterministic per process, so a retried publish has the same ID |
| `sampleProcessId` | UUID string | |
| `completedAt` | RFC 3339 timestamp (UTC) | |

**Consumers**

| Consumer | Effect | Idempotency |
|----------|--------|-------------|
| `sample-process-completed.record-receipt` (ApiService) | Inserts a `processed_events` row | Primary key (`consumer`, `message_id`). A duplicate returns `200` without changes. |

**Versioning**: A breaking change publishes a new type (`…V2`) on a new topic. Fields may be added to
V1, but never removed.

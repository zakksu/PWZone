# PW Companion — Integration Tests

Placeholder for end-to-end integration tests that mock the PW process.

## Planned coverage

- Full attach → poll → WebSocket broadcast pipeline with mocked memory backend
- Config hot-reload during active polling
- Graceful degradation when offsets are invalid

## Run

```bash
dotnet test PWCompanion.sln --filter "Category=Integration"
```

Unit tests live in `src/Companion/Tests/`.

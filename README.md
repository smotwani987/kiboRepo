# 🚀 Kibo SDET Challenge

Refactored the legacy Kibo API tests into a reusable **SDET testing framework / SDK**
with cleaner test design, polling, builders, observability, and GenAI-assisted edge
coverage.

![.NET](https://img.shields.io/badge/.NET-10-purple)
![Tests](https://img.shields.io/badge/Tests-15%20Passing-brightgreen)
![GenAI](https://img.shields.io/badge/GenAI-Documented-orange)

---

## ✅ What Was Improved

| Area | Before | After |
| --- | --- | --- |
| API calls | Direct `HttpClient` in tests | Reusable `KiboApiClient` |
| Test data | Inline JSON | Fluent builders |
| Waiting | `Thread.Sleep()` | Generic polling utility |
| Failure debugging | Basic assertions | Request/response diagnostics |
| Edge coverage | Basic happy path tests | GenAI-inspired destructive tests |
| Logging | Not available | Toggleable via environment variable |

---

## 🏗️ Project Structure

```text
src/
├── Kibo.MockApi/              # Mock API - not modified
└── Kibo.TestingFramework/     # Reusable testing SDK
    ├── KiboApiClient.cs
    ├── ApiResponse.cs
    ├── ApiResponseAssertions.cs
    ├── HttpDiagnostics.cs
    ├── OrderBuilder.cs
    ├── LineItemBuilder.cs
    └── Poller.cs

tests/
└── Kibo.LegacyTests/
    ├── OrderTests.cs
    ├── OrderEdgeCaseTests.cs
    └── ObservabilityTests.cs

PROMPT_LOG.md                 # AI usage documentation
```

---

## ▶️ How to Run

### Build

```bash
dotnet build
```

### Start Mock API

```bash
dotnet run --project src/Kibo.MockApi
```

### Run Tests

```bash
dotnet test Kibo.SDET.Challenge.sln
```

---

## ⚙️ Environment Variables

| Variable | Purpose |
| --- | --- |
| `KIBO_BASE_URL` | Override API base URL. Default: `http://localhost:5000` |
| `KIBO_API_LOGGING=true` | Enables request/response console logging |

Example:

```bash
KIBO_API_LOGGING=true dotnet test Kibo.SDET.Challenge.sln
```

---

## 📋 Task Coverage

| Task | Implementation |
| --- | --- |
| Task 1 | Reusable API client and response wrapper |
| Task 2 | Fluent `OrderBuilder` and `LineItemBuilder` |
| Task 3 | Generic async `Poller` replacing `Thread.Sleep()` |
| Task 4 | 7 GenAI-inspired edge/destructive tests |
| Task 5 | `PROMPT_LOG.md` documenting AI usage |
| Task 6 | Request/response diagnostics, timing, correlation ID, and diagnostic assertions |

---

## 🔍 Observability

The framework captures useful diagnostics for every API call:

```text
Request method + URL
Request headers + body
Response status + headers + body
Elapsed time
Correlation ID
```

Logging is **off by default** to keep CI output clean, but diagnostics remain
available through `ApiResponse` and failed assertions.

---

## 🧪 Edge Cases Covered

| Edge Case | Purpose |
| --- | --- |
| SQL injection in `x-kibo-tenant` | Security validation |
| Zero / negative pricing | Financial validation |
| Extremely long `customerEmail` | Input length validation |
| Empty `lineItems` | Required data validation |
| Missing required fields | Contract validation |
| Unicode / special characters | Encoding behavior |
| Oversized payload | Payload handling |

Some destructive tests assert the mock API's current behavior and document
validation gaps in comments instead of intentionally failing the suite.

---

## 🤖 AI Usage

AI was used for scaffolding, review, edge-case brainstorming, observability design,
and documentation. Outputs were manually reviewed and refined before committing.

See:

```text
PROMPT_LOG.md
```

---

## ✅ Final Validation

```text
dotnet build                         ✅ Passed
dotnet test Kibo.SDET.Challenge.sln  ✅ 15 tests passed
```

Additional checks:

```text
No direct HttpClient usage in tests
No Thread.Sleep usage
No raw JSON serialization in tests
No build artifacts committed
Kibo.MockApi source not modified
```

# Kibo SDET Challenge

Refactored legacy API tests into a reusable testing framework / SDK for the Kibo mock fulfillment API.

## What Was Built

- Reusable API client
- Fluent order and line item data builders
- Generic async polling utility
- GenAI-inspired edge case tests
- Diagnostics-rich API assertions
- Request/response observability
- Timing capture
- Correlation ID support
- Toggleable logging

## Folder Structure

- `src/Kibo.MockApi` - mock API, not modified
- `src/Kibo.TestingFramework` - reusable testing SDK
- `tests/Kibo.LegacyTests` - refactored API tests
- `PROMPT_LOG.md` - AI usage documentation

## How To Run

```bash
dotnet build
dotnet run --project src/Kibo.MockApi
dotnet test Kibo.SDET.Challenge.sln
```

## Environment Variables

- `KIBO_BASE_URL` - intended base URL override for environments that do not use `http://localhost:5000`
- `KIBO_API_LOGGING=true` - enables API request/response console logging

## Task Coverage

- Task 1: Platform Shift / API Client
- Task 2: Fluent Builders
- Task 3: Polling Utility
- Task 4: AI-Driven Edge Cases
- Task 5: Prompt Log
- Task 6: Observability & Diagnostics

## Edge Cases Covered

- SQL injection in `x-kibo-tenant`
- Zero/negative pricing
- Extremely long `customerEmail`
- Empty `lineItems`
- Missing required fields
- Unicode/special characters
- Oversized payload

Some destructive tests assert the mock API's current behavior and document validation gaps in comments, instead of intentionally failing the suite.

## Observability

The framework captures request method, URL, headers, body, response status, response headers, response body, elapsed milliseconds, and correlation ID. Diagnostics are available on `ApiResponse`, logging is off by default, and `KIBO_API_LOGGING=true` enables console output. Failed status assertions include request/response diagnostics for CI debugging.

## AI Usage

AI was used for scaffolding, review, edge case brainstorming, and documentation. Outputs were manually reviewed and refined to keep changes scoped, framework-driven, and aligned with the assignment criteria.

## Final Validation

- `dotnet build` passed
- `dotnet test Kibo.SDET.Challenge.sln` passed
- 15 tests passing

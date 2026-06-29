# AI Prompt Log - Kibo SDET Challenge

## Prompt 1 - Task 1 Platform Shift / API Client

**Tool Used:** ChatGPT + Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I asked AI to inspect the legacy test suite, identify anti-patterns, and implement the first API-client refactor slice.

**What I Kept:**  
The reusable `KiboApiClient`, `ApiResponse`, and `HttpDiagnostics` foundation; project reference from tests to the framework; one initial happy-path refactor.

**What I Reviewed / Changed:**  
I verified no changes were made under `src/Kibo.MockApi`, kept the first change small, and confirmed tests stayed green before expanding the refactor.

## Prompt 2 - Task 2 Fluent Builders

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I asked AI to scaffold fluent builders for orders and line items so tests could stop constructing raw JSON or anonymous payloads.

**What I Kept:**  
`OrderBuilder`, `LineItemBuilder`, valid defaults, chainable overrides, support for empty/invalid edge data later.

**What I Reviewed / Changed:**  
I ensured the builders remained permissive enough for destructive tests, then refactored existing tests to use builder-created payloads.

## Prompt 3 - API Client Refactor Review Finding

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
After Task 2, I asked AI to review `OrderTests.cs` for remaining direct `HttpClient` usage.

**What I Kept:**  
Additional `KiboApiClient` methods such as `CreateOrderAsync`, `GetOrderAsync`, and `includeTenantHeader: false` for missing-header scenarios.

**What I Reviewed / Changed:**  
I rejected leaving mixed raw HTTP and framework calls in the same test class. The remaining tests were refactored so `OrderTests.cs` no longer used direct `HttpClient`, raw JSON, or hardcoded endpoint construction.

## Prompt 4 - Task 3 Polling Utility

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I asked AI to replace `Thread.Sleep(6000)` with a reusable async polling utility.

**What I Kept:**  
A generic `Poller.UntilAsync` method with configurable timeout and interval, returning the matched result.

**What I Reviewed / Changed:**  
I removed old sleep comments, verified `Thread.Sleep` no longer appeared in tests, and confirmed the timeout message includes the last observed result.

## Prompt 5 - Task 4 AI-Driven Edge Cases

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I used AI to brainstorm destructive API scenarios, then manually aligned the list to the official assignment examples.

**What I Kept:**  
Tests for SQL injection tenant headers, zero/negative pricing, extremely long emails, empty line items, missing fields, Unicode/special characters, and oversized payloads.

**What I Reviewed / Changed:**  
I made the tests assert current mock API behavior rather than ideal behavior, documented validation gaps in comments, and ensured all tests used the framework instead of raw HTTP.

## Prompt 6 - Task 6 Observability and Diagnostics

**Tool Used:** Codex  
**Reasoning Mode:** High  

**Prompt Summary:**  
I asked AI to help design framework-level observability: request/response capture, elapsed timing, correlation IDs, toggleable logging, and diagnostics-rich assertions.

**What I Kept:**  
Diagnostics on every `ApiResponse`, request and response logs, elapsed milliseconds, correlation ID propagation, `KIBO_API_LOGGING=true`, and reusable status assertion helpers.

**What I Reviewed / Changed:**  
I ensured logging is off by default and non-intrusive, while diagnostics remain available for failed assertions. I added observability tests for timing, correlation ID exposure, failure-message content, and the default logging behavior.

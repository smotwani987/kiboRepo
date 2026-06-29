# AI Prompt Log — Kibo SDET Challenge

## Prompt 1 — Repository Inspection and Task 1 API Client Wrapper

**Tool Used:** ChatGPT + Codex  
**Reasoning Mode:** Medium  
**Prompt Summary:**  
I asked AI to inspect the Kibo SDET challenge repository, identify the current legacy API testing issues, and implement only Task 1 by introducing a reusable API client wrapper.

**AI Suggested / Implemented:**  
- Added `ApiResponse.cs`
- Added `HttpDiagnostics.cs`
- Added `KiboApiClient.cs`
- Updated `Kibo.LegacyTests.csproj` with framework project reference
- Refactored one happy path test in `OrderTests.cs` to use `KiboApiClient`

**How I Reviewed It:**  
I verified that:
- No files were changed under `src/Kibo.MockApi`
- The client captures status code, response body, request URI, request body, elapsed time, and correlation ID
- Only one test was refactored first to keep the change small and safe
- `dotnet test Kibo.SDET.Challenge.sln` passed with 4 tests passing

**Final Outcome:**  
Task 1 foundation was completed successfully. The framework now has a reusable API client layer instead of direct duplicated `HttpClient` usage in tests.

## Prompt 2 — Task 2 Fluent Test Data Builders

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I asked AI to implement fluent test data builders for orders and line items so tests do not depend on raw JSON strings or repeated object creation logic.

**AI Suggested / Implemented:**  
- Added `OrderBuilder.cs`
- Added `LineItemBuilder.cs`
- Refactored `OrderTests.cs` to use builders
- Added valid default test data
- Added chainable builder methods for overriding order and line item data

**How I Reviewed It:**  
I verified that:
- Builders provide valid default order data
- Tests are more readable
- Raw JSON duplication is reduced
- No changes were made under `src/Kibo.MockApi`
- `dotnet test Kibo.SDET.Challenge.sln` passed with 4 tests passing

**Final Outcome:**  
Task 2 was completed successfully. Test data creation is now reusable, readable, and ready for edge-case testing.

## Prompt 2 — Fluent Builders and Review Finding

**Tool Used:** Codex  
**Reasoning Mode:** Medium  
**Prompt Summary:**  
I asked AI to implement fluent `OrderBuilder` and `LineItemBuilder` classes and refactor existing tests to use builders where applicable.

**AI Suggested / Implemented:**  
- Added `OrderBuilder.cs`
- Added `LineItemBuilder.cs`
- Updated `OrderTests.cs`
- Verified `dotnet test Kibo.SDET.Challenge.sln` passed with 4 tests

**Manual Review Finding:**  
After reviewing the changed `OrderTests.cs`, I noticed that three tests were still directly using `HttpClient`. This meant the Task 1 refactoring was only partially applied.

**Action Taken:**  
I treated this as a review gap and planned a follow-up cleanup task to remove remaining direct `HttpClient` usage from legacy tests.

## Prompt 3 — Replace Fixed Sleep with Polling Utility

**Tool Used:** Codex  
**Reasoning Mode:** Medium  
**Prompt Summary:**  
I asked AI to replace brittle `Thread.Sleep()` usage with a reusable async polling utility.

**AI Suggested / Implemented:**  
- Added `Poller.cs`
- Refactored `GetOrder_AfterCreation_StatusBecomesReadyForFulfillment`
- Removed `Thread.Sleep()` from tests

**How I Reviewed It:**  
I verified there was no remaining `Thread.Sleep` usage by running:
`grep -R "Thread.Sleep" tests/Kibo.LegacyTests`

**Final Outcome:**  
The test suite passed with 4 tests. The async status test is now more reliable and framework-driven.

## Prompt 4 — GenAI-Inspired Edge Case Tests

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I used GenAI to brainstorm destructive and edge-case API test scenarios for the order endpoints. The initial implementation covered several scenarios, but I manually reviewed it against the official assignment examples and asked AI to realign the tests with the stated evaluation criteria.

**Review and Refinement:**  
After reviewing the candidate instructions, I noticed the test coverage needed to explicitly include the sample areas mentioned in the assignment:
- SQL injection attempts in the `x-kibo-tenant` header
- Negative or zero pricing in line items
- Extremely long strings in `customerEmail`
- Empty `lineItems` array
- Missing required fields
- Unicode / special characters
- Oversized payloads

I then asked Codex to update `OrderEdgeCaseTests.cs` so the tests clearly mapped to these examples while still using the reusable framework instead of raw HTTP.

**Final Outcome:**  
Added 7 GenAI-inspired edge-case tests. The tests document the mock API's current behavior, keep the suite green, and show security, validation, financial, encoding, and payload-size coverage.

**Validation:**  
I verified:
- No direct `HttpClient` usage in tests
- No `Thread.Sleep`
- No raw `JsonSerializer.Serialize` usage in tests
- `dotnet test Kibo.SDET.Challenge.sln` passed with 11 tests

## Prompt 6 — Test Observability and Diagnostics

**Tool Used:** Codex  
**Reasoning Mode:** Medium  

**Prompt Summary:**  
I asked AI to help scaffold the observability design for API tests, including request/response capture, timing, correlation IDs, and diagnostics-rich assertion failures.

**How I Reviewed It:**  
I ensured logging is toggleable and non-intrusive through `KIBO_API_LOGGING`, while diagnostics remain available on every `ApiResponse` even when console logging is disabled. I also verified timing and correlation data are exposed to support CI failure debugging.

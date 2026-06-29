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
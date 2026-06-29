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
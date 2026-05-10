# PrintHub QA - Testing Infrastructure

## Overview

This directory contains the testing infrastructure for PrintHub, including smoke tests, visual regression checks, and integration readiness gates.

## Quick Start

```bash
# Restore and build
dotnet restore PrintHub.sln
dotnet build PrintHub.sln --configuration Release

# Run all tests
dotnet test PrintHub.sln

# Run specific test categories
dotnet test --filter "Category=Smoke"    # Smoke tests only
dotnet test --filter "Category=Unit"     # Unit tests only  
dotnet test --filter "Category=Visual"   # Visual regression tests
dotnet test --filter "Category=Integration"  # Integration readiness
```

## Test Categories

### 🔥 Smoke Tests (`Category=Smoke`)
Fast checks that verify core API endpoints respond correctly without requiring the full application stack.

**Location:** `PrintHub.Tests/Smoke/`

**Key Classes:**
- `SmokeTestBase` - Base class for smoke tests
- `SmokeTestRunner` - Discovers and runs smoke tests from configuration
- `ApiSmokeTests` - Pre-defined smoke test cases for the API

**Configuration:**
```json
{
  "SmokeTests": {
    "Enabled": true,
    "ApiBaseUrl": "http://localhost:5000",
    "TimeoutSeconds": 30,
    "Endpoints": [
      { "Name": "Health", "Path": "/health", "ExpectedStatus": 200 },
      { "Name": "Products.List", "Path": "/api/products", "ExpectedStatus": 401 }
    ]
  }
}
```

**Running Smoke Tests:**
```bash
dotnet test --filter "Category=Smoke"
```

### 👁️ Visual Regression Tests (`Category=Visual`)
Checks that the UI renders correctly across different browsers and viewports.

**Location:** `PrintHub.Tests/Visual/`

**Key Classes:**
- `VisualRegressionTests` - Visual test cases using Playwright
- `VisualCheckRunner` - Runner for visual checks with baseline comparison

**Configuration:**
```json
{
  "VisualChecks": {
    "Enabled": false,
    "ScreenshotsPath": "./test-output/screenshots",
    "Browsers": ["chromium"],
    "Viewports": [
      { "Width": 1280, "Height": 720, "Name": "desktop" },
      { "Width": 375, "Height": 812, "Name": "mobile" }
    ]
  }
}
```

**Note:** Visual checks require:
1. Playwright browsers installed: `npx playwright install`
2. Visual checks enabled in config
3. Running API for page screenshots

### 🚦 Integration Readiness Gates (`Category=Integration`)
Evaluates whether the system is ready for integration/deployment.

**Location:** `PrintHub.Tests/Integration/`

**Gates:**
1. **Smoke Tests Gate** - All smoke tests must pass
2. **Unit Tests Gate** - All unit tests must pass  
3. **Visual Checks Gate** - All visual checks pass (optional)
4. **Security Gate** - No critical security issues found
5. **Coverage Gate** - Code coverage meets threshold

**Configuration:**
```json
{
  "IntegrationGates": {
    "RequireAllSmokeTestsPass": true,
    "RequireUnitTestsPass": true,
    "RequireVisualChecksPass": false,
    "MinCoveragePercent": 0,
    "BlockOnSecurityScan": true
  }
}
```

**Running Integration Gates:**
```bash
dotnet test --filter "Category=Integration"
```

## Configuration Files

| File | Purpose |
|------|---------|
| `appsettings.Testing.json` | Test configuration for all test types |
| `.github/workflows/ci.yml` | GitHub Actions CI pipeline |
| `.github/workflows/integration-gate.yml` | Dedicated integration gate workflow |

## GitHub Actions Integration

### CI Pipeline (ci.yml)
Runs on every push and PR:
1. Build & Unit Tests
2. Integration Gates
3. Code Quality Checks
4. Security Scan

### Integration Gate Workflow (integration-gate.yml)
Dedicated workflow that evaluates all readiness gates:
1. Smoke Tests Gate
2. Unit Tests Gate  
3. Integration Readiness Assessment
4. Gate Decision

## Adding New Tests

### Adding a Smoke Test
```csharp
public class ApiSmokeTests : IClassFixture<SmokeTestOptions>
{
    [Fact]
    [Trait("Priority", "High")]
    public async Task MyEndpoint_ReturnsOk()
    {
        var options = SmokeTestRunner.LoadFromConfiguration();
        var runner = new SmokeTestRunner(options);
        var results = await runner.RunAllTestsAsync();
        
        var result = results.FirstOrDefault(r => r.TestName == "MyEndpoint");
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }
}
```

### Adding a Visual Check
```csharp
public class VisualRegressionTests : IClassFixture<VisualCheckOptions>
{
    [SkippableFact]
    [Trait("Category", "Visual")]
    public async Task MyPage_RendersCorrectly()
    {
        Skip.If(!_options.Enabled, "Visual checks disabled");
        
        var page = await _browser.NewPageAsync();
        await page.SetViewportSizeAsync(1280, 720);
        await page.GotoAsync("http://localhost:3000/my-page");
        
        // Assert page loads correctly
        var title = await page.TitleAsync();
        title.Should().NotBeNullOrEmpty();
        
        await page.CloseAsync();
    }
}
```

## Test Output

Results are saved to:
- `test-output/smoke-results.json` - Smoke test results
- `test-output/integration-gates.json` - Integration gate report
- `test-output/screenshots/` - Visual regression screenshots

## Triage

### Smoke Test Failures
1. Check if API is running
2. Verify endpoint paths in configuration
3. Check authentication requirements
4. Review response body in JSON output

### Visual Check Failures
1. Review screenshot diffs in `test-output/screenshots/diff/`
2. Update baselines with `UpdateBaselines: true` if changes are intentional
3. Check for console errors in page

### Integration Gate Failures
1. Review individual gate results in JSON report
2. Check specific gate logs for details
3. Verify all prerequisites are met
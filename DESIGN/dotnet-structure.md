# PrintHub - .NET Solution Structure

## Overview

PrintHub uses a clean architecture pattern with .NET 8. The solution is organized into focused projects with clear dependencies.

---

## Solution Layout

```
PrintHub/
├── DESIGN/                          # Architecture & design docs
├── src/
│   ├── PrintHub.API/                 # ASP.NET Core Web API
│   ├── PrintHub.Core/                # Domain entities, interfaces
│   ├── PrintHub.Infrastructure/      # EF Core, Azure, External APIs
│   ├── PrintHub.Worker/              # Azure Functions (background jobs)
│   └── PrintHub.Tests/               # Unit tests
├── PrintHub.sln                      # Solution file
└── README.md
```

---

## Project Dependencies

```
PrintHub.API
    └── PrintHub.Core
    └── PrintHub.Infrastructure

PrintHub.Worker
    └── PrintHub.Core
    └── PrintHub.Infrastructure

PrintHub.Tests
    └── PrintHub.Core
    └── PrintHub.Infrastructure
    └── PrintHub.API
```

---

## PrintHub.Core

Contains all domain logic with **no external dependencies**.

```
PrintHub.Core/
├── Entities/
│   ├── User.cs
│   ├── Shop.cs
│   ├── Product.cs
│   ├── Part.cs
│   ├── ProductPart.cs
│   ├── PrintFile.cs
│   ├── PrintFileVersion.cs
│   ├── PrintJob.cs
│   ├── PrintJobItem.cs
│   ├── PersonalizedOrder.cs
│   ├── InventoryMovement.cs
│   └── CostRecord.cs
├── Enums/
│   ├── PrintJobStatus.cs
│   ├── PrintJobItemStatus.cs
│   ├── PersonalizedOrderStatus.cs
│   └── ShopProvider.cs
├── Interfaces/
│   ├── Repositories/
│   │   ├── IUserRepository.cs
│   │   ├── IShopRepository.cs
│   │   ├── IProductRepository.cs
│   │   ├── IPartRepository.cs
│   │   └── IPrintJobRepository.cs
│   └── Services/
│       ├── IProductService.cs
│       ├── IPrintQueueService.cs
│       ├── IEtsyService.cs
│       ├── IBambuService.cs
│       └── IInventoryService.cs
└── Services/
    ├── PrintQueueResolutionService.cs  # Shared part consolidation logic
    └── InventoryCalculationService.cs
```

---

## PrintHub.Infrastructure

Implements all interfaces defined in Core. Depends on Azure SDKs, EF Core, HTTP clients.

```
PrintHub.Infrastructure/
├── Data/
│   ├── PrintHubDbContext.cs          # EF Core DbContext
│   └── CosmosDb/
│       └── CosmosDbConfiguration.cs
├── Repositories/
│   ├── UserRepository.cs
│   ├── ShopRepository.cs
│   ├── ProductRepository.cs
│   ├── PartRepository.cs
│   └── PrintJobRepository.cs
├── Services/
│   ├── Etsy/
│   │   ├── EtsyApiClient.cs
│   │   └── EtsyOAuthService.cs
│   ├── Bambu/
│   │   ├── BambuConnectClient.cs
│   │   └── BambuCloudClient.cs
│   └── Storage/
│       └── AzureBlobStorageService.cs
├── Configuration/
│   ├── EtsyOptions.cs
│   ├── AzureStorageOptions.cs
│   └── BambuOptions.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

---

## PrintHub.API

ASP.NET Core 8 Web API project.

```
PrintHub.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── ShopsController.cs
│   ├── ProductsController.cs
│   ├── PartsController.cs
│   ├── QueueController.cs
│   ├── JobsController.cs
│   ├── PrintersController.cs
│   └── InsightsController.cs
├── Middleware/
│   ├── ExceptionHandlerMiddleware.cs
│   └── RateLimitingMiddleware.cs
├── Filters/
│   └── ValidateModelAttribute.cs
├── Requests/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   └── LoginRequest.cs
│   ├── Products/
│   │   ├── CreateProductRequest.cs
│   │   └── UpdateProductRequest.cs
│   ├── Queue/
│   │   ├── AddToQueueRequest.cs
│   │   └── AddPersonalizedToQueueRequest.cs
│   └── Printers/
│       ├── RegisterBambuPrinterRequest.cs
│       └── RegisterKlipperPrinterRequest.cs
├── Responses/
│   ├── ErrorResponse.cs
│   ├── ProductResponse.cs
│   ├── QueueResponse.cs
│   └── InsightDashboardResponse.cs
├── Program.cs
└── appsettings.json
```

---

## PrintHub.Worker

Azure Functions for background processing.

```
PrintHub.Worker/
├── Functions/
│   ├── EtsyOrderSyncFunction.cs      # Poll Etsy for new orders
│   ├── InventoryAlertFunction.cs      # Check low stock, send alerts
│   ├── PrintJobStatusFunction.cs      # Monitor Bambu job progress
│   └── CleanupFunction.cs             # Purge old files per retention policy
├── host.json
└── local.settings.json
```

---

## PrintHub.Tests

xUnit tests with Moq for mocking.

```
PrintHub.Tests/
├── Services/
│   ├── PrintQueueResolutionServiceTests.cs
│   ├── InventoryCalculationServiceTests.cs
│   └── ProductServiceTests.cs
├── Controllers/
│   ├── ProductsControllerTests.cs
│   └── QueueControllerTests.cs
└── TestHelpers/
    ├── Fixtures/
    └── Builders/
```

---

## Key NuGet Packages

| Project | Packages |
|---------|----------|
| PrintHub.Core | (none - pure C#) |
| PrintHub.Infrastructure | Microsoft.EntityFrameworkCore.Cosmos, Azure.Storage.Blobs, Microsoft.Extensions.Http |
| PrintHub.API | Microsoft.AspNetCore.Authentication.JwtBearer, Swashbuckle.AspNetCore, AspNetCoreRateLimit |
| PrintHub.Worker | Microsoft.Azure.Functions.Worker, Microsoft.Extensions.Azure |
| PrintHub.Tests | xUnit, Moq, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing |

---

## Configuration (appsettings.json)

```json
{
  "Etsy": {
    "ClientId": "",
    "ClientSecret": "",
    "CallbackUrl": "https://app.printhub.example.com/auth/etsy/callback",
    "Scopes": "listings_rw transactions profile"
  },
  "Bambu": {
    "ApiBaseUrl": "https://api.bambulab.com",
    "AppKey": "",
    "AppSecret": ""
  },
  "Azure": {
    "BlobStorage": {
      "ConnectionString": "",
      "ContainerName": "printfiles",
      "UseManagedIdentity": true
    },
    "CosmosDb": {
      "ConnectionString": "",
      "DatabaseName": "PrintHub"
    }
  },
  "Auth": {
    "Jwt": {
      "Secret": "",
      "Issuer": "PrintHub",
      "Audience": "PrintHubApp",
      "ExpirationMinutes": 60
    }
  }
}
```

---

## Running Locally

```bash
# Restore and build
dotnet restore PrintHub.sln
dotnet build PrintHub.sln

# Run API
cd src/PrintHub.API
dotnet run

# Run tests
dotnet test PrintHub.sln
```

---

## Azure Deployment (Bicep concepts)

- App Service or Container Apps for API
- Azure Functions for Worker
- Cosmos DB (serverless mode for startup)
- Blob Storage v2
- Azure AD B2C for authentication
- Application Gateway + WAF for security
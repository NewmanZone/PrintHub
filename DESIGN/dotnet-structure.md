# PrintHub - .NET Project Structure

## Overview

The backend uses a clean architecture layout. Phase 1 implements workspace-scoped Etsy file preparation. Printer adapters and direct print execution are later-phase modules and should not be required for Phase 1 builds or tests.

## Solution Layout

```text
PrintHub.sln
|-- src/
|   |-- PrintHub.Api/
|   |-- PrintHub.Core/
|   |-- PrintHub.Infrastructure/
|   `-- PrintHub.Worker/
`-- tests/
    |-- PrintHub.Tests/
    |-- PrintHub.Core.Tests/
    `-- PrintHub.Infrastructure.Tests/
```

## PrintHub.Core

Domain entities, interfaces, and business logic.

```text
PrintHub.Core/
|-- Entities/
|   |-- User.cs
|   |-- Workspace.cs
|   |-- WorkspaceMember.cs
|   |-- Shop.cs
|   |-- Product.cs
|   |-- Part.cs
|   |-- ProductPart.cs
|   |-- PrintFile.cs
|   |-- PrintFileVersion.cs
|   |-- EtsyOrder.cs
|   |-- EtsyOrderItem.cs
|   |-- PreparationBundle.cs
|   |-- PreparationBundleItem.cs
|   `-- AuditEvent.cs
|-- Enums/
|   |-- WorkspaceRole.cs
|   |-- EtsyOrderStatus.cs
|   |-- OrderItemPreparationStatus.cs
|   `-- PreparationBundleStatus.cs
|-- Interfaces/
|   |-- Repositories/
|   |   |-- IUserRepository.cs
|   |   |-- IWorkspaceRepository.cs
|   |   |-- IShopRepository.cs
|   |   |-- IProductRepository.cs
|   |   |-- IPartRepository.cs
|   |   |-- IFileRepository.cs
|   |   |-- IOrderRepository.cs
|   |   `-- IPreparationBundleRepository.cs
|   `-- Services/
|       |-- ICurrentUserContext.cs
|       |-- IWorkspaceAuthorizationService.cs
|       |-- IEtsyService.cs
|       |-- IFileStorageService.cs
|       `-- IPreparationBundleService.cs
`-- Services/
    |-- WorkspaceAuthorizationService.cs
    `-- PreparationBundleService.cs
```

## PrintHub.Infrastructure

External integrations and persistence.

```text
PrintHub.Infrastructure/
|-- Data/
|   |-- PrintHubDbContext.cs
|   `-- CosmosOptions.cs
|-- Repositories/
|   |-- UserRepository.cs
|   |-- WorkspaceRepository.cs
|   |-- ShopRepository.cs
|   |-- ProductRepository.cs
|   |-- PartRepository.cs
|   |-- FileRepository.cs
|   |-- OrderRepository.cs
|   `-- PreparationBundleRepository.cs
|-- Services/
|   |-- Etsy/
|   |   |-- EtsyClient.cs
|   |   |-- EtsyOptions.cs
|   |   `-- EtsySyncService.cs
|   |-- Files/
|   |   |-- AzureBlobFileStorageService.cs
|   |   `-- FileValidationService.cs
|   `-- Bundles/
|       `-- BundleArchiveService.cs
`-- DependencyInjection.cs
```

Later phase printer integrations should live under `Services/Printers/` behind adapter interfaces after Phase 1.

## PrintHub.Api

ASP.NET Core 8 Web API.

```text
PrintHub.Api/
|-- Controllers/
|   |-- AuthController.cs
|   |-- WorkspacesController.cs
|   |-- MembersController.cs
|   |-- ShopsController.cs
|   |-- ProductsController.cs
|   |-- PartsController.cs
|   |-- FilesController.cs
|   |-- OrdersController.cs
|   `-- PreparationBundlesController.cs
|-- Middleware/
|   |-- ErrorHandlingMiddleware.cs
|   `-- RequestLoggingMiddleware.cs
|-- Models/
|   |-- Auth/
|   |   `-- CurrentUserResponse.cs
|   |-- Workspaces/
|   |   |-- CreateWorkspaceRequest.cs
|   |   `-- InviteMemberRequest.cs
|   |-- Products/
|   |   |-- CreateProductRequest.cs
|   |   `-- UpdateProductRequest.cs
|   |-- Parts/
|   |   |-- CreatePartRequest.cs
|   |   `-- SetCurrentVersionRequest.cs
|   |-- Orders/
|   |   `-- UpdateOrderItemMappingRequest.cs
|   `-- Bundles/
|       |-- CreateManualBundleRequest.cs
|       `-- UpdateBundleStatusRequest.cs
|-- Authorization/
|   |-- WorkspaceRequirement.cs
|   `-- WorkspaceAuthorizationHandler.cs
|-- Program.cs
`-- appsettings.json
```

Do not add `RegisterRequest`, `LoginRequest`, password reset models, or password auth controllers.

## PrintHub.Worker

Background jobs.

```text
PrintHub.Worker/
|-- Functions/
|   |-- EtsyListingSyncFunction.cs
|   |-- EtsyOrderSyncFunction.cs
|   |-- EtsyWebhookFunction.cs
|   |-- FileMetadataFunction.cs
|   `-- BundleArchiveFunction.cs
|-- Services/
|   `-- WorkerWorkspaceContext.cs
`-- Program.cs
```

## Tests

```text
tests/
|-- PrintHub.Core.Tests/
|   |-- WorkspaceAuthorizationServiceTests.cs
|   |-- PreparationBundleServiceTests.cs
|   `-- FileVersioningTests.cs
|-- PrintHub.Infrastructure.Tests/
|   |-- EtsySyncServiceTests.cs
|   |-- AzureBlobFileStorageServiceTests.cs
|   `-- RepositoryTests.cs
`-- PrintHub.Tests/
    |-- AuthControllerTests.cs
    |-- WorkspacesControllerTests.cs
    |-- ProductsControllerTests.cs
    |-- OrdersControllerTests.cs
    |-- PreparationBundlesControllerTests.cs
    `-- AuthorizationTests.cs
```

## Configuration

```json
{
  "Authentication": {
    "Authority": "https://tenant.b2clogin.com/...",
    "Audience": "api://printhub"
  },
  "Cosmos": {
    "ConnectionString": "...",
    "DatabaseName": "PrintHub"
  },
  "Storage": {
    "ConnectionString": "...",
    "ContainerName": "print-files"
  },
  "Etsy": {
    "ClientId": "...",
    "ClientSecret": "...",
    "RedirectUri": "https://app.printhub.example.com/settings?etsy=callback"
  }
}
```

## Development Commands

```bash
dotnet restore PrintHub.sln
dotnet build PrintHub.sln
dotnet test PrintHub.sln
dotnet run --project src/PrintHub.Api
```

## Lock

- OAuth-only. No password auth code.
- Workspace authorization on every protected endpoint.
- Phase 1 API should not depend on printer adapters.
- Keep printer execution code out of the Phase 1 critical path.

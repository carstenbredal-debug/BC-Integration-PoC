# BC-Integration-PoC

Proof of concept for bidirectional integration with **Microsoft Dynamics 365 Business Central** via the standard [BC REST API v2.0](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/).

Built as an **Azure Functions** project (C# / .NET 8, isolated worker model).

## Features

- **OAuth2 authentication** — Client credentials flow via MSAL (`Microsoft.Identity.Client`)
- **Pull from BC** — Read companies, customers, and items
- **Push to BC** — Create or update customers and items (matched by entity number)
- **Bidirectional sync** — Orchestrated pull + push in a single call

## Project Structure

```
src/
├── Configuration/
│   └── BusinessCentralOptions.cs       # Typed settings
├── Models/
│   ├── BcCompany.cs                    # Company entity
│   ├── BcCustomer.cs                   # Customer entity
│   ├── BcItem.cs                       # Item entity
│   ├── ODataResponse.cs               # OData collection wrapper
│   └── SyncResult.cs                  # Sync operation result
├── Services/
│   ├── BusinessCentralAuthService.cs   # OAuth2 token acquisition
│   ├── BusinessCentralApiClient.cs     # Typed HTTP client for BC API v2.0
│   └── SyncService.cs                 # Sync orchestration
├── Functions/
│   ├── PullFromBcFunction.cs           # GET endpoints (companies, customers, items)
│   ├── PushToBcFunction.cs             # POST endpoints (push customers, items)
│   └── SyncFunction.cs                # Sync endpoints (pull, push, full sync)
├── Program.cs                          # Host + DI setup
├── host.json
└── BC-Integration-PoC.csproj
```

## Prerequisites

1. **.NET 8 SDK** — [Install](https://dot.net/download)
2. **Azure Functions Core Tools v4** — [Install](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
3. **Azure AD App Registration** with:
   - API Permission: `Dynamics 365 Business Central` → `API.ReadWrite.All` (Application)
   - Admin consent granted

## Configuration

Set the following environment variables (or add them to `local.settings.json`):

| Variable | Description |
|----------|-------------|
| `BC_TENANT_ID` | Azure AD tenant ID (GUID) |
| `BC_CLIENT_ID` | App registration client ID |
| `BC_CLIENT_SECRET` | App registration client secret |
| `BC_ENVIRONMENT` | BC environment name (e.g. `sandbox`, `production`) |
| `BC_COMPANY_ID` | *(Optional)* Target a specific company by ID |

### local.settings.json (template)

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "none",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "BC_TENANT_ID": "<your-tenant-id>",
    "BC_CLIENT_ID": "<your-client-id>",
    "BC_CLIENT_SECRET": "<your-client-secret>",
    "BC_ENVIRONMENT": "production",
    "BC_COMPANY_ID": ""
  }
}
```

## Build & Run

```bash
cd src
dotnet build
func start
```

## API Endpoints

### Pull (read from BC)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/GetCompanies` | List all BC companies |
| GET | `/api/GetCustomers` | List customers |
| GET | `/api/GetCustomer/{id}` | Get customer by ID |
| GET | `/api/GetItems` | List items |
| GET | `/api/GetItem/{id}` | Get item by ID |

### Push (write to BC)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/PushCustomers` | Create/update customers (JSON array body) |
| POST | `/api/PushItems` | Create/update items (JSON array body) |

### Sync

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/SyncAll` | Full bidirectional sync |
| POST | `/api/PullCustomers` | Pull customers with sync result |
| POST | `/api/PullItems` | Pull items with sync result |

## BC API Reference

Base URL: `https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/v2.0`

- [API Endpoints](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/endpoints-apis-for-dynamics)
- [Customer entity](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/api/dynamics_customer_get)
- [Item entity](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/api/dynamics_item_get)
- [OAuth authentication](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/webservices/authenticate-web-services-using-oauth)

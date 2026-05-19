# BC-Integration-PoC

Proof of concept for bidirectional integration with **Microsoft Dynamics 365 Business Central** via the standard [BC REST API v2.0](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/).

Built as an **Azure Functions** backend (C# / .NET 8, isolated worker model) with a **Blazor WebAssembly** frontend for creating customers.

## Features

- **OAuth2 authentication** — Client credentials flow via MSAL (`Microsoft.Identity.Client`)
- **Pull from BC** — Read companies, customers, and items
- **Push to BC** — Create or update customers and items (matched by entity number)
- **Bidirectional sync** — Orchestrated pull + push in a single call
- **Blazor frontend** — Web UI for creating customers directly in BC

## Project Structure

```
src/                                       # Azure Functions backend
├── Configuration/
│   └── BusinessCentralOptions.cs
├── Models/
├── Services/
├── Functions/
├── Program.cs
├── host.json
└── BC-Integration-PoC.csproj

web/                                       # Blazor WebAssembly frontend
├── Models/
│   ├── CreateCustomerRequest.cs           # Form model with validation
│   └── CustomerResult.cs                 # API response DTOs
├── Services/
│   ├── BcApiService.cs                    # API client for Azure Functions backend
│   └── FunctionKeyHandler.cs              # Auth handler for function keys
├── Pages/
│   └── Home.razor                         # Customer creation form
├── Layout/
├── Program.cs
├── wwwroot/appsettings.json               # API URL + function key config
└── web.csproj
```

## Prerequisites

1. **.NET 8 SDK** — [Install](https://dot.net/download)
2. **Azure Functions Core Tools v4** — [Install](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
3. **Azurite storage emulator** — required by the Azure Functions runtime
   - **Visual Studio 2022**: Included automatically. Go to **View → Other Windows → Azurite** and click **Start** before debugging.
   - **Standalone install**: `npm install -g azurite` then run `azurite --silent --location .azurite`
4. **Azure AD App Registration** with:
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
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
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

### Visual Studio
1. Start Azurite: **View → Other Windows → Azurite → Start Azurite**
2. Press **F5** to debug

### Command Line
```bash
# Terminal 1: Start Azurite
azurite --silent --location .azurite

# Terminal 2: Run the Functions backend
cd src
dotnet build
func start

# Terminal 3: Run the Blazor frontend
cd web
dotnet run
```

The frontend will be available at `http://localhost:5000` (or the port shown in the terminal).

### Frontend Configuration

Edit `web/wwwroot/appsettings.json` to set the API URL and function key:

```json
{
  "ApiBaseUrl": "https://<your-function-app>.azurewebsites.net/api/",
  "FunctionKey": "<your-function-key>"
}
```

For local development, point `ApiBaseUrl` to `http://localhost:7071/api/` (no function key needed locally).

### CORS

For Azure deployment, add the frontend URL to your Function App's CORS settings:
1. Azure Portal → Function App → **API → CORS**
2. Add the frontend URL (e.g. `https://your-site.azurestaticapps.net`)

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

## Deploy Blazor Frontend to Azure

The Blazor frontend is deployed automatically to **Azure Static Web Apps** via GitHub Actions on every push to `main` that changes the `web/` folder.

### Initial Setup

1. **Create an Azure Static Web App** in the [Azure Portal](https://portal.azure.com/#create/Microsoft.StaticApp):
   - Plan type: **Free**
   - Deployment source: **Other** (GitHub Actions workflow is already in the repo)
2. **Copy the deployment token**: Static Web App → Overview → **Manage deployment token**
3. **Add it as a GitHub secret**: Repo → Settings → Secrets and variables → Actions → **New repository secret**
   - Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
   - Value: paste the deployment token
4. **Add CORS** on your Function App: Azure Portal → Function App → API → CORS → add the Static Web App URL
5. **Trigger a deploy**: Push a change to `web/` or run the workflow manually from the Actions tab

### Function Key

To authenticate API calls from the deployed frontend, update `web/wwwroot/appsettings.json` with your function key before deploying:

```json
{
  "ApiBaseUrl": "https://<your-function-app>.azurewebsites.net/api/",
  "FunctionKey": "<your-function-key>"
}
```

## BC API Reference

Base URL: `https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/v2.0`

- [API Endpoints](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/endpoints-apis-for-dynamics)
- [Customer entity](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/api/dynamics_customer_get)
- [Item entity](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/api/dynamics_item_get)
- [OAuth authentication](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/webservices/authenticate-web-services-using-oauth)

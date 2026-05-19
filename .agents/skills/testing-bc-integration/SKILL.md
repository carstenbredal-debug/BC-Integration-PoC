---
name: testing-bc-integration
description: Test the BC Integration PoC Azure Functions end-to-end against a live Business Central environment. Use when verifying BC API connectivity, endpoint behavior, or sync operations.
---

# Testing BC Integration PoC

## Devin Secrets Needed

- `BC_TENANT_ID` — Azure AD tenant ID
- `BC_CLIENT_ID` — App registration client ID
- `BC_CLIENT_SECRET` — App registration client secret
- `BC_ENVIRONMENT` — BC environment name (e.g. `production`)

## Prerequisites

1. **.NET 8 SDK** — should be pre-installed in the environment
2. **Azure Functions Core Tools v4** — install if not present:
   ```bash
   npm install -g azure-functions-core-tools@4 --unsafe-perm true
   ```

## Setup

1. Create `src/local.settings.json` (gitignored — use `src/local.settings.json.template` as base):
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "none",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "BC_TENANT_ID": "<from secret>",
       "BC_CLIENT_ID": "<from secret>",
       "BC_CLIENT_SECRET": "<from secret>",
       "BC_ENVIRONMENT": "<from secret>",
       "BC_COMPANY_ID": "95c5fbb4-5d53-f111-a820-6045bd8c6cbb"
     }
   }
   ```
   Note: If BC_* env vars are already set in the environment, `func start` will use those and skip the local.settings.json values for those keys.

2. Start the Functions host:
   ```bash
   cd src && func start
   ```
   The host should list all 10 endpoints on `http://localhost:7071`.

## Known Issues

- **Storage health check warning**: Even with `AzureWebJobsStorage` set to `"none"`, Azure Functions Core Tools v4 may log an unhealthy storage warning. This is cosmetic — all HTTP-triggered functions work fine. Visual Studio might be stricter about this warning than the CLI.
- **Environment name**: The BC environment is `production`, not a sandbox. `Cronus Test 1` is a **company** within the production environment, not the environment name itself.

## Test Endpoints

### Read (safe to run anytime)
```bash
curl http://localhost:7071/api/GetCompanies
curl http://localhost:7071/api/GetCustomers
curl http://localhost:7071/api/GetItems
curl http://localhost:7071/api/GetCustomer/{customerId}
curl http://localhost:7071/api/GetItem/{itemId}
```

### Sync (read-only pull)
```bash
curl -X POST http://localhost:7071/api/SyncAll
curl -X POST http://localhost:7071/api/PullCustomers
curl -X POST http://localhost:7071/api/PullItems
```

### Write (modifies BC data — use with caution on production)
```bash
curl -X POST http://localhost:7071/api/PushCustomers -d '[{"number":"99999","displayName":"Test Customer"}]'
curl -X POST http://localhost:7071/api/PushItems -d '[{"number":"99999","displayName":"Test Item"}]'
```

## Expected Data (Cronus Test 1)

- **Companies**: 5 total, including Cronus Test 1 (id: `95c5fbb4-5d53-f111-a820-6045bd8c6cbb`)
- **Customers**: At least 5, first is `#10000 - Kontorcentralen A/S (Nyborg, DK)`
- **Items**: At least 51, first is `#1896-S - ATHEN Skrivebord (unitPrice: 5560)`

## Quick Smoke Test via curl (no func start needed)

To verify BC credentials work without starting the Functions host:
```bash
ACCESS_TOKEN=$(curl -s -X POST "https://login.microsoftonline.com/${BC_TENANT_ID}/oauth2/v2.0/token" \
  -d "client_id=${BC_CLIENT_ID}" \
  -d "client_secret=${BC_CLIENT_SECRET}" \
  -d "scope=https://api.businesscentral.dynamics.com/.default" \
  -d "grant_type=client_credentials" | python3 -c "import sys,json; print(json.load(sys.stdin)['access_token'])")

curl -s -H "Authorization: Bearer $ACCESS_TOKEN" \
  "https://api.businesscentral.dynamics.com/v2.0/${BC_TENANT_ID}/${BC_ENVIRONMENT}/api/v2.0/companies"
```

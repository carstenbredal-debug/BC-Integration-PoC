---
name: testing-bc-integration
description: Test the BC Integration PoC app end-to-end locally. Use when verifying customer creation, invoice creation, or any BC API integration changes.
---

# Testing BC Integration PoC

## Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4
- Python 3 (for SPA server)
- BC credentials configured in `src/local.settings.json`

## Devin Secrets Needed

- `BC_CLIENT_SECRET` — Business Central app client secret (used in `local.settings.json`)
- `BC_FUNCTION_KEY` — Azure Functions host key (for deployed environment testing)

## Starting the Backend

```bash
cd /home/ubuntu/BusinessCentralIntegration/src
func start
```

Backend runs on `http://localhost:7071/api/`.

Verify with: `curl http://localhost:7071/api/GetCompanies`

## Starting the Frontend

The frontend is a Blazor WebAssembly app. It must be published first, then served with a custom SPA server (Python's simple HTTP server doesn't support SPA routing — `/invoice` returns 404).

```bash
# Publish the WASM app
cd /home/ubuntu/BusinessCentralIntegration/web
dotnet publish -c Release -o /tmp/blazor-publish

# Serve with SPA-compatible server
cd /tmp/blazor-publish/wwwroot
python3 /tmp/spa_server.py 5300
```

Frontend runs on `http://localhost:5300/`.

### SPA Server Script

If `/tmp/spa_server.py` doesn't exist, create it:

```python
import http.server, sys, os

class SPAHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        path = self.translate_path(self.path)
        if not os.path.exists(path) or os.path.isdir(path):
            if not os.path.exists(os.path.join(path, 'index.html')):
                self.path = '/index.html'
        super().do_GET()

port = int(sys.argv[1]) if len(sys.argv) > 1 else 8000
http.server.HTTPServer(('', port), SPAHandler).serve_forever()
```

## Frontend Configuration

The frontend's API base URL is set in `web/wwwroot/appsettings.json`. For local testing, ensure it points to `http://localhost:7071/api/`. For deployed testing, it should point to the Azure Functions URL with the function key.

The function key injection happens at build time via the deploy workflow — check `appsettings.json` has `FunctionKey` set if testing deployed.

## Key Test Flows

### Customer Creation (`/` route)

1. Navigate to `http://localhost:5300/`
2. Fill in Display Name, Email, Phone
3. Optionally fill address fields (Street, Postal Code, City, Country/Region dropdown)
4. Optionally select Payment Terms, Gen. Bus. Posting Group, VAT Bus. Posting Group, Customer Posting Group
5. Click "Create Customer"
6. Verify green success alert with customer number

### Invoice Creation (`/invoice` route)

1. Navigate to `http://localhost:5300/invoice`
2. Select a customer from dropdown (6+ customers from BC)
3. Optionally enter External Document Number
4. Select item from dropdown — unit price auto-fills
5. Set quantity — line total updates automatically
6. Click "+ Add Line" for additional lines
7. Click "Create Invoice"
8. Verify green success alert with invoice number, "Draft" status, line count, totals excl/incl tax

### Validation Tests

- Submit invoice without customer → "Please select a customer." in red
- Add/remove lines: Remove button hidden when only 1 line exists

## Known Issues & Workarounds

- **SPA routing**: Python's `SimpleHTTPServer` returns 404 for `/invoice`. Must use custom SPAHandler that falls back to `index.html`.
- **JSON serialization**: BC API rejects requests with read-only fields (e.g., `id`, `customerNumber`). The fix uses `JsonIgnoreCondition.WhenWritingDefault` with nullable string properties. If you see 400 errors from BC, check serialization first.
- **Blazor WASM + DelegatingHandler**: `DelegatingHandler.InnerHandler` cannot be set in WASM (throws `PlatformNotSupportedException`). Use `HttpClient` directly with custom configuration instead of handler chains.
- **BC posting groups**: Gen. Bus. and VAT Bus. Posting Groups are not available via standard REST API v2.0. They are read/written via OData v4 (`workflowCustomers` entity).

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/GetCompanies` | GET | List BC companies |
| `/api/GetCustomers` | GET | List BC customers |
| `/api/GetItems` | GET | List BC items |
| `/api/GetCountryRegions` | GET | List country/region codes |
| `/api/GetPaymentTerms` | GET | List payment terms |
| `/api/GetPostingGroups` | GET | List posting group values |
| `/api/CreateCustomer` | POST | Create customer in BC |
| `/api/CreateInvoice` | POST | Create sales invoice with lines |

## Test Data

- **Test customer**: Kontorcentralen A/S (10000)
- **Test items**: AMSTERDAM Lampe (1928-S, unit price 305), PARIS Gæstestol (1900-S, unit price 1071)
- **Expected VAT**: 25% (Danish company)
- **Company ID**: `95c5fbb4-5d53-f111-a820-6045bd8c6cbb`

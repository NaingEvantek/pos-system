# Troubleshooting Guide

## Common Errors and Solutions

### Error: Circular Reference / Object Cycle Detected

**Error Message:**
```
System.Text.Json.JsonException: A possible object cycle was detected. 
This can either be due to a cycle or if the object depth is larger than 
the maximum allowed depth of 32.
```

**Cause:**
Entity Framework models have circular references (Sale → Items → Sale).

**Solution:**
This has been fixed in two ways:

1. **Program.cs** - Added JSON serialization options:
```csharp
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
})
```

2. **DTOs** - Created Data Transfer Objects to break circular references

The fix is already included in the code. Just make sure you:
```bash
cd POS.API
dotnet restore
dotnet build
dotnet run
```

---

### Error: System.Drawing.Printing namespace not found

**Error Message:**
```
error CS1069: The type name 'PrintPageEventArgs' could not be found in the namespace 'System.Drawing.Printing'
```

**Solution:**

The `System.Drawing.Common` package is already included in `POS.PrintService.csproj`. Just restore the packages:

```bash
cd POS.PrintService
dotnet restore
dotnet build
```

If the error persists, manually add the package:

```bash
dotnet add package System.Drawing.Common --version 8.0.0
```

---

### Error: Port already in use

**Error Message:**
```
Failed to bind to address http://localhost:5000: address already in use
```

**Solution:**

Change the port in `appsettings.json`:

For Main API (`POS.API/appsettings.json`):
```json
{
  "Urls": "http://localhost:5050"
}
```

For Print Service (`POS.PrintService/appsettings.json`):
```json
{
  "Urls": "http://localhost:5051"
}
```

Don't forget to update the URLs in `pos-frontend/src/services/api.js`:
```javascript
const API_BASE_URL = 'http://localhost:5050/api';
const PRINT_SERVICE_URL = 'http://localhost:5051/api';
```

---

### Error: CORS policy blocking requests

**Error Message:**
```
Access to XMLHttpRequest has been blocked by CORS policy
```

**Solution:**

1. Make sure all three services are running
2. Check that the React app is running on port 3000 (or update CORS settings)
3. Verify CORS configuration in `Program.cs` includes your React app's URL

---

### Error: Database creation failed

**Error Message:**
```
Unable to create database
```

**Solution:**

1. Delete the existing database file:
```bash
cd POS.API
rm pos.db
```

2. Restart the API - it will recreate the database automatically

---

### Error: npm install fails

**Solution:**

1. Delete node_modules and package-lock.json:
```bash
cd pos-frontend
rm -rf node_modules package-lock.json
```

2. Reinstall:
```bash
npm install
```

---

### Error: Print service returns 500 error

**Cause:**
The print service uses System.Drawing.Common which has platform-specific behavior.

**Solutions:**

**On Linux/Mac:**
The Windows-specific PrintDocument code won't work. The service will still generate HTML receipts that can be printed via browser.

**On Windows:**
Make sure System.Drawing.Common is installed:
```bash
cd POS.PrintService
dotnet add package System.Drawing.Common
dotnet restore
```

---

### Alternative: Browser-Only Printing

If you want to avoid System.Drawing.Common entirely, you can remove the Windows printing code:

1. Open `POS.PrintService/Controllers/PrintController.cs`

2. Remove or comment out these lines:
```csharp
// Remove these using statements
using System.Drawing;
using System.Drawing.Printing;

// Remove the entire PrintDocument_PrintPage method
```

The service will still work perfectly for browser-based printing!

---

### React App Shows "Failed to load products"

**Solutions:**

1. Check API is running:
   - Open http://localhost:5000/swagger
   - Should show Swagger documentation

2. Check API URL in code:
   - File: `pos-frontend/src/services/api.js`
   - Verify: `const API_BASE_URL = 'http://localhost:5000/api';`

3. Check browser console for CORS errors

---

### Receipt Window Blocked by Pop-up Blocker

**Solution:**

Allow pop-ups for localhost:3000 in your browser settings.

**Chrome:**
1. Click the pop-up blocked icon in address bar
2. Choose "Always allow pop-ups from http://localhost:3000"

**Firefox:**
1. Click Options in the notification bar
2. Choose "Allow pop-ups for localhost"

---

## Platform-Specific Notes

### Windows
- Full printing support including System.Drawing
- Can print directly to printers
- Thermal printer support available

### Linux/Mac
- Browser-based printing works perfectly
- System.Drawing.Common has limited functionality
- Use HTML receipt generation (already implemented)
- For production, consider Electron app or CUPS integration

---

## Quick Diagnostics

Run these commands to check your setup:

```bash
# Check .NET version (should be 8.x.x)
dotnet --version

# Check Node version (should be 18+)
node --version

# Check if ports are available
netstat -an | grep 5000
netstat -an | grep 5001
netstat -an | grep 3000

# Test API directly
curl http://localhost:5000/api/products

# Test Print Service
curl http://localhost:5001/api/print/printers
```

---

## Still Having Issues?

1. Check all three services are running in separate terminals
2. Review the console output for error messages
3. Check the browser console (F12) for JavaScript errors
4. Verify all dependencies are installed:
   ```bash
   # For .NET projects
   dotnet restore
   
   # For React
   npm install
   ```

5. Try the test print endpoint:
   ```bash
   curl -X POST http://localhost:5001/api/print/test
   ```

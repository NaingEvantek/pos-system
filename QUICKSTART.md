# Quick Start Guide

## Prerequisites Check

Before starting, verify you have:

1. **.NET 8 SDK installed**
   ```bash
   dotnet --version
   ```
   Should show version 8.x.x

2. **Node.js installed**
   ```bash
   node --version
   ```
   Should show version 18.x.x or higher

## Installation

### First Time Setup

1. **Install Frontend Dependencies**
   ```bash
   cd pos-frontend
   npm install
   cd ..
   ```

2. **Restore .NET Dependencies**
   ```bash
   cd POS.API
   dotnet restore
   cd ..
   
   cd POS.PrintService
   dotnet restore
   cd ..
   ```

## Running the Application

### Option 1: Automated Start (Recommended)

**Windows:**
```bash
start-all.bat
```

**Linux/Mac:**
```bash
./start-all.sh
```

This will start all three services automatically in separate terminal windows.

### Option 2: Manual Start

Open **3 separate terminal windows** and run:

**Terminal 1 - Main API:**
```bash
cd POS.API
dotnet run
```

**Terminal 2 - Print Service:**
```bash
cd POS.PrintService
dotnet run
```

**Terminal 3 - React Frontend:**
```bash
cd pos-frontend
npm run dev
```

## Access the Application

Once all services are running:

- **POS Application**: http://localhost:3000
- **API Documentation**: http://localhost:5000/swagger
- **Print Service**: http://localhost:5001

## Testing the System

1. Open http://localhost:3000 in your browser
2. Click on any product to add it to cart
3. Adjust quantities using +/- buttons
4. Enter customer name (optional)
5. Select payment method
6. Click "Checkout"
7. Receipt will open in new window and print dialog will appear

## Common Issues

### Port Already in Use

If you get "port already in use" errors:

**Change API port:**
Edit `POS.API/appsettings.json`:
```json
"Urls": "http://localhost:5050"
```

**Change Print Service port:**
Edit `POS.PrintService/appsettings.json`:
```json
"Urls": "http://localhost:5051"
```

**Change React port:**
Edit `pos-frontend/vite.config.js`:
```javascript
server: {
  port: 3001
}
```

Don't forget to update the API URLs in `pos-frontend/src/services/api.js`!

### Database Issues

If you encounter database errors:

1. Stop the API
2. Delete `POS.API/pos.db`
3. Restart the API - database will be recreated

### Build Errors

If you get build errors:

**.NET Projects:**
```bash
dotnet clean
dotnet restore
dotnet build
```

**React Project:**
```bash
rm -rf node_modules package-lock.json
npm install
```

## Stopping the Application

### Windows (Automated Start)
- Close all terminal windows that were opened

### Linux/Mac (Automated Start)
- Press Ctrl+C in the terminal

### Manual Start
- Press Ctrl+C in each of the three terminal windows

## Next Steps

- Customize products in `POS.API/Data/POSDbContext.cs`
- Modify receipt layout in `POS.PrintService/Controllers/PrintController.cs`
- Adjust tax rate in `POS.API/Controllers/SalesController.cs`
- Customize UI colors in `pos-frontend/src/index.css`

## Need Help?

Check the main README.md for detailed documentation and troubleshooting.

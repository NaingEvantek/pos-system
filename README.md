# POS System - React + .NET Core 8

A complete Point of Sale (POS) application with sales management and receipt printing functionality.

## Project Structure

```
pos-system/
├── POS.API/                    # Main .NET Core 8 Web API
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   └── SalesController.cs
│   ├── Data/
│   │   └── POSDbContext.cs
│   ├── Models/
│   │   ├── Product.cs
│   │   └── Sale.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── POS.API.csproj
│
├── POS.PrintService/           # Local Print Service
│   ├── Controllers/
│   │   └── PrintController.cs
│   ├── Models/
│   │   └── ReceiptModel.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── POS.PrintService.csproj
│
└── pos-frontend/               # React 18 Frontend
    ├── src/
    │   ├── components/
    │   │   ├── Cart.jsx
    │   │   └── ProductList.jsx
    │   ├── services/
    │   │   └── api.js
    │   ├── App.jsx
    │   ├── main.jsx
    │   └── index.css
    ├── index.html
    ├── package.json
    └── vite.config.js
```

## Prerequisites

- .NET 8 SDK - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- Node.js 18+ - [Download](https://nodejs.org/)
- npm or yarn

## Step-by-Step Setup

### Step 1: Setup Main API (.NET Core 8)

1. Open terminal/command prompt and navigate to the POS.API directory:

```bash
cd pos-system/POS.API
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Run the API:

```bash
dotnet run
```

The API will start on `http://localhost:5000`

You can verify it's running by opening `http://localhost:5000/swagger` in your browser.

### Step 2: Setup Print Service

1. Open a NEW terminal window and navigate to the POS.PrintService directory:

```bash
cd pos-system/POS.PrintService
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Run the print service:

```bash
dotnet run
```

The print service will start on `http://localhost:5001`

### Step 3: Setup React Frontend

1. Open a NEW terminal window and navigate to the pos-frontend directory:

```bash
cd pos-system/pos-frontend
```

2. Install dependencies:

```bash
npm install
```

3. Start the development server:

```bash
npm run dev
```

The React app will start on `http://localhost:3000`

4. Open your browser and go to `http://localhost:3000`

## Usage

### Making a Sale

1. **Browse Products**: The left side shows available products with their prices and stock levels.

2. **Add to Cart**: Click on any product to add it to the shopping cart on the right.

3. **Manage Cart**:
   - Use + and - buttons to adjust quantities
   - Click "Remove" to delete items from cart
   - View subtotal, tax (10%), and total in real-time

4. **Checkout**:
   - (Optional) Enter customer name
   - Select payment method (Cash, Credit Card, Debit Card, Mobile Payment)
   - Click "Checkout" button

5. **Receipt Printing**:
   - After checkout, a receipt will automatically open in a new browser window
   - The receipt shows all sale details
   - The browser's print dialog will appear automatically
   - You can print to any printer connected to your computer

### API Endpoints

#### Main API (Port 5000)

**Products:**
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

**Sales:**
- `GET /api/sales` - Get all sales
- `GET /api/sales/{id}` - Get sale by ID
- `POST /api/sales` - Create new sale
- `GET /api/sales/today` - Get today's sales summary

#### Print Service (Port 5001)

- `POST /api/print/receipt` - Print receipt
- `GET /api/print/printers` - Get available printers
- `POST /api/print/test` - Test print

## Features

### Main API
✅ Product management (CRUD operations)
✅ Sales tracking and history
✅ Automatic tax calculation (10%)
✅ Stock management (auto-decrements on sale)
✅ SQLite database with Entity Framework Core
✅ Swagger API documentation
✅ CORS enabled for React app

### Print Service
✅ Receipt generation in HTML format
✅ Browser-based printing
✅ Receipt preview in new window
✅ Support for multiple printers
✅ Test print functionality
✅ Formatted receipt with sale details

### Frontend
✅ Modern React 18 with Vite
✅ Responsive design
✅ Real-time cart updates
✅ Product grid display
✅ Multiple payment methods
✅ Customer name tracking
✅ Automatic tax calculation
✅ One-click checkout and print
✅ Error handling and loading states
✅ Success notifications

## Database

The application uses SQLite with the following seeded products:

1. Laptop - $1,200.00 (10 in stock)
2. Mouse - $25.00 (50 in stock)
3. Keyboard - $80.00 (30 in stock)
4. Monitor - $400.00 (15 in stock)
5. Headphones - $150.00 (25 in stock)

The database file (`pos.db`) is created automatically when you first run the API.

## Printing Options

### Current Implementation (Browser-based)
The current implementation uses browser printing, which:
- Opens receipts in a new window
- Uses the browser's print dialog
- Works on any OS (Windows, Mac, Linux)
- Requires user interaction

### For Production (Direct Printing)

For actual POS deployment with silent/direct printing, you have these options:

**Option 1: Electron Desktop App**
- Wrap the React app in Electron
- Enable silent printing without dialogs
- Direct printer selection
- Best for dedicated POS terminals

**Option 2: Windows Service**
- Uncomment the `PrintDocument` code in `PrintController.cs`
- Requires Windows OS
- Can print directly to thermal/receipt printers
- Silent printing without user interaction

**Option 3: Thermal Printer Integration**
- Use ESC/POS commands for receipt printers
- Install printer-specific drivers
- Modify `PrintController.cs` to send ESC/POS commands
- Ideal for retail/restaurant environments

## Troubleshooting

### API won't start
- Make sure .NET 8 SDK is installed: `dotnet --version`
- Check if port 5000 is available
- Delete `pos.db` and restart to recreate database

### Print service won't start
- Check if port 5001 is available
- Verify .NET 8 SDK installation

### React app won't start
- Make sure Node.js is installed: `node --version`
- Delete `node_modules` and run `npm install` again
- Check if port 3000 is available

### Receipt won't print
- Make sure print service is running on port 5001
- Check browser's pop-up blocker settings
- Verify that your printer is connected and set as default

### CORS errors
- Verify all three services are running
- Check API URLs in `src/services/api.js`
- Clear browser cache

## Future Enhancements

- [ ] User authentication and authorization
- [ ] Multi-user support with roles
- [ ] Barcode scanner integration
- [ ] Inventory management
- [ ] Sales reports and analytics
- [ ] Customer loyalty program
- [ ] Return/refund processing
- [ ] Email receipt option
- [ ] Multiple currency support
- [ ] Discount and promotion system

## Technologies Used

**Backend:**
- .NET Core 8
- Entity Framework Core 8
- SQLite
- Swagger/OpenAPI

**Frontend:**
- React 18
- Vite
- Axios
- Modern CSS

**Printing:**
- System.Drawing (for Windows printing)
- HTML-based receipt generation
- Browser print API

## License

This is a demo project for educational purposes.

## Support

For issues or questions, please check:
1. All three services are running
2. Ports 3000, 5000, and 5001 are available
3. .NET 8 SDK and Node.js are installed correctly
4. Database file has proper permissions

---

**Happy Selling! 🛒💰**

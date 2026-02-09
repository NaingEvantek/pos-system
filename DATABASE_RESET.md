# Database Reset and Setup Guide

## Quick Fix for Foreign Key Error

The error occurs because the database schema is outdated. Follow these steps:

### Step 1: Delete Old Database
```bash
cd POS.API
rm pos.db
rm pos.db-shm
rm pos.db-wal
```

### Step 2: Start API (Creates new database)
```bash
dotnet run
```

Wait for: "Now listening on: http://localhost:5000"

### Step 3: Initialize Default Users
```bash
# In a new terminal or browser
curl -X POST http://localhost:5000/api/auth/init
```

Or visit: http://localhost:5000/api/auth/init

### Step 4: Test the API
```bash
# Test if database is working
curl http://localhost:5000/api/products
```

You should see the 5 seeded products.

### Step 5: Start Frontend
```bash
cd ../pos-frontend
npm install  # if first time
npm run dev
```

### Step 6: Login
- URL: http://localhost:3000
- Username: admin
- Password: admin123

---

## If Error Persists

### Option A: Manual Database Cleanup
```bash
# Stop the API (Ctrl+C)
cd POS.API

# Remove all database files
rm -f pos.db*

# Clear any EF Core migrations
rm -rf Migrations/

# Restart API
dotnet run
```

### Option B: Check for Running Processes
```bash
# On Windows
taskkill /F /IM dotnet.exe

# On Linux/Mac
pkill dotnet

# Then restart
dotnet run
```

### Option C: Use SQLite Browser
1. Download: https://sqlitebrowser.org/
2. Open `pos.db`
3. Check if tables exist:
   - Products ✓
   - Sales ✓
   - SaleItems ✓
   - Users ✓
   - Customers ✓
   - DebitTransactions ✓

If any are missing, delete the database and recreate.

---

## Common Issues

### Issue 1: "Customer with ID X not found"
**Solution:** Make sure you're selecting "Walk-In" or "Online" type for new customers. Royal customers must be created first in Customer Management page.

### Issue 2: "FOREIGN KEY constraint failed"
**Cause:** Old database schema without Customers table
**Solution:** Delete `pos.db` and restart API

### Issue 3: "Table 'Customers' not found"
**Cause:** Database not created with new schema
**Solution:** 
```bash
rm pos.db
dotnet run
```

---

## Verification Checklist

After setup, verify:

- [ ] API running on http://localhost:5000
- [ ] Swagger available at http://localhost:5000/swagger
- [ ] Products endpoint returns 5 items
- [ ] Auth init returns success
- [ ] Frontend running on http://localhost:3000
- [ ] Can login with admin/admin123
- [ ] Can see products in POS page

---

## Testing the New Features

### Test Walk-In Customer
1. Select "Walk-In" type
2. Enter name: "John Doe"
3. Add products
4. Set discount: 10%
5. Payment will auto-fill
6. Complete sale ✓

### Test Online Customer  
1. Select "Online" type
2. Enter name: "Jane Smith"
3. Enter phone: "555-1234"
4. Add products
5. Complete sale ✓

### Test Royal Customer
1. First create Royal customer in Customer Management:
   - Name: "ABC Company"
   - Phone: "555-5678"
   - Type: Royal
   - Credit Limit: $10,000
2. Go to POS
3. Select "Royal" type
4. Choose "ABC Company"
5. Add products (wholesale pricing applied)
6. Enter payment less than total
7. Balance goes to credit ✓

---

## Database Schema

If you want to verify the schema manually:

```sql
-- Check tables exist
SELECT name FROM sqlite_master WHERE type='table';

-- Should show:
-- Products
-- Sales
-- SaleItems  
-- Users
-- Customers
-- DebitTransactions

-- Check Sales table columns
PRAGMA table_info(Sales);

-- Should include:
-- Discount, PaymentAmount, Balance, CustomerId, PriceType, IsPaid
```

---

## Need More Help?

If the issue persists:

1. Check API console for detailed error messages
2. Verify database file permissions
3. Make sure SQLite is installed
4. Try running as administrator/sudo
5. Check if port 5000 is available

**Still having issues?** 
Share the complete error message from the API console.

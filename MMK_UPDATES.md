# MMK Currency & System Updates Guide

## Changes Made

### 1. Currency Changed to MMK (Myanmar Kyat)
- ✅ All prices now in MMK (no decimals)
- ✅ Removed $ symbol, replaced with MMK
- ✅ All amounts formatted with commas (e.g., 1,200,000 MMK)
- ✅ No decimal points anywhere in the system

### 2. Credit Limit Removed
- ✅ Royal customers now have **UNLIMITED CREDIT**
- ✅ No credit limit checks
- ✅ Removed CreditLimit field from Customer model
- ✅ Only tracks current debit balance

### 3. Stock Management Changed
- ✅ Products created with **ZERO stock** initially
- ✅ New **Inventory Entry** page to add stock
- ✅ Similar to purchase entry but for incoming stock
- ✅ Automatically updates product stock

### 4. Customer Registration During Sales
- ✅ Walk-In and Online customers registered on POS page
- ✅ **Phone number is unique identifier**
- ✅ If phone exists, use existing customer and update info
- ✅ If new phone, create new customer
- ✅ Added **Address** field for all customer types

### 5. New Monthly Sales Chart Report
- ✅ Shows last 12 months including current month
- ✅ Stacked bar chart: Retail vs Wholesale
- ✅ Monthly revenue breakdown
- ✅ Visual analytics

## Database Changes

### Products Table
```sql
RetailPrice: int (was decimal)
WholesalePrice: int (was decimal)
Price: int (was decimal)
Stock: int (starts at 0, not seeded)
```

### Customers Table
```sql
CurrentDebit: int (was decimal)
-- REMOVED: CreditLimit
```

### Sales Table
```sql
TotalAmount: int (was decimal)
Subtotal: int (was decimal)
Discount: int (was decimal)
PaymentAmount: int (was decimal)
Balance: int (was decimal)
```

### New: InventoryEntries Table
```sql
Id: int
EntryDate: datetime
ReferenceNumber: string
Supplier: string
TotalAmount: int
Notes: string
```

### New: InventoryEntryItems Table
```sql
Id: int
InventoryEntryId: int
ProductId: int
ProductName: string
Quantity: int
UnitCost: int
Total: int
```

## API Endpoints Added

### Inventory Management
```
GET    /api/inventory              - List all inventory entries
GET    /api/inventory/{id}         - Get specific entry
POST   /api/inventory              - Create new entry (adds stock)
DELETE /api/inventory/{id}         - Delete entry (removes stock)
```

### Monthly Sales Chart
```
GET /api/reports/monthly-sales-chart
```

Returns:
```json
{
  "period": "Last 12 Months",
  "startDate": "2025-02-01",
  "endDate": "2026-02-06",
  "monthlySales": [
    {
      "month": "Feb 2025",
      "year": 2025,
      "monthNumber": 2,
      "totalRevenue": 5000000,
      "totalSales": 45,
      "retailRevenue": 3000000,
      "wholesaleRevenue": 2000000,
      "retailCount": 30,
      "wholesaleCount": 15
    },
    // ... 11 more months
  ],
  "totalRevenue": 60000000,
  "totalSales": 540
}
```

## Frontend Utility - Currency Formatting

New file: `/pos-frontend/src/utils/currency.js`

```javascript
// Format: 1200000 → "1,200,000"
formatMMK(amount)

// Parse: "1,200,000" → 1200000
parseMMK(value)

// Format with currency: 1200000 → "1,200,000 MMK"
formatCurrency(amount)
```

### Usage Example:
```javascript
import { formatMMK } from '../utils/currency';

// Display price
<span>{formatMMK(product.retailPrice)} MMK</span>

// Input handling
<input 
  value={price}
  onChange={(e) => {
    const value = e.target.value.replace(/,/g, '');
    if (/^\d*$/.test(value)) {
      setPrice(value);
    }
  }}
/>
```

## Key Behavior Changes

### 1. POS Page - Customer Entry
**Walk-In:**
- Name required
- Phone optional
- Address optional
- If phone provided and exists → updates existing customer
- If phone provided and new → creates new customer

**Online:**
- Name required
- Phone required
- Address optional
- If phone exists → updates existing customer
- If phone new → creates new customer

**Royal:**
- Select from dropdown (must pre-exist)
- Shows current debit
- "Unlimited Credit" label displayed

### 2. Product Creation
- Stock field **removed** from product creation form
- All new products start with Stock = 0
- Use **Inventory Entry** page to add stock

### 3. Inventory Entry Page
Similar to purchase order:
1. Enter Reference Number (PO/Invoice#)
2. Enter Supplier name
3. Add products:
   - Select product
   - Enter quantity
   - Enter unit cost (in MMK)
4. Save → Stock automatically added to products

### 4. Credit Limits
**Before:**
- Royal customers had credit limits
- System checked limit before sale
- Showed "Available Credit"

**After:**
- Royal customers have UNLIMITED credit
- No limit checks
- Shows only "Current Debit"
- Label: "✓ Unlimited Credit"

## Testing Checklist

### Database Reset Required
```bash
cd POS.API
rm pos.db
dotnet run
curl -X POST http://localhost:5000/api/auth/init
```

### Test Scenarios

**1. Product & Inventory:**
- [ ] Create product (stock should be 0)
- [ ] Add inventory entry
- [ ] Verify stock increased
- [ ] Check all prices show as MMK with commas

**2. Walk-In Customer:**
- [ ] Enter name only → sale works
- [ ] Enter name + phone → customer created
- [ ] Same phone again → updates existing customer
- [ ] All amounts show in MMK format

**3. Online Customer:**
- [ ] Requires name + phone
- [ ] Address field works
- [ ] Same phone → reuses customer
- [ ] Must pay in full

**4. Royal Customer:**
- [ ] Select from dropdown
- [ ] Shows current debit in MMK
- [ ] No credit limit shown
- [ ] Can buy on credit (unlimited)
- [ ] Debit balance updates

**5. Monthly Chart:**
- [ ] Shows 12 months
- [ ] Displays in MMK
- [ ] Stacked bars (Retail/Wholesale)
- [ ] Current month included

## UI Changes Summary

### Currency Display
- Old: `$1,200.00`
- New: `1,200,000 MMK`

### Input Fields
- Old: Step 0.01, decimal allowed
- New: Integer only, no decimals, comma-formatted display

### Customer Info
- Old: Credit Limit field with availability %
- New: Current Debit only, "Unlimited Credit" badge

### Product Form
- Old: Stock field with initial value
- New: No stock field, shows "Stock: 0" after creation

### New Pages
1. **Inventory Entry** - Stock management
2. **Monthly Sales Chart** - Analytics dashboard

## Migration Path

If you have existing data:

1. **Backup current database**
   ```bash
   cp pos.db pos.db.backup
   ```

2. **Convert prices** (multiply by 1000 for MMK approximation)
   ```sql
   UPDATE Products SET 
     RetailPrice = RetailPrice * 1000,
     WholesalePrice = WholesalePrice * 1000,
     Price = Price * 1000;
   ```

3. **Remove credit limits**
   ```sql
   -- Credit limits now ignored, unlimited for Royal
   ```

4. **Reset stock to 0** (use Inventory Entry to add back)
   ```bash
   UPDATE Products SET Stock = 0;
   ```

Or just delete database and start fresh (recommended).

## Production Deployment

1. Update environment variables
2. Change default passwords
3. Set production MMK exchange rate if needed
4. Configure email for customer notifications
5. Backup database daily
6. Monitor debit balances for Royal customers

## Support

For issues:
1. Check browser console for errors
2. Check API logs
3. Verify database schema matches new structure
4. Ensure all files updated
5. Clear browser cache

---

**Ready to use with MMK currency, unlimited credit, inventory management, and monthly analytics!**

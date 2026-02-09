# New Features Guide - Advanced POS System

## 🎉 New Features Added

### 1. Product Search & Filter in POS
- ✅ Real-time search by product name or description
- ✅ Filter by category
- ✅ Instant results as you type

### 2. Customer Management
- ✅ Three customer types:
  - **Walk-In**: No registration, one-time customers
  - **Online**: Registered customers
  - **Royal**: VIP customers with credit facility
- ✅ Customer CRUD operations (Create, Read, Update, Delete)
- ✅ Credit limit management for Royal customers
- ✅ Debit tracking and balance monitoring

### 3. Retail & Wholesale Pricing
- ✅ Dual pricing system for each product
- ✅ Retail price for walk-in/online customers
- ✅ Wholesale price for bulk buyers
- ✅ Price type selection during checkout

### 4. Debit Control System
- ✅ Credit limit enforcement for Royal customers
- ✅ Real-time debit balance tracking
- ✅ Transaction history (Debit, Credit, Adjustment)
- ✅ Credit utilization monitoring
- ✅ Outstanding balance reports

### 5. Comprehensive Reports
- ✅ **Sales Summary**: Total revenue, average sale, payment methods, top products
- ✅ **Inventory Report**: Stock levels, low stock alerts, out of stock items
- ✅ **Customer Debits**: Outstanding balances, credit utilization
- ✅ **Daily Trend**: 7-day sales breakdown
- ✅ **Price Type Analysis**: Retail vs Wholesale performance
- ✅ **Customer Sales**: Top customers, purchase patterns

## 📋 Database Schema Updates

### New Tables

**Customers Table:**
```sql
- Id: int (PK)
- Name: string
- Phone: string
- Email: string
- Address: string
- Type: enum (WalkIn=0, Online=1, Royal=2)
- CreditLimit: decimal
- CurrentDebit: decimal
- IsActive: boolean
- CreatedAt: datetime
- LastPurchase: datetime
```

**DebitTransactions Table:**
```sql
- Id: int (PK)
- CustomerId: int (FK)
- SaleId: int (FK, nullable)
- Amount: decimal
- Type: enum (Debit=0, Credit=1, Adjustment=2)
- Notes: string
- TransactionDate: datetime
```

### Updated Tables

**Products Table:**
```sql
+ RetailPrice: decimal
+ WholesalePrice: decimal
(Price field kept for backward compatibility)
```

**Sales Table:**
```sql
+ CustomerId: int (FK, nullable)
+ PriceType: enum (Retail=0, Wholesale=1)
+ IsPaid: boolean
```

## 🚀 How to Use New Features

### Using Search in POS

1. Go to Point of Sale page
2. Use the search box to find products instantly
3. Or use the category dropdown to filter
4. Click products to add to cart

### Managing Customers

1. Click "👥 Customers" in sidebar
2. Click "+ Add Customer" button
3. Fill in details:
   - Name * (required)
   - Phone * (required)
   - Email (optional)
   - Address (optional)
   - Customer Type (Walk-In/Online/Royal)
   - Credit Limit (for Royal customers only)
4. Click "Create Customer"

### Using Credit System for Royal Customers

**Setting Up:**
1. Create customer with Type = "Royal"
2. Set credit limit (e.g., $5,000)
3. Customer can now purchase on credit

**Making Credit Sale:**
1. In POS, select Royal customer
2. Complete sale
3. Choose "Credit" payment method
4. System checks credit limit automatically
5. Debit is added to customer's balance

**Recording Payments:**
1. Go to Customers page
2. Find customer with outstanding balance
3. Click "Record Payment" (API endpoint available)
4. System reduces debit balance

### Viewing Reports

1. Click "📈 Reports" in sidebar
2. Select report type:
   - Sales Summary
   - Inventory
   - Customer Debits
   - Daily Trend
3. Adjust date range if needed
4. View detailed analytics

## 📊 API Endpoints

### Customers
```
GET    /api/customers                    - List all customers
GET    /api/customers/{id}               - Get customer details
POST   /api/customers                    - Create customer
PUT    /api/customers/{id}               - Update customer
DELETE /api/customers/{id}               - Delete customer
GET    /api/customers/{id}/debit-balance - Get debit balance
GET    /api/customers/{id}/transactions  - Get transaction history
POST   /api/customers/debit-transaction  - Record debit transaction
GET    /api/customers/royal              - List royal customers only
```

### Reports
```
GET /api/reports/sales-summary?startDate=...&endDate=...
GET /api/reports/inventory
GET /api/reports/customer-debits
GET /api/reports/daily-sales?days=7
GET /api/reports/customer-sales?startDate=...&endDate=...
GET /api/reports/price-type-analysis?startDate=...&endDate=...
```

## 💡 Usage Examples

### Example 1: Royal Customer Purchase

```javascript
// Create Royal customer
POST /api/customers
{
  "name": "ABC Company",
  "phone": "555-1234",
  "email": "abc@company.com",
  "type": 2,  // Royal
  "creditLimit": 10000
}

// Make sale on credit
POST /api/sales
{
  "customerId": 1,
  "priceType": 1,  // Wholesale
  "paymentMethod": "Credit",
  "isPaid": false,
  "items": [...]
}

// Record payment
POST /api/customers/debit-transaction
{
  "customerId": 1,
  "amount": 500,
  "type": 1,  // Credit (payment)
  "notes": "Payment received"
}
```

### Example 2: Retail vs Wholesale Sale

```javascript
// Retail sale
{
  "priceType": 0,  // Retail price
  "items": [{
    "productName": "Laptop",
    "quantity": 1,
    "unitPrice": 1200.00  // Retail price
  }]
}

// Wholesale sale
{
  "priceType": 1,  // Wholesale price
  "items": [{
    "productName": "Laptop",
    "quantity": 10,
    "unitPrice": 1000.00  // Wholesale price
  }]
}
```

## 🔐 Security Considerations

**Credit Limits:**
- Enforced at API level
- Cannot exceed limit
- Real-time validation
- Admin override available

**Data Protection:**
- Customer data encrypted in transit
- PII handled securely
- Audit trails maintained

## 📈 Reporting Features

### Sales Summary Report
- Total revenue by period
- Average sale value
- Payment method breakdown
- Top selling products
- Date range filtering

### Inventory Report
- Total inventory value
- Low stock alerts (<10 items)
- Out of stock items
- Category-wise breakdown

### Customer Debits Report
- Total outstanding debits
- Credit utilization %
- Risk analysis
- Payment history

### Daily Trend Report
- Last 7 days breakdown
- Daily revenue chart
- Sales count per day
- Tax collected

## 🎯 Business Benefits

**Customer Management:**
- Track loyal customers
- Reward VIP customers
- Better relationship management

**Credit System:**
- Increase sales to wholesale buyers
- Maintain cash flow control
- Risk management
- Automated tracking

**Pricing Strategy:**
- Maximize profit margins
- Competitive wholesale pricing
- Flexible pricing options

**Reports & Analytics:**
- Data-driven decisions
- Identify trends
- Optimize inventory
- Track performance

## 🔄 Migration from Previous Version

**If updating from older version:**

1. **Backup database:**
   ```bash
   cp POS.API/pos.db POS.API/pos.db.backup
   ```

2. **Delete old database:**
   ```bash
   rm POS.API/pos.db
   ```

3. **Run API to create new schema:**
   ```bash
   cd POS.API
   dotnet run
   ```

4. **Initialize users:**
   ```bash
   curl -X POST http://localhost:5000/api/auth/init
   ```

5. **Import old data if needed**

## 📝 Best Practices

**Customer Management:**
- Always verify customer type before setting credit limits
- Review debit balances regularly
- Set appropriate credit limits based on business relationship
- Keep customer information up to date

**Pricing:**
- Set wholesale prices 10-20% lower than retail
- Review pricing strategy monthly
- Consider volume discounts
- Monitor competitor pricing

**Debit Control:**
- Set conservative credit limits initially
- Review and adjust based on payment history
- Send reminders for outstanding balances
- Implement payment terms (Net 30, etc.)

**Reporting:**
- Review reports weekly
- Track key metrics
- Identify slow-moving inventory
- Monitor cash flow

## 🚨 Important Notes

1. **Credit Limits**: Once set, they can be increased but decreasing requires admin approval
2. **Debit Transactions**: Cannot be deleted, only adjusted
3. **Customer Type**: Changing from Royal to other types will lock their credit facility
4. **Reports**: Date ranges are inclusive
5. **Search**: Case-insensitive, searches in name and description

## 🔧 Configuration

**Credit System Settings** (in future updates):
- Default credit limit for new Royal customers
- Maximum credit period
- Interest on overdue amounts
- Automated reminder schedules

**Pricing Settings**:
- Default margin between retail/wholesale
- Bulk pricing tiers
- Seasonal pricing rules

## 📚 Related Documentation

- `AUTHENTICATION.md` - Authentication setup
- `README.md` - General setup
- `TROUBLESHOOTING.md` - Common issues

---

**Questions or Issues?**
Check the API documentation at http://localhost:5000/swagger when the API is running.

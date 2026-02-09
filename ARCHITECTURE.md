# System Architecture

## Overview

The POS system consists of three main components that work together to provide a complete point-of-sale solution with receipt printing capabilities.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Browser                             │
│                                                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              React Frontend (Port 3000)                    │  │
│  │  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐     │  │
│  │  │ ProductList │  │     Cart     │  │  API Service │     │  │
│  │  │  Component  │  │  Component   │  │              │     │  │
│  │  └─────────────┘  └──────────────┘  └──────────────┘     │  │
│  └───────────────────────────────────────────────────────────┘  │
│                           │         │                            │
└───────────────────────────┼─────────┼────────────────────────────┘
                            │         │
                            ▼         ▼
              ┌─────────────────────────────────────┐
              │  HTTP Requests (Axios)              │
              │  - Products API                     │
              │  - Sales API                        │
              │  - Print API                        │
              └─────────────────────────────────────┘
                            │         │
                ┌───────────┘         └──────────────┐
                │                                     │
                ▼                                     ▼
┌───────────────────────────────┐     ┌──────────────────────────────┐
│   Main API (Port 5000)        │     │  Print Service (Port 5001)   │
│                               │     │                              │
│  ┌─────────────────────────┐ │     │  ┌────────────────────────┐ │
│  │ Products Controller     │ │     │  │  Print Controller      │ │
│  ├─────────────────────────┤ │     │  ├────────────────────────┤ │
│  │ Sales Controller        │ │     │  │  Receipt Generation    │ │
│  └─────────────────────────┘ │     │  │  HTML/Browser Printing │ │
│                               │     │  │  (Windows: PrintDoc)   │ │
│  ┌─────────────────────────┐ │     │  └────────────────────────┘ │
│  │   EF Core DbContext     │ │     └──────────────────────────────┘
│  └─────────────────────────┘ │                   │
│              │                │                   │
│              ▼                │                   ▼
│  ┌─────────────────────────┐ │     ┌──────────────────────────────┐
│  │   SQLite Database       │ │     │    System Printer            │
│  │   - Products            │ │     │    (via browser or OS)       │
│  │   - Sales               │ │     └──────────────────────────────┘
│  │   - SaleItems           │ │
│  └─────────────────────────┘ │
└───────────────────────────────┘
```

## Data Flow

### 1. Product Display Flow
```
User opens app → React requests products → Main API queries DB → 
Returns JSON → React displays products
```

### 2. Add to Cart Flow
```
User clicks product → React updates local state → 
Cart component re-renders → Displays updated cart
```

### 3. Checkout & Print Flow
```
User clicks Checkout →

Step 1: Save Sale
React → POST /api/sales → Main API →
  - Calculate tax
  - Update stock
  - Save to database
  - Return sale data

Step 2: Print Receipt
React → POST /api/print/receipt → Print Service →
  - Generate HTML receipt
  - Return HTML to browser

Step 3: Display & Print
Browser receives HTML →
  - Opens new window
  - Displays receipt
  - Triggers print dialog
```

## Component Responsibilities

### React Frontend
- **ProductList Component**
  - Display available products
  - Handle product selection
  - Show stock levels

- **Cart Component**
  - Manage cart items
  - Calculate totals
  - Handle checkout process
  - Customer information input

- **API Service**
  - HTTP communication
  - Error handling
  - Data transformation

### Main API (.NET Core)
- **Products Controller**
  - CRUD operations for products
  - Stock management
  - Product search

- **Sales Controller**
  - Create sales transactions
  - Calculate taxes
  - Update inventory
  - Sales history
  - Today's sales summary

- **Database Layer**
  - Entity Framework Core
  - SQLite database
  - Data persistence
  - Relationships management

### Print Service (.NET Core)
- **Print Controller**
  - Receipt generation
  - HTML formatting
  - Printer communication
  - Test printing

## Technology Stack

### Frontend
- **React 18**: UI library
- **Vite**: Build tool & dev server
- **Axios**: HTTP client
- **CSS**: Styling (no framework)

### Backend - Main API
- **.NET 8**: Runtime
- **ASP.NET Core**: Web framework
- **Entity Framework Core**: ORM
- **SQLite**: Database
- **Swagger**: API documentation

### Backend - Print Service
- **.NET 8**: Runtime
- **ASP.NET Core**: Web framework
- **System.Drawing**: Printing (Windows)
- **HTML Generation**: Browser printing

## API Endpoints

### Main API (http://localhost:5000/api)

**Products**
- GET    /products          - List all products
- GET    /products/{id}     - Get product details
- POST   /products          - Create product
- PUT    /products/{id}     - Update product
- DELETE /products/{id}     - Delete product

**Sales**
- GET    /sales             - List all sales
- GET    /sales/{id}        - Get sale details
- POST   /sales             - Create sale
- GET    /sales/today       - Today's sales summary

### Print Service (http://localhost:5001/api)

**Print**
- POST   /print/receipt     - Print receipt
- GET    /print/printers    - List available printers
- POST   /print/test        - Test print

## Database Schema

### Products Table
```
Id          : INTEGER (PK)
Name        : TEXT
Description : TEXT
Price       : DECIMAL
Stock       : INTEGER
Category    : TEXT
```

### Sales Table
```
Id            : INTEGER (PK)
SaleDate      : DATETIME
TotalAmount   : DECIMAL
Tax           : DECIMAL
Subtotal      : DECIMAL
PaymentMethod : TEXT
CustomerName  : TEXT
```

### SaleItems Table
```
Id          : INTEGER (PK)
SaleId      : INTEGER (FK → Sales)
ProductName : TEXT
Quantity    : INTEGER
UnitPrice   : DECIMAL
Total       : DECIMAL
```

## Security Considerations

### Current Implementation (Development)
- CORS enabled for all origins
- No authentication
- No authorization
- Suitable for local development only

### Production Recommendations
- Implement JWT authentication
- Add role-based authorization
- Restrict CORS to specific domains
- Use HTTPS
- Implement rate limiting
- Add input validation
- SQL injection protection (already handled by EF Core)
- Secure sensitive data in environment variables

## Scalability Considerations

### Current Limitations
- SQLite (single file, limited concurrent writes)
- No session management
- No load balancing
- Single machine deployment

### Production Improvements
- Migrate to PostgreSQL/SQL Server
- Add Redis for caching
- Implement message queues
- Containerize with Docker
- Add API gateway
- Implement microservices architecture
- Use cloud services (Azure/AWS)

## Performance

### Expected Performance
- Product listing: < 100ms
- Sale creation: < 200ms
- Receipt generation: < 50ms
- Database queries: < 50ms

### Optimization Opportunities
- Add database indexing
- Implement caching
- Use async/await consistently
- Optimize SQL queries
- Add pagination
- Implement lazy loading

## Monitoring & Logging

### Current State
- Basic console logging
- No monitoring
- No error tracking

### Recommended Additions
- Serilog for structured logging
- Application Insights / ELK stack
- Health check endpoints
- Performance metrics
- Error tracking (Sentry/Raygun)
- Audit trails for sales

## Deployment

### Development
- Run all three services locally
- Use file-based SQLite
- No containerization

### Production Options
1. **Traditional Server**
   - IIS / Nginx
   - Separate services
   - Reverse proxy

2. **Containerized**
   - Docker Compose
   - Kubernetes
   - Cloud platforms

3. **PaaS**
   - Azure App Service
   - AWS Elastic Beanstalk
   - Heroku

## Future Enhancements

### Phase 1 (Essential)
- User authentication
- Role management
- Better error handling
- Input validation

### Phase 2 (Enhanced Features)
- Barcode scanning
- Email receipts
- Return/refund processing
- Discount management

### Phase 3 (Advanced)
- Multi-store support
- Real-time inventory sync
- Analytics dashboard
- Mobile app
- Cloud POS integration

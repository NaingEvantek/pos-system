# Authentication & Authorization Guide

## 🔐 Overview

The POS system now includes a complete authentication and authorization module with:
- ✅ JWT (JSON Web Token) based authentication
- ✅ Secure password hashing with BCrypt
- ✅ Login page with credentials validation
- ✅ User roles (Admin, Manager, Cashier)
- ✅ Protected API endpoints
- ✅ Token storage in localStorage
- ✅ Automatic token refresh
- ✅ Logout functionality

## 🎯 Features

### Backend Authentication
- **JWT Tokens**: 8-hour expiration, configurable
- **Password Hashing**: BCrypt with salt rounds
- **Role-Based Access**: Admin, Manager, Cashier roles
- **Secure Endpoints**: Protected API routes
- **User Management**: User table in database

### Frontend Authentication
- **Login Page**: Beautiful, responsive design
- **Session Management**: Automatic token handling
- **Protected Routes**: Redirect to login if not authenticated
- **User Info Display**: Shows logged-in user in sidebar
- **Logout Button**: Clear session and return to login

## 👥 Default Users

The system comes with 2 pre-configured users:

### Admin Account
- **Username**: `admin`
- **Password**: `admin123`
- **Role**: Admin
- **Email**: admin@pos.com
- **Access**: Full system access

### Cashier Account
- **Username**: `cashier`
- **Password**: `admin123`
- **Role**: Cashier
- **Email**: cashier@pos.com
- **Access**: POS operations

## 🚀 How to Use

### First Time Setup

1. **Delete old database** (if exists):
   ```bash
   cd POS.API
   rm pos.db
   ```

2. **Start the API**:
   ```bash
   dotnet restore
   dotnet run
   ```
   The database will be created with default users.

3. **Start the React app**:
   ```bash
   cd pos-frontend
   npm install
   npm run dev
   ```

4. **Login**:
   - Open http://localhost:3000
   - Use credentials: `admin` / `admin123`
   - Click "Login"

### Login Flow

1. User enters username and password
2. Frontend sends POST request to `/api/auth/login`
3. Backend validates credentials
4. If valid, backend generates JWT token
5. Frontend stores token in localStorage
6. User is redirected to POS dashboard
7. Token is automatically included in all API requests

### Logout Flow

1. User clicks "Logout" button in sidebar
2. Token is removed from localStorage
3. User is redirected to login page

## 🔑 API Endpoints

### Authentication Endpoints

**Login**
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@pos.com",
    "fullName": "System Administrator",
    "role": "Admin",
    "lastLogin": "2024-01-01T10:00:00"
  }
}
```

**Register** (for creating new users)
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "password123",
  "fullName": "New User",
  "role": "Cashier"
}

Response: Same as login
```

### Protected Endpoints

All existing endpoints now require authentication:
- `GET /api/products` - Requires valid JWT token
- `POST /api/sales` - Requires valid JWT token
- etc.

**How to call protected endpoints**:
```javascript
// Token is automatically added by axios interceptor
const response = await api.get('/products');

// Manual way:
fetch('http://localhost:5000/api/products', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

## 🛡️ Security Features

### Password Security
- **BCrypt Hashing**: Industry-standard password hashing
- **Salt Rounds**: Configurable salt rounds (default: 11)
- **No Plain Text**: Passwords never stored in plain text

### Token Security
- **JWT Standard**: Industry-standard JSON Web Tokens
- **Signed Tokens**: HMAC SHA-256 signature
- **Expiration**: 8-hour token lifetime (configurable)
- **Claims**: User ID, username, email, role

### API Security
- **CORS Protection**: Configured for specific origins
- **Token Validation**: All requests validated
- **401 Handling**: Automatic redirect to login
- **Role Claims**: User role embedded in token

## 🔧 Configuration

### Backend (appsettings.json)

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration12345678901234567890",
    "Issuer": "POSSystem",
    "Audience": "POSUsers",
    "ExpirationHours": 8
  }
}
```

**Important**: Change the `SecretKey` in production!

### Frontend (localStorage)

Tokens are stored in browser's localStorage:
- **Key**: `token`
- **User Info**: `user` (JSON string)

## 📝 Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FullName TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'Cashier',
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL,
    LastLogin TEXT
);
```

## 🎨 UI Components

### Login Page
- **Location**: `pos-frontend/src/components/Login.jsx`
- **Features**:
  - Username/password inputs
  - Loading state
  - Error messages
  - Demo credentials display
  - Responsive design

### Sidebar User Info
- **Location**: Updated `Sidebar.jsx`
- **Features**:
  - User avatar with initials
  - Full name display
  - Role badge
  - Logout button

## 🔐 User Roles

### Admin
- Full system access
- Can manage products
- Can view all sales
- Can create/edit/delete everything

### Manager
- Can manage products
- Can view sales reports
- Can process sales
- Limited administrative access

### Cashier
- Can process sales
- Can view products
- Limited to POS operations
- No product management

**Note**: Role-based restrictions are set up in the backend but need to be implemented in controllers using `[Authorize(Roles = "Admin")]` attributes.

## 🚨 Troubleshooting

### Login Not Working

**Error: "Invalid username or password"**
- Check credentials are correct
- Verify database has been created (check for pos.db file)
- Make sure API is running on port 5000

**Error: "Network Error"**
- Verify API is running: http://localhost:5000/swagger
- Check CORS settings in Program.cs
- Verify frontend is on port 3000 or 5173

### Token Issues

**Error: "401 Unauthorized"**
- Token may have expired (8 hour limit)
- Try logging out and back in
- Clear localStorage and login again

**Clear localStorage manually**:
```javascript
// In browser console:
localStorage.removeItem('token');
localStorage.removeItem('user');
```

### Database Issues

**Users not found**:
```bash
# Delete and recreate database
cd POS.API
rm pos.db
dotnet run
# Database will be recreated with default users
```

## 🔄 Adding New Users

### Via API (Postman/cURL)
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newcashier",
    "email": "cashier2@pos.com",
    "password": "secure123",
    "fullName": "New Cashier",
    "role": "Cashier"
  }'
```

### Via Database (SQLite)
```sql
INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, IsActive, CreatedAt)
VALUES (
  'newuser',
  'new@pos.com',
  '$2a$11$hashedpasswordhere',
  'New User',
  'Cashier',
  1,
  datetime('now')
);
```

**Note**: Use BCrypt to hash the password first!

## 📚 Code Examples

### Check if User is Logged In
```javascript
import { isAuthenticated, getCurrentUser } from './services/api';

if (isAuthenticated()) {
  const user = getCurrentUser();
  console.log('Logged in as:', user.username);
}
```

### Logout Programmatically
```javascript
import { logout } from './services/api';

const handleLogout = () => {
  logout();
  window.location.href = '/';
};
```

### Make Authenticated Request
```javascript
import api from './services/api';

// Token is automatically added
const products = await api.get('/products');
```

## 🎯 Next Steps

### Recommended Enhancements

1. **Password Reset**: Add "Forgot Password" functionality
2. **User Profile**: Allow users to update their profile
3. **Admin Panel**: Create user management interface
4. **Audit Logs**: Track user actions
5. **2FA**: Add two-factor authentication
6. **Session Timeout**: Warn users before token expires
7. **Remember Me**: Add persistent login option

## 🔒 Production Checklist

Before deploying to production:

- [ ] Change JWT SecretKey in appsettings.json
- [ ] Use environment variables for secrets
- [ ] Enable HTTPS
- [ ] Update CORS to specific domains
- [ ] Change default user passwords
- [ ] Add rate limiting
- [ ] Implement password complexity requirements
- [ ] Add account lockout after failed attempts
- [ ] Set up proper logging
- [ ] Configure secure cookies
- [ ] Use production database (PostgreSQL/SQL Server)

## 📖 Additional Resources

- JWT.io - https://jwt.io/
- BCrypt - https://github.com/BcryptNet/bcrypt.net
- OWASP Auth Guide - https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html

---

**Security Note**: The default passwords (`admin123`) are for development only. Always change them in production!

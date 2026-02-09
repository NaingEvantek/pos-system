# Authentication Setup Guide

## 🎯 Quick Answer: How to Get Admin Password

**The database does NOT have default users seeded.** You must create them first!

## 🚀 Setup in 3 Easy Steps

### Step 1: Start the API

```bash
cd POS.API
dotnet restore
dotnet run
```

Wait for: `Now listening on: http://localhost:5000`

### Step 2: Create Default Users

**Open your browser and go to:**
```
http://localhost:5000/api/auth/init
```

**OR use curl:**
```bash
curl -X POST http://localhost:5000/api/auth/init
```

You'll see:
```json
{
  "message": "Default users initialized successfully",
  "users": ["admin", "cashier"],
  "credentials": {
    "admin": {
      "username": "admin",
      "password": "admin123"
    },
    "cashier": {
      "username": "cashier",
      "password": "admin123"
    }
  }
}
```

✅ **Done! Users are now created with proper BCrypt password hashes.**

### Step 3: Login

Start the React app:
```bash
cd pos-frontend
npm install
npm run dev
```

Open http://localhost:3000 and login with:
- **Username**: `admin`  
- **Password**: `admin123`

---

## ❓ Why This Approach?

BCrypt password hashing **cannot be done** at compile time or in seed data because:
1. Each hash needs a random salt
2. The BCrypt library must be called at runtime
3. Seed data is compiled, not executed

So we use the `/api/auth/init` endpoint to create users with proper hashing when the app first runs.

---

## 🔄 Alternative: Create Your Own Admin

Instead of using the default users, you can create your own:

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "myadmin",
    "email": "myadmin@example.com",
    "password": "MySecurePassword123!",
    "fullName": "My Admin Name",
    "role": "Admin"
  }'
```

This creates a user with proper password hashing immediately.

---

## 📋 Complete Workflow

```bash
cd POS.API
dotnet restore
dotnet run
```

The API will start on http://localhost:5000

### Step 2: Initialize Default Users

Call the initialization endpoint to create proper password hashes:

**Option A: Using Browser**
- Open: http://localhost:5000/api/auth/init

**Option B: Using curl**
```bash
curl -X POST http://localhost:5000/api/auth/init
```

**Option C: Using Postman**
- Method: POST
- URL: http://localhost:5000/api/auth/init
- Click Send

You'll see a response like:
```json
{
  "message": "Default users initialized successfully",
  "users": ["admin", "cashier"],
  "credentials": {
    "admin": {
      "username": "admin",
      "password": "admin123"
    },
    "cashier": {
      "username": "cashier",
      "password": "admin123"
    }
  }
}
```

### Step 3: Login

Now you can login with:
- **Username**: `admin`
- **Password**: `admin123`

OR

- **Username**: `cashier`
- **Password**: `admin123`

## 📝 What the /init Endpoint Does

The initialization endpoint:
1. Checks if default users exist
2. If users have placeholder hashes, it replaces them with proper BCrypt hashes for "admin123"
3. If users don't exist, it creates them with proper hashes
4. Returns the credentials to use

## 🔄 Alternative: Use Register Endpoint

If you prefer, you can skip the seeded users and create your own:

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "myadmin",
    "email": "myadmin@pos.com",
    "password": "MySecurePassword123",
    "fullName": "My Admin User",
    "role": "Admin"
  }'
```

This will:
- Create the user with proper password hashing
- Return a JWT token
- Allow immediate login

## 🗄️ Manual Database Setup (Advanced)

If you want to manually update the database:

```bash
# Install sqlite3
# Windows: Download from sqlite.org
# Mac: brew install sqlite3
# Linux: sudo apt install sqlite3

# Open the database
cd POS.API
sqlite3 pos.db

# Update admin password
UPDATE Users 
SET PasswordHash = (SELECT '$2a$11$' || hex(randomblob(22))) 
WHERE Username = 'admin';

# Better: Use BCrypt from C# or online tool
# Then update manually:
UPDATE Users 
SET PasswordHash = 'YOUR_BCRYPT_HASH_HERE'
WHERE Username = 'admin';
```

**Note**: It's much easier to just use the `/api/auth/init` endpoint!

## ✅ Verify Setup

Test that everything works:

```bash
# Test login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

You should get a response with a JWT token and user info.

## 🎯 Full Workflow

```bash
# 1. Start API
cd POS.API
dotnet run

# 2. In another terminal, initialize users
curl -X POST http://localhost:5000/api/auth/init

# 3. Start React app
cd ../pos-frontend
npm install
npm run dev

# 4. Open browser
# http://localhost:3000

# 5. Login with:
# Username: admin
# Password: admin123
```

## 🔐 Password Details

The default password `admin123` is hashed using:
- **Algorithm**: BCrypt
- **Salt Rounds**: 11
- **Format**: $2a$11$[hash]

Example BCrypt hash for "admin123":
```
$2a$11$YpTqG.5vz5Z5vz5Z5vz5Zu7W8GqE.QxQxQxQxQxQxQxQxQxQxQxQ
```

Each time you run `/api/auth/init`, a new salt is generated, so the hash will be different but will still verify against "admin123".

## 🚨 Troubleshooting

### "Invalid username or password" after setup
- Make sure you called `/api/auth/init` first
- Check the API logs for any errors
- Verify the database exists: `ls POS.API/pos.db`
- Try deleting the database and recreating:
  ```bash
  cd POS.API
  rm pos.db
  dotnet run
  # Then call /api/auth/init again
  ```

### "User not found"
- The database might not have been created
- Run the API first: `dotnet run`
- Then call `/api/auth/init`

### Can't access /api/auth/init
- Verify API is running: http://localhost:5000/swagger
- Check the port (should be 5000)
- Look for errors in the console

## 🎉 You're Done!

After initialization, you can:
- ✅ Login with admin/admin123
- ✅ Login with cashier/admin123  
- ✅ Create new users via the register endpoint
- ✅ Use the POS system

## 🔒 Security Note

**IMPORTANT**: Change the default password "admin123" in production!

You can change passwords by:
1. Creating new users with strong passwords
2. Disabling the default accounts
3. Or updating the password through user management features

---

**Pro Tip**: Bookmark http://localhost:5000/api/auth/init for quick setup during development!

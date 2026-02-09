#!/bin/bash

echo "=========================================="
echo "POS System - Database Reset Script"
echo "=========================================="
echo ""

cd POS.API

echo "Step 1: Stopping any running API processes..."
pkill -f "dotnet.*POS.API" 2>/dev/null
sleep 2

echo ""
echo "Step 2: Deleting old database files..."
rm -f pos.db pos.db-shm pos.db-wal
echo "Database files deleted."

echo ""
echo "Step 3: Installing/Restoring packages..."
dotnet restore

echo ""
echo "Step 4: Building the API..."
dotnet build

echo ""
echo "=========================================="
echo "Database reset complete!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Run: dotnet run (in POS.API folder)"
echo "2. Initialize users: curl -X POST http://localhost:5000/api/auth/init"
echo "3. Start frontend: cd ../pos-frontend && npm run dev"
echo "4. Login with: admin / admin123"
echo ""
echo "Press Enter to start the API now..."
read

echo ""
echo "Starting API..."
dotnet run

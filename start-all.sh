#!/bin/bash

echo "Starting POS System..."
echo ""

# Start Main API
echo "Starting Main API on port 5000..."
cd POS.API
dotnet run &
API_PID=$!
cd ..

sleep 3

# Start Print Service
echo "Starting Print Service on port 5001..."
cd POS.PrintService
dotnet run &
PRINT_PID=$!
cd ..

sleep 3

# Start React Frontend
echo "Starting React Frontend on port 3000..."
cd pos-frontend
npm run dev &
REACT_PID=$!
cd ..

echo ""
echo "All services started!"
echo ""
echo "Main API: http://localhost:5000"
echo "Print Service: http://localhost:5001"
echo "React App: http://localhost:3000"
echo ""
echo "Process IDs:"
echo "API: $API_PID"
echo "Print Service: $PRINT_PID"
echo "React App: $REACT_PID"
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for user interrupt
wait

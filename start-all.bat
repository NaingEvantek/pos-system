@echo off
echo Starting POS System...
echo.

echo Starting Main API on port 5000...
start "POS API" cmd /k "cd POS.API && dotnet run"

timeout /t 3

echo Starting Print Service on port 5001...
start "Print Service" cmd /k "cd POS.PrintService && dotnet run"

timeout /t 3

echo Starting React Frontend on port 3000...
start "React App" cmd /k "cd pos-frontend && npm run dev"

echo.
echo All services started!
echo.
echo Main API: http://localhost:5000
echo Print Service: http://localhost:5001
echo React App: http://localhost:3000
echo.
echo Press any key to exit (services will continue running)
pause

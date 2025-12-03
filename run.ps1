# Start the application in the background
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet watch run"

# Wait a moment for the server to start
Start-Sleep -Seconds 3

# Open the browser
Start-Process "https://localhost:7106"

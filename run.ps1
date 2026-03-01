Write-Host "Restoring..."
dotnet restore

Write-Host "Building..."
dotnet build

Write-Host "Running tests..."
dotnet test

Write-Host "Starting API..."
Start-Process powershell -WorkingDirectory $PSScriptRoot -ArgumentList "-NoExit", "-Command", "dotnet run --project src/RailcarTrips.Api"

Start-Sleep -Seconds 2

Write-Host "Starting Client..."
Start-Process powershell -WorkingDirectory $PSScriptRoot -ArgumentList "-NoExit", "-Command", "dotnet run --project src/RailcarTrips.Client"

Write-Host "API should be available at http://localhost:5171/swagger"
Write-Host "Client should be available at http://localhost:5066/railcar-trips"

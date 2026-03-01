#!/usr/bin/env bash

echo "Restoring..."
dotnet restore

echo "Building..."
dotnet build

echo "Running tests..."
dotnet test

echo "Starting API..."
dotnet run --project src/RailcarTrips.Api &

sleep 2

echo "Starting Client..."
dotnet run --project src/RailcarTrips.Client &

echo "Done."

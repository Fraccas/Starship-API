#!/bin/bash
set -e

# If seeding is requested, seed only; otherwise, just run API
if [[ "$1" == "--seed" ]]; then
    echo "Running database seed..."
    dotnet StarShipApi.dll --seed
    echo "Seeding finished."
    exit 0
fi

echo "Starting API..."
dotnet StarShipApi.dll

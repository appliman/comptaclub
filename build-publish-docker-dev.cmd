
@echo off
SETLOCAL EnableDelayedExpansion

echo ===================================================================
echo Building StockAssoPro Docker Images with Base Image
echo ===================================================================

docker compose -f ./src/ComptaClub.Blazor/docker-compose-dev.yml build

echo.
echo Starting all services...
docker compose -f ./src/ComptaClub.Blazor/docker-compose-dev.yml down
docker compose -f ./src/ComptaClub.Blazor/docker-compose-dev.yml up -d 

ENDLOCAL
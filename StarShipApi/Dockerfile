# ============================
# BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ENV ASPNETCORE_ENVIRONMENT=Production

COPY StarShipApi.csproj ./
RUN dotnet restore

COPY . .

RUN dotnet tool restore
RUN dotnet publish -c Release -o /app/publish

RUN dotnet tool run dotnet-ef database update --project StarShipApi.csproj --startup-project /src


# ============================
# RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /src/starships.db starships.db

EXPOSE 8080

ENTRYPOINT ["dotnet", "StarShipApi.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore
COPY src/chickko.api/*.csproj ./src/chickko.api/
RUN dotnet restore src/chickko.api/chickko.api.csproj

# Copy everything else and build
COPY src/chickko.api/ ./src/chickko.api/
RUN dotnet publish src/chickko.api/chickko.api.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "chickko.api.dll"]
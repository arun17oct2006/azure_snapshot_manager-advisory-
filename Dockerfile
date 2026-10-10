# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AzureSnapshotManager.sln .
COPY src/AzureSnapshotManager.Core/AzureSnapshotManager.Core.csproj src/AzureSnapshotManager.Core/
COPY src/AzureSnapshotManager.Infrastructure/AzureSnapshotManager.Infrastructure.csproj src/AzureSnapshotManager.Infrastructure/
COPY src/AzureSnapshotManager.Api/AzureSnapshotManager.Api.csproj src/AzureSnapshotManager.Api/
RUN dotnet restore src/AzureSnapshotManager.Api/AzureSnapshotManager.Api.csproj

COPY src/ src/
RUN dotnet publish src/AzureSnapshotManager.Api/AzureSnapshotManager.Api.csproj -c Release -o /app --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Hosting platforms (Render, Koyeb, Cloud Run, ...) tell the container which
# port to listen on through the PORT variable; fall back to 8080 locally.
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} exec dotnet AzureSnapshotManager.Api.dll"]

# Multi-stage build: compile with the full SDK, then run on the much smaller ASP.NET runtime image.

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first and restore packages.
# Docker caches this layer, so packages are only re-downloaded when a .csproj changes.
COPY src/CareEMR.Domain/CareEMR.Domain.csproj                 src/CareEMR.Domain/
COPY src/CareEMR.Application/CareEMR.Application.csproj       src/CareEMR.Application/
COPY src/CareEMR.Infrastructure/CareEMR.Infrastructure.csproj src/CareEMR.Infrastructure/
COPY src/CareEMR.Api/CareEMR.Api.csproj                       src/CareEMR.Api/
RUN dotnet restore src/CareEMR.Api/CareEMR.Api.csproj

# Now copy the source code and publish a Release build.
# (global.json is excluded in .dockerignore: it pins the local SDK, and the image brings its own .NET 10 SDK.)
COPY src/ src/
RUN dotnet publish src/CareEMR.Api/CareEMR.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The .NET image listens on port 8080 and runs as a non-root user ("app") by default.
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "CareEMR.Api.dll"]

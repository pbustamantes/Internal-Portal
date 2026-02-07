FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy Directory.Build.props (shared build settings)
COPY Directory.Build.props ./

# Copy project files for restore
COPY src/Core/InternalPortal.Domain/InternalPortal.Domain.csproj src/Core/InternalPortal.Domain/
COPY src/Core/InternalPortal.Application/InternalPortal.Application.csproj src/Core/InternalPortal.Application/
COPY src/Infrastructure/InternalPortal.Persistence/InternalPortal.Persistence.csproj src/Infrastructure/InternalPortal.Persistence/
COPY src/Infrastructure/InternalPortal.Infrastructure/InternalPortal.Infrastructure.csproj src/Infrastructure/InternalPortal.Infrastructure/
COPY src/Presentation/InternalPortal.API/InternalPortal.API.csproj src/Presentation/InternalPortal.API/

# Restore
RUN dotnet restore src/Presentation/InternalPortal.API/InternalPortal.API.csproj

# Copy source code
COPY src/ src/

# Publish
RUN dotnet publish src/Presentation/InternalPortal.API/InternalPortal.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/uploads

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InternalPortal.API.dll"]

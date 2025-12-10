# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for better caching
COPY ["PulperiaSystem.Web/PulperiaSystem.Web.csproj", "PulperiaSystem.Web/"]
COPY ["PulperiaSystem.Models/PulperiaSystem.Models.csproj", "PulperiaSystem.Models/"]
COPY ["PulperiaSystem.DataAccess/PulperiaSystem.DataAccess.csproj", "PulperiaSystem.DataAccess/"]
COPY ["PulperiaSystem.UI/PulperiaSystem.UI.csproj", "PulperiaSystem.UI/"]

# Restore dependencies
RUN dotnet restore "PulperiaSystem.Web/PulperiaSystem.Web.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/PulperiaSystem.Web"
RUN dotnet build "PulperiaSystem.Web.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "PulperiaSystem.Web.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PulperiaSystem.Web.dll"]

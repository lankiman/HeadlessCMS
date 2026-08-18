# Stage 1: Base Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: SDK Image for Building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project files first to leverage Docker caching for NuGet packages
COPY ["api/api.csproj", "api/"]
COPY ["services/services.csproj", "services/"]
COPY ["data/data.csproj", "data/"]
COPY ["common/common.csproj", "common/"]

# Restore dependencies
RUN dotnet restore "api/api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/api"

# Build the project
RUN dotnet build "api.csproj" -c Release -o /app/build

# Stage 3: Publish the Application
FROM build AS publish
RUN dotnet publish "api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tell Railway how to start your application
ENTRYPOINT ["dotnet", "api.dll"]
# Stage 1: Base Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: SDK Image for Building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything at once so BuildKit doesn't have to resolve individual subpath checksums
COPY . .

# Restore directly targeting the main API project
RUN dotnet restore "Api/Api.csproj"

# Build
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Stage 3: Publish Application
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
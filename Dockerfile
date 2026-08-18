# Stage 1: Base Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: SDK Image for Building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project files using exact repo casing and names
COPY ["HeadlessCMS.API/HeadlessCMS.API.csproj", "HeadlessCMS.API/"]
COPY ["HeadlessCMS.Common/HeadlessCMS.Common.csproj", "HeadlessCMS.Common/"]
COPY ["HeadlessCMS.Data/HeadlessCMS.Data.csproj", "HeadlessCMS.Data/"]
COPY ["HeadlessCMS.Services/HeadlessCMS.Services.csproj", "HeadlessCMS.Services/"]

# Restore dependencies for the API project
RUN dotnet restore "HeadlessCMS.API/HeadlessCMS.API.csproj"

# Copy remaining source code
COPY . .
WORKDIR "/src/HeadlessCMS.API"

# Build project
RUN dotnet build "HeadlessCMS.API.csproj" -c Release -o /app/build

# Stage 3: Publish Application
FROM build AS publish
RUN dotnet publish "HeadlessCMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Start application
ENTRYPOINT ["dotnet", "HeadlessCMS.API.dll"]
# Stage 1: Base Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: SDK Image for Building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Match exact casing of your folders and .csproj files
COPY ["Api/Api.csproj", "Api/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Data/Data.csproj", "Data/"]
COPY ["Common/Common.csproj", "Common/"]

# Restore dependencies
RUN dotnet restore "Api/Api.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/Api"

# Build the project
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Stage 3: Publish the Application
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Api.dll"]
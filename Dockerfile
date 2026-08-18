# Stage 1: Base Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: SDK Image for Building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# --- 1. VIEW BEFORE COPYING CODE ---
RUN echo "\n==================== BEFORE COPY ====================" && \
    ls -la /src && \
    echo "=====================================================\n"

# --- 2. COPY THE REPOSITORY CONTENT ---
COPY . .

# --- 3. VIEW AFTER COPYING CODE ---
RUN echo "\n==================== AFTER COPY =====================" && \
    echo "--- Top Level Files ---" && ls -la /src && \
    echo "\n--- All .csproj Files Found ---" && find . -name "*.csproj" && \
    echo "=====================================================\n"

# --- 4. RESTORE DEPENDENCIES ---
# Note: Update "Api/Api.csproj" if the output from step 3 shows a different casing or subfolder path!
RUN dotnet restore "Api/Api.csproj"

# Build the project
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Stage 3: Publish Application
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Start application
ENTRYPOINT ["dotnet", "Api.dll"]
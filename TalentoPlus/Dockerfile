# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["TalentoPlus.Core/TalentoPlus.Core.csproj", "TalentoPlus.Core/"]
COPY ["TalentoPlus.Infrastructure/TalentoPlus.Infrastructure.csproj", "TalentoPlus.Infrastructure/"]
COPY ["TalentoPlus.API/TalentoPlus.API.csproj", "TalentoPlus.API/"]

# Restore dependencies
RUN dotnet restore "TalentoPlus.API/TalentoPlus.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/TalentoPlus.API"
RUN dotnet build "TalentoPlus.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TalentoPlus.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install fonts for QuestPDF
RUN apt-get update && apt-get install -y libfontconfig1 && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TalentoPlus.API.dll"]


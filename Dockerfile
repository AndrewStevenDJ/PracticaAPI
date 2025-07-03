# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copiar solo el archivo csproj primero (para optimizar caché)
COPY ["PracticaAPI.csproj", "."]
RUN dotnet restore "PracticaAPI.csproj"

# 2. Copiar el resto de los archivos
COPY . .

# Publicar en modo Release
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --fro
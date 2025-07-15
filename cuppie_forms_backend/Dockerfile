# Базовый рантайм слой
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Слой сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release 
WORKDIR /src

# Копируем только проекты и sln
COPY src/Cuppie.Api/Cuppie.Api.csproj Cuppie.Api/
COPY src/Cuppie.Application/Cuppie.Application.csproj Cuppie.Application/
COPY src/Cuppie.Domain/Cuppie.Domain.csproj Cuppie.Domain/
COPY src/Cuppie.Infrastructure/Cuppie.Infrastructure.csproj Cuppie.Infrastructure/

# Восстановление зависимостей
RUN dotnet restore Cuppie.Api/Cuppie.Api.csproj

# Копируем остальной код
COPY src/ .

# Сборка
WORKDIR /src/Cuppie.Api
RUN dotnet build "Cuppie.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация
FROM build AS publish
RUN dotnet publish "Cuppie.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный контейнер
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Cuppie.Api.dll"]

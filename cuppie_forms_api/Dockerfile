FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["cuppie_forms_api/cuppie_forms_api.csproj", "cuppie_forms_api/"]
RUN dotnet restore "cuppie_forms_api/cuppie_forms_api.csproj"
COPY . .
WORKDIR "/src/cuppie_forms_api"
RUN dotnet build "cuppie_forms_api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "cuppie_forms_api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cuppie_forms_api.dll"]

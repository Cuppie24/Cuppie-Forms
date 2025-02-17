FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file and restore as distinct layers
COPY  cuppie/cuppie.csproj .
RUN dotnet restore  

# Copy source code and publish app
COPY  cuppie/. .
RUN dotnet publish -o /app


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 5000
EXPOSE 5001
WORKDIR /app
COPY  --from=build /app .
ENTRYPOINT ["./cuppie"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /source

# Copy project file and restore as distinct layers
COPY  cuppie/cuppie.csproj .
RUN dotnet restore -a $TARGETARCH   

# Copy source code and publish app
COPY  cuppie/. .
RUN dotnet publish -a $TARGETARCH -o /app


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 5000
EXPOSE 5001
WORKDIR /app
COPY  --from=build /app .
ENTRYPOINT ["./cuppie"]

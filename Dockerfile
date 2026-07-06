# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/VocaPlay.Domain/VocaPlay.Domain.csproj src/VocaPlay.Domain/
COPY src/VocaPlay.Application/VocaPlay.Application.csproj src/VocaPlay.Application/
COPY src/VocaPlay.Infrastructure/VocaPlay.Infrastructure.csproj src/VocaPlay.Infrastructure/
COPY src/VocaPlay.Api/VocaPlay.Api.csproj src/VocaPlay.Api/
RUN dotnet restore src/VocaPlay.Api/VocaPlay.Api.csproj

COPY src/ src/
RUN dotnet publish src/VocaPlay.Api/VocaPlay.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "VocaPlay.Api.dll"]

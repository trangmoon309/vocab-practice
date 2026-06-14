FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["VocaPlay.Api/VocaPlay.Api.csproj", "VocaPlay.Api/"]
COPY ["VocaPlay.Application/VocaPlay.Application.csproj", "VocaPlay.Application/"]
COPY ["VocaPlay.Infrastructure/VocaPlay.Infrastructure.csproj", "VocaPlay.Infrastructure/"]
COPY ["VocaPlay.Domain/VocaPlay.Domain.csproj", "VocaPlay.Domain/"]
RUN dotnet restore "VocaPlay.Api/VocaPlay.Api.csproj"

COPY . .
WORKDIR /src/VocaPlay.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "VocaPlay.Api.dll"]

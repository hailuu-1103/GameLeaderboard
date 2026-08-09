FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GameLeaderboard.API/GameLeaderboard.API.csproj", "GameLeaderboard.API/"]
RUN dotnet restore "GameLeaderboard.API/GameLeaderboard.API.csproj"
COPY . .
WORKDIR "/src/GameLeaderboard.API"
RUN dotnet build "./GameLeaderboard.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GameLeaderboard.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameLeaderboard.API.dll"]

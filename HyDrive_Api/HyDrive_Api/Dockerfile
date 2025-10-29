FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HyDrive_Api/HyDrive_Api.csproj", "HyDrive_Api/"]
RUN dotnet restore "HyDrive_Api/HyDrive_Api.csproj"
COPY . .
WORKDIR "/src/HyDrive_Api"
RUN dotnet build "./HyDrive_Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HyDrive_Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HyDrive_Api.dll"]

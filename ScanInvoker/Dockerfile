FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
RUN mkdir -p /data/projectloader/files && chmod 0777 /data/projectloader/files
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ScanInvoker/ScanInvoker.csproj", "ScanInvoker/"]
RUN dotnet restore "ScanInvoker/ScanInvoker.csproj"
COPY . .
WORKDIR "/src/ScanInvoker"
RUN dotnet build "ScanInvoker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ScanInvoker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScanInvoker.dll"]

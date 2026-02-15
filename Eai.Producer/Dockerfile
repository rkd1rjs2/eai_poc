# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 프로젝트 파일 복사 및 Restore
COPY ["Eai.Producer/Eai.Producer.csproj", "Eai.Producer/"]
COPY ["Eai.Infrastructure/Eai.Infrastructure.csproj", "Eai.Infrastructure/"]
COPY ["Eai.Shared/Eai.Shared.csproj", "Eai.Shared/"]
RUN dotnet restore "Eai.Producer/Eai.Producer.csproj"

# 전체 소스 복사 및 Build
COPY . .
WORKDIR "/src/Eai.Producer"
RUN dotnet build "Eai.Producer.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Eai.Producer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage (Runtime)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eai.Producer.dll"]

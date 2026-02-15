# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Eai.Transformer/Eai.Transformer.csproj", "Eai.Transformer/"]
COPY ["Eai.Infrastructure/Eai.Infrastructure.csproj", "Eai.Infrastructure/"]
COPY ["Eai.Shared/Eai.Shared.csproj", "Eai.Shared/"]
RUN dotnet restore "Eai.Transformer/Eai.Transformer.csproj"

COPY . .
WORKDIR "/src/Eai.Transformer"
RUN dotnet build "Eai.Transformer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eai.Transformer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime (Transformer는 백그라운드 서비스이므로 runtime 이미지 사용)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eai.Transformer.dll"]

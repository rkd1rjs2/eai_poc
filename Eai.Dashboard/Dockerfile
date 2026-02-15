# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Eai.Dashboard/Eai.Dashboard.csproj", "Eai.Dashboard/"]
COPY ["Eai.Infrastructure/Eai.Infrastructure.csproj", "Eai.Infrastructure/"]
COPY ["Eai.Shared/Eai.Shared.csproj", "Eai.Shared/"]
RUN dotnet restore "Eai.Dashboard/Eai.Dashboard.csproj"

COPY . .
WORKDIR "/src/Eai.Dashboard"
RUN dotnet build "Eai.Dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eai.Dashboard.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime (Dashboard는 Blazor Server이므로 aspnet 이미지 사용)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Eai.Dashboard.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NotificationSystem.sln ./
COPY src ./src

RUN dotnet restore NotificationSystem.sln
RUN dotnet publish src/Hosts/Worker/Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Worker.dll"]

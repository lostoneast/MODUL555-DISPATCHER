# API: .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Modul555.Dispatcher.Api/DispatcherApp.csproj Modul555.Dispatcher.Api/
RUN dotnet restore Modul555.Dispatcher.Api/DispatcherApp.csproj
COPY src/Modul555.Dispatcher.Api/ Modul555.Dispatcher.Api/
RUN dotnet publish Modul555.Dispatcher.Api/DispatcherApp.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DispatcherApp.dll"]

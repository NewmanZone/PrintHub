FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PrintHub.sln ./
COPY src/PrintHub.Api/PrintHub.Api.csproj src/PrintHub.Api/
COPY src/PrintHub.Core/PrintHub.Core.csproj src/PrintHub.Core/
COPY src/PrintHub.Infrastructure/PrintHub.Infrastructure.csproj src/PrintHub.Infrastructure/
RUN dotnet restore src/PrintHub.Api/PrintHub.Api.csproj

COPY . .
RUN dotnet publish src/PrintHub.Api/PrintHub.Api.csproj --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PrintHub.Api.dll"]

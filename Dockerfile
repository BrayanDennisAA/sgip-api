FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Sgip.slnx ./
COPY src/Sgip.Domain/Sgip.Domain.csproj src/Sgip.Domain/
COPY src/Sgip.Application/Sgip.Application.csproj src/Sgip.Application/
COPY src/Sgip.Infrastructure/Sgip.Infrastructure.csproj src/Sgip.Infrastructure/
COPY src/Sgip.WebApi/Sgip.WebApi.csproj src/Sgip.WebApi/
COPY tests/Sgip.UnitTests/Sgip.UnitTests.csproj tests/Sgip.UnitTests/
COPY tests/Sgip.IntegrationTests/Sgip.IntegrationTests.csproj tests/Sgip.IntegrationTests/

RUN dotnet restore

COPY src/ src/
COPY tests/ tests/

RUN dotnet publish src/Sgip.WebApi/Sgip.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .


EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Sgip.WebApi.dll"]

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Hermenos.csproj", "./"]
RUN dotnet restore "Hermenos.csproj"

COPY . .
RUN dotnet publish "Hermenos.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=10000

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet Hermenos.dll --urls http://0.0.0.0:${PORT:-10000}"]

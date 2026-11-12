FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY EventPack.csproj ./
RUN dotnet restore EventPack.csproj
COPY . .
RUN dotnet publish EventPack.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
USER $APP_UID
ENTRYPOINT ["dotnet", "EventPack.dll"]

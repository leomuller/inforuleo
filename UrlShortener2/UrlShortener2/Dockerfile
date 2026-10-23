# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy just the csproj first, restore, THEN copy the rest of the source.
# Docker caches each instruction as a layer; as long as the csproj hasn't
# changed, this restore layer is reused even when only .cs files change.
COPY UrlShortener2/UrlShortener2.csproj UrlShortener2/
RUN dotnet restore UrlShortener2/UrlShortener2.csproj

COPY UrlShortener2/ UrlShortener2/
RUN dotnet publish UrlShortener2/UrlShortener2.csproj -c Release -o /app/publish --no-restore

# ---- runtime stage ----
# Smaller image: no SDK/compiler, just the ASP.NET runtime needed to run the published app.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UrlShortener2.dll"]

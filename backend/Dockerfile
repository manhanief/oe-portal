# ---------- 1. Build: compile and publish the API ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore packages first, so Docker can cache this step when only code changes
COPY Resto.Api/Resto.Api.csproj Resto.Api/
RUN dotnet restore Resto.Api/Resto.Api.csproj

COPY Resto.Api/ Resto.Api/
RUN dotnet publish Resto.Api/Resto.Api.csproj -c Release -o /app --no-restore

# ---------- 2. Run: small runtime image, no SDK or source code ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Folder for login-cookie encryption keys (kept in a volume so logins survive restarts)
RUN mkdir -p /keys && chown "$APP_UID" /keys

COPY --from=build /app .

# Run as the image's built-in non-root user, not as root
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Resto.Api.dll"]

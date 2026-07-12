# ── build ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app

# ── runtime ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
# Хостинг обычно прокидывает порт через $PORT (Program.cs это учитывает).
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InstagramExtraApi.dll"]

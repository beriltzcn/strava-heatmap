# ---------- 1. asama: React arayuzunu derle ----------
FROM node:22-alpine AS client-build

WORKDIR /client
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/ ./
RUN npm run build

# ---------- 2. asama: .NET API'yi derle ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build

WORKDIR /src
COPY src/StravaHeatmap.Api/StravaHeatmap.Api.csproj ./StravaHeatmap.Api/
RUN dotnet restore ./StravaHeatmap.Api/StravaHeatmap.Api.csproj
COPY src/StravaHeatmap.Api/ ./StravaHeatmap.Api/
RUN dotnet publish ./StravaHeatmap.Api/StravaHeatmap.Api.csproj -c Release -o /app

# ---------- 3. asama: calisma ortami ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app
COPY --from=api-build /app ./

# React'in derlenmis hali API'nin wwwroot klasorune girer.
# Boylece tek konteyner, tek origin, CORS derdi yok.
COPY --from=client-build /client/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StravaHeatmap.Api.dll"]

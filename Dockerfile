# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# CPM va NuGet config (layer cache — bu fayllar kamdan-kam o'zgaradi)
COPY Directory.Build.props Directory.Packages.props NuGet.Config ./

# Solution va har bir csproj alohida (restore cache uchun)
COPY ucms-api.sln ./
COPY src/Ucms.Domain/Ucms.Domain.csproj            src/Ucms.Domain/
COPY src/Ucms.Application/Ucms.Application.csproj  src/Ucms.Application/
COPY src/Ucms.Infrastructure/Ucms.Infrastructure.csproj src/Ucms.Infrastructure/
COPY src/Ucms.Api/Ucms.Api.csproj                  src/Ucms.Api/

RUN dotnet restore

# Manba kodini ko'chirish va publish
COPY src/ src/
RUN dotnet publish src/Ucms.Api/Ucms.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user (xavfsizlik)
RUN adduser --disabled-password --gecos "" appuser

# Doimiy saqlash uchun umumiy papka (avatars, hujjatlar va h.k. — har biri o'z
# subfolderini runtime'da Handlerlar/Resolverlar o'zi yaratadi). Volume sifatida
# e'lon qilinadi — shunda docker-compose'da bog'lansa, image qayta build
# qilinganda fayllar saqlanib qoladi.
RUN mkdir -p /app/storage && chown -R appuser:appuser /app/storage
VOLUME ["/app/storage"]

USER appuser

COPY --from=build --chown=appuser:appuser /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Ucms.Api.dll"]

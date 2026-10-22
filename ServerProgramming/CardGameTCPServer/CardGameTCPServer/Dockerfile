# Build natively on the CI machine's CPU; .NET IL runs on any CPU anyway
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Dependencies first: this layer stays cached until the .csproj changes
COPY CardGameTCPServer.csproj .
RUN dotnet restore

# Then the code, which changes often
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# ---------- Stage 2: run (runtime only) ----------
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .

# Data folder, owned by the non-root user
RUN mkdir /data && chown app:app /data
COPY --chown=app:app config.json /data/
WORKDIR /data

USER app
EXPOSE 7777
ENTRYPOINT ["dotnet", "/app/CardGameTCPServer.dll"]

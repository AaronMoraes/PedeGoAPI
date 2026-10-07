FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Banco SQLite e imagens enviadas ficam em /data. Monte um volume aqui para não perder dados.
RUN mkdir -p /data && chown -R app:app /data
ENV DB_PATH=/data/gestaodepedidos.db
ENV UPLOADS_PATH=/data/uploads
VOLUME /data

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# Não roda como root dentro do container.
USER app

ENTRYPOINT ["dotnet", "GestaodePedidosAPI.dll"]

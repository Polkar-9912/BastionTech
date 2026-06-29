# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiamos todo el código fuente al contenedor de un solo golpe
COPY . ./

# Restauramos dependencias y compilamos
RUN dotnet restore
RUN dotnet publish -c Release -o out

# 2. Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Configurar el puerto dinámico que asigna Render
ENV ASPNETCORE_URLS=http://*:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BastionTech.dll"]
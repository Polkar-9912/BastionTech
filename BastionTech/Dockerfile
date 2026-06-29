# 1. Etapa de compilación (Actualizada a .NET 10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copiamos absolutamente todo el repositorio al contenedor
COPY . ./

# Buscamos el .csproj automáticamente y lo compilamos
RUN find . -name "*.csproj" -exec dotnet publish {} -c Release -o /app/out \;

# 2. Etapa de ejecución (Actualizada a .NET 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Configurar el puerto dinámico que asigna Render
ENV ASPNETCORE_URLS=http://*:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BastionTech.dll"]
# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiamos absolutamente todo el repositorio al contenedor
COPY . ./

# 🌟 MAGIA AQUÍ: Buscamos el .csproj automáticamente en cualquier subcarpeta y lo publicamos.
# (A partir de .NET 6+, el comando publish ya ejecuta un restore implícito, así que nos ahorramos un paso).
RUN find . -name "*.csproj" -exec dotnet publish {} -c Release -o /app/out \;

# 2. Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Configurar el puerto dinámico que asigna Render
ENV ASPNETCORE_URLS=http://*:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "BastionTech.dll"]
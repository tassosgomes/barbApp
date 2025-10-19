# Estágio 1: Build
# Utiliza a imagem do SDK do .NET 8 para compilar a aplicação.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos .csproj e restaura as dependências primeiro.
# Isso aproveita o cache do Docker. Se os projetos não mudarem, as dependências não são baixadas novamente.
COPY ["BarbApp.API/BarbApp.API.csproj", "BarbApp.API/"]
COPY ["BarbApp.Application/BarbApp.Application.csproj", "BarbApp.Application/"]
COPY ["BarbApp.Domain/BarbApp.Domain.csproj", "BarbApp.Domain/"]
COPY ["BarbApp.Infrastructure/BarbApp.Infrastructure.csproj", "BarbApp.Infrastructure/"]
RUN dotnet restore "BarbApp.API/BarbApp.API.csproj"

# Copia o restante do código-fonte.
COPY . .
WORKDIR "/src/BarbApp.API"
# Publica a aplicação em modo Release para a pasta /app/publish.
RUN dotnet publish "BarbApp.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio 2: Imagem Final
# Utiliza a imagem de runtime do ASP.NET, que é menor e mais segura.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia os artefatos publicados do estágio de build.
COPY --from=build /app/publish .

# Expõe a porta 8080 para tráfego externo e configura o ASP.NET Core para ouvir nela.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Define o ponto de entrada do container para executar a aplicação.
ENTRYPOINT ["dotnet", "BarbApp.API.dll"]

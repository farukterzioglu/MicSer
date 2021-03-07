FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY "MicSer.IdentityServer.csproj" .
RUN dotnet restore "MicSer.IdentityServer.csproj"
COPY . .

RUN dotnet build "MicSer.IdentityServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MicSer.IdentityServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MicSer.IdentityServer.dll"] 

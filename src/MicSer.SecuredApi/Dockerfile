FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY "MicSer.SecuredApi.csproj" .
RUN dotnet restore "MicSer.SecuredApi.csproj"
COPY . .

RUN dotnet build "MicSer.SecuredApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MicSer.SecuredApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MicSer.SecuredApi.dll"] 

#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["InfoServerGRPC/InfoServerGRPC.csproj", "InfoServerGRPC/"]
COPY ["InfoServiceGRPC/InfoServiceGRPC.csproj", "InfoServiceGRPC/"]
COPY ["InfoService/InfoService.csproj", "InfoService/"]
COPY ["ApiKeyManager/ApiKeyManager.csproj", "ApiKeyManager/"]
COPY ["Helper/Helper.csproj", "Helper/"]
COPY ["JwtTokenManager/JwtTokenManager.csproj", "JwtTokenManager/"]
RUN dotnet restore "InfoServerGRPC/InfoServerGRPC.csproj"
COPY . .
WORKDIR "/src/InfoServerGRPC"
RUN dotnet build "InfoServerGRPC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InfoServerGRPC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InfoServerGRPC.dll"]
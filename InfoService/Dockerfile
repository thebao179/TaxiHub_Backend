#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["InfoService/InfoService.csproj", "InfoService/"]
RUN dotnet restore "InfoService/InfoService.csproj"
COPY . .
WORKDIR "/src/InfoService"
RUN dotnet build "InfoService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InfoService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InfoService.dll"]
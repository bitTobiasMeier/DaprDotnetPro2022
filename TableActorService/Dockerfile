#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TableActorService/TableActorService.csproj", "TableActorService/"]
COPY ["TableActor.Contracts/TableActor.Contracts.csproj", "TableActor.Contracts/"]
RUN dotnet restore "TableActorService/TableActorService.csproj"
COPY . .
WORKDIR "/src/TableActorService"
RUN dotnet build "TableActorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TableActorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TableActorService.dll"]

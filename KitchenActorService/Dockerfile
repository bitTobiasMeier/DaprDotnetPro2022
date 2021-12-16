#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KitchenActorService/KitchenActorService.csproj", "KitchenActorService/"]
COPY ["KitchenActor.Contracts/KitchenActor.Contracts.csproj", "KitchenActor.Contracts/"]
RUN dotnet restore "KitchenActorService/KitchenActorService.csproj"
COPY . .
WORKDIR "/src/KitchenActorService"
RUN dotnet build "KitchenActorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KitchenActorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KitchenActorService.dll"]

#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OrderActorService/OrderActorService.csproj", "OrderActorService/"]
COPY ["OrderActor.Contracts/OrderActor.Contracts.csproj", "OrderActor.Contracts/"]
RUN dotnet restore "OrderActorService/OrderActorService.csproj"
COPY . .
WORKDIR "/src/OrderActorService"
RUN dotnet build "OrderActorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderActorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderActorService.dll"]

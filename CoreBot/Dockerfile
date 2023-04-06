FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CoreBot/CoreBot.csproj", "CoreBot/"]
RUN dotnet restore "CoreBot/CoreBot.csproj"
COPY . .
WORKDIR "/src/CoreBot"
RUN dotnet build "CoreBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoreBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreBot.dll"]

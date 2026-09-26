#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
RUN mkdir -p /app/data && chown $APP_UID /app/data
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY ["src/ComptaClub.Blazor/ComptaClub.Blazor.csproj", "src/ComptaClub.Blazor/"]
COPY . .
RUN dotnet restore "src/ComptaClub.Blazor/ComptaClub.Blazor.csproj"
WORKDIR "/src/src/ComptaClub.Blazor"
RUN dotnet build "ComptaClub.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComptaClub.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComptaClub.Blazor.dll"]

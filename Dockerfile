#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Prepare writable storage outside the shell-less runtime image.
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS directories
RUN mkdir -p /app/data

# Keep ICU and time zone data for French accounting formats.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
COPY --from=directories --chown=$APP_UID:$APP_UID /app/data /app/data
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM directories AS build
WORKDIR /src
COPY ["src/ComptaClub.Blazor/ComptaClub.Blazor.csproj", "src/ComptaClub.Blazor/"]
COPY . .
RUN dotnet restore "src/ComptaClub.Blazor/ComptaClub.Blazor.csproj"
WORKDIR "/src/src/ComptaClub.Blazor"
RUN dotnet build "ComptaClub.Blazor.csproj" -c Release --no-restore -o /app/build

FROM build AS publish
# RequiresAspNetWebAssets in the project makes the initial restore include the bootstrap.
RUN dotnet publish "ComptaClub.Blazor.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false
RUN test -s /app/publish/wwwroot/_framework/blazor.web.js

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComptaClub.Blazor.dll"]

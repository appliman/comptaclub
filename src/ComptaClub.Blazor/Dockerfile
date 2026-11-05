# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Prepare writable storage outside the shell-less runtime image.
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS directories
RUN mkdir -p /app/data

# This stage is used when running from VS in fast mode (Default for Debug configuration).
# Keep ICU and time zone data for French accounting formats.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
COPY --from=directories --chown=$APP_UID:$APP_UID /app/data /app/data
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM directories AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ComptaClub.Blazor/ComptaClub.Blazor.csproj", "src/ComptaClub.Blazor/"]
COPY ["src/ComptaClub.Core/ComptaClub.Core.csproj", "src/ComptaClub.Core/"]
COPY ["src/ComptaClub.Contracts/ComptaClub.Contracts.csproj", "src/ComptaClub.Contracts/"]
COPY ["src/ComptaClub.Datas/ComptaClub.Datas.csproj", "src/ComptaClub.Datas/"]
COPY ["src/ComptaClub.Datas.MsSql/ComptaClub.Datas.MsSql.csproj", "src/ComptaClub.Datas.MsSql/"]
COPY ["src/ComptaClub.Datas.Sqlite/ComptaClub.Datas.Sqlite.csproj", "src/ComptaClub.Datas.Sqlite/"]
COPY ["src/ComptaClub.EntityFramework/ComptaClub.EntityFramework.csproj", "src/ComptaClub.EntityFramework/"]
COPY ["src/ComptaClub.Import/ComptaClub.Import.csproj", "src/ComptaClub.Import/"]
RUN dotnet restore "./src/ComptaClub.Blazor/ComptaClub.Blazor.csproj"
COPY . .
WORKDIR "/src/src/ComptaClub.Blazor"
RUN dotnet build "./ComptaClub.Blazor.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# RequiresAspNetWebAssets in the project makes the initial restore include the bootstrap.
RUN dotnet publish "./ComptaClub.Blazor.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false
RUN test -s /app/publish/wwwroot/_framework/blazor.web.js

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComptaClub.Blazor.dll"]

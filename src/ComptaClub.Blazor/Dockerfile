# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
RUN mkdir -p /app/data && chown $APP_UID /app/data
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
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
RUN dotnet build "./ComptaClub.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ComptaClub.Blazor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ComptaClub.Blazor.dll"]

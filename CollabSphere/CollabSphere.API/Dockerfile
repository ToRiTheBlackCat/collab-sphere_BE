# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_HTTPS_PORTS=8081 \ 
	#ASPNETCORE_HTTP_PORTS=8080 \
	DOTNET_RUNNING_IN_CONTAINER=true \
    #ASPNETCORE_ENVIRONMENT=Development \
	ASPNETCORE_URLS=http://+:8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CollabSphere.API/CollabSphere.API.csproj", "CollabSphere.API/"]
COPY ["CollabSphere.Application/CollabSphere.Application.csproj", "CollabSphere.Application/"]
COPY ["CollabSphere.Domain/CollabSphere.Domain.csproj", "CollabSphere.Domain/"]
COPY ["CollabSphere.Infrastructure/CollabSphere.Infrastructure.csproj", "CollabSphere.Infrastructure/"]
RUN dotnet restore "./CollabSphere.API/CollabSphere.API.csproj"
COPY . .
WORKDIR "/src/CollabSphere.API"
RUN dotnet build "./CollabSphere.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CollabSphere.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CollabSphere.API.dll"]
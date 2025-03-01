# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/identity/NuGet.Config", "services/identity/"]
COPY ["services/saas/NuGet.Config", "services/saas/"]
COPY ["services/administration/host/Tasky.Administration.HttpApi.Host/Tasky.Administration.HttpApi.Host.csproj", "services/administration/host/Tasky.Administration.HttpApi.Host/"]
COPY ["shared/Tasky.Microservice.Shared/Tasky.Microservice.Shared.csproj", "shared/Tasky.Microservice.Shared/"]
COPY ["services/administration/src/Tasky.Administration.EntityFrameworkCore/Tasky.Administration.EntityFrameworkCore.csproj", "services/administration/src/Tasky.Administration.EntityFrameworkCore/"]
COPY ["shared/Tasky.Shared/Tasky.Shared.csproj", "shared/Tasky.Shared/"]
COPY ["services/administration/src/Tasky.Administration.Domain/Tasky.Administration.Domain.csproj", "services/administration/src/Tasky.Administration.Domain/"]
COPY ["services/administration/src/Tasky.Administration.Domain.Shared/Tasky.Administration.Domain.Shared.csproj", "services/administration/src/Tasky.Administration.Domain.Shared/"]
COPY ["shared/Tasky.Hosting.Shared/Tasky.Hosting.Shared.csproj", "shared/Tasky.Hosting.Shared/"]
COPY ["shared/Tasky.ServiceDefaults/Tasky.ServiceDefaults.csproj", "shared/Tasky.ServiceDefaults/"]
COPY ["services/identity/src/Tasky.IdentityService.Application.Contracts/Tasky.IdentityService.Application.Contracts.csproj", "services/identity/src/Tasky.IdentityService.Application.Contracts/"]
COPY ["services/identity/src/Tasky.IdentityService.Domain.Shared/Tasky.IdentityService.Domain.Shared.csproj", "services/identity/src/Tasky.IdentityService.Domain.Shared/"]
COPY ["services/identity/src/Tasky.IdentityService.EntityFrameworkCore/Tasky.IdentityService.EntityFrameworkCore.csproj", "services/identity/src/Tasky.IdentityService.EntityFrameworkCore/"]
COPY ["services/identity/src/Tasky.IdentityService.Domain/Tasky.IdentityService.Domain.csproj", "services/identity/src/Tasky.IdentityService.Domain/"]
COPY ["services/saas/src/Tasky.SaaS.Application.Contracts/Tasky.SaaS.Application.Contracts.csproj", "services/saas/src/Tasky.SaaS.Application.Contracts/"]
COPY ["services/administration/src/Tasky.Administration.Application.Contracts/Tasky.Administration.Application.Contracts.csproj", "services/administration/src/Tasky.Administration.Application.Contracts/"]
COPY ["services/saas/src/Tasky.SaaS.Domain.Shared/Tasky.SaaS.Domain.Shared.csproj", "services/saas/src/Tasky.SaaS.Domain.Shared/"]
COPY ["services/administration/src/Tasky.Administration.HttpApi/Tasky.Administration.HttpApi.csproj", "services/administration/src/Tasky.Administration.HttpApi/"]
COPY ["services/administration/src/Tasky.Administration.Application/Tasky.Administration.Application.csproj", "services/administration/src/Tasky.Administration.Application/"]
RUN dotnet restore "./services/administration/host/Tasky.Administration.HttpApi.Host/Tasky.Administration.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/administration/host/Tasky.Administration.HttpApi.Host"
RUN dotnet build "./Tasky.Administration.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tasky.Administration.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tasky.Administration.HttpApi.Host.dll"]
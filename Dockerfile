# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project definition files first for efficient layer caching
COPY ["NexaCommerce.Api/NexaCommerce.Api.csproj", "NexaCommerce.Api/"]
COPY ["NexaCommerce.Application/NexaCommerce.Application.csproj", "NexaCommerce.Application/"]
COPY ["NexaCommerce.Common/NexaCommerce.Common.csproj", "NexaCommerce.Common/"]
COPY ["NexaCommerce.Contracts/NexaCommerce.Contracts.csproj", "NexaCommerce.Contracts/"]
COPY ["NexaCommerce.Data/NexaCommerce.Data.csproj", "NexaCommerce.Data/"]
COPY ["NexaCommerce.Domain/NexaCommerce.Domain.csproj", "NexaCommerce.Domain/"]
COPY ["NexaCommerce.Repository/NexaCommerce.Repository.csproj", "NexaCommerce.Repository/"]
COPY ["NexaCommerce.Security/NexaCommerce.Security.csproj", "NexaCommerce.Security/"]

RUN dotnet restore "NexaCommerce.Api/NexaCommerce.Api.csproj"

# Copy full source and publish
COPY . .
WORKDIR "/src/NexaCommerce.Api"
RUN dotnet publish "NexaCommerce.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexaCommerce.Api.dll"]

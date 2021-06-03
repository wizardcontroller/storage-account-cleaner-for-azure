#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["com.ataxlab.functions.table.retention.dashboard/com.ataxlab.functions.table.retention.dashboard.csproj", "com.ataxlab.functions.table.retention.dashboard/"]
COPY ["com.ataxlab.azure.table.retention.models/com.ataxlab.azure.table.retention.models.csproj", "com.ataxlab.azure.table.retention.models/"]
COPY ["com.ataxlab.azure.table.retention.services/com.ataxlab.azure.table.retention.services.csproj", "com.ataxlab.azure.table.retention.services/"]


RUN dotnet restore "com.ataxlab.functions.table.retention.dashboard/com.ataxlab.functions.table.retention.dashboard.csproj"
RUN dotnet restore "com.ataxlab.azure.table.retention.models/com.ataxlab.azure.table.retention.models.csproj"

RUN dotnet restore "com.ataxlab.azure.table.retention.services/com.ataxlab.azure.table.retention.services.csproj"
COPY . .
WORKDIR "/src/com.ataxlab.functions.table.retention.dashboard"
RUN dotnet build "com.ataxlab.functions.table.retention.dashboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "com.ataxlab.functions.table.retention.dashboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "com.ataxlab.functions.table.retention.dashboard.dll"]
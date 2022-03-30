FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["isolaatti_API.csproj", "./"]
RUN dotnet restore "isolaatti_API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "isolaatti_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "isolaatti_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "isolaatti_API.dll"]

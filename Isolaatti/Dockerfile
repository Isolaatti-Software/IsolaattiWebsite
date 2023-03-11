FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

#ARG dbcon
#ARG keysdbcon
#ARG sendgrid
#
#ENV db_connection_string=$dbcon
#ENV key_database_con_string=$keysdbcon
#ENV send_grid_api_key=$sendgrid

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Isolaatti.csproj", "./"]
RUN dotnet restore "Isolaatti.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Isolaatti.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Isolaatti.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Isolaatti.dll"]

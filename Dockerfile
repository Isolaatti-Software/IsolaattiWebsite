FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

#ARG dbcon
#ARG keysdbcon
#ARG sendgrid
#
#ENV db_connection_string=$dbcon
#ENV key_database_con_string=$keysdbcon
#ENV send_grid_api_key=$sendgrid

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
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

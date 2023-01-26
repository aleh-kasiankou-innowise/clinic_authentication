FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Innowise.Clinic.Auth/Innowise.Clinic.Auth.csproj", "Innowise.Clinic.Auth/"]
COPY ["Innowise.Clinic.Auth.Persistence.Extensions/Innowise.Clinic.Auth.Persistence.Extensions.csproj", "Innowise.Clinic.Auth.Persistence.Extensions/"]
COPY ["Innowise.Clinic.Auth.Persistence/Innowise.Clinic.Auth.Persistence.csproj", "Innowise.Clinic.Auth.Persistence/"]
RUN dotnet restore "Innowise.Clinic.Auth/Innowise.Clinic.Auth.csproj"
COPY . .
WORKDIR "/src/Innowise.Clinic.Auth"
RUN dotnet build "Innowise.Clinic.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Innowise.Clinic.Auth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Innowise.Clinic.Auth.dll"]

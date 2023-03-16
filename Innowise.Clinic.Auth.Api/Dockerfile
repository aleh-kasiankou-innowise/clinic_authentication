FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
COPY ./Innowise.Clinic.Auth.Api/NuGet.Config ./
WORKDIR "/src/Innowise.Clinic.Auth.Api"
RUN dotnet build "Innowise.Clinic.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Innowise.Clinic.Auth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Innowise.Clinic.Auth.Api.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
CMD ["dotnet", "BirthDayTrack.dll"]
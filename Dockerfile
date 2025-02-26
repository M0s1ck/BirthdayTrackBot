FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY src/BirthDayTrack/*.csproj src/BirthDayTrack/
COPY src/AccessConfiguration/*.csproj src/AccessConfiguration/

RUN dotnet restore "BirthDayTrack.sln"

COPY src/. ./src/
RUN dotnet publish "BirthDayTrack/BirthDayTrack.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
CMD ["dotnet", "BirthDayTrack.dll"]
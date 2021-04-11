FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ../../Shared/Shared.csproj ./
COPY ../../Worker/Worker.csproj ./
RUN dotnet restore Worker.csproj

COPY ../../Shared ./
COPY ../../Worker ./
RUN dotnet publish Worker.csproj -c Release -o out 

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Worker.dll"]
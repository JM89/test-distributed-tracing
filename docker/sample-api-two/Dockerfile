FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ../../Shared/Shared.csproj ./
COPY ../../SampleApi.Two/SampleApi.Two.csproj ./
RUN dotnet restore SampleApi.Two.csproj

COPY ../../Shared ./
COPY ../../SampleApi.Two ./
RUN dotnet publish SampleApi.Two.csproj -c Release -o out 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SampleApi.Two.dll"]
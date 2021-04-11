FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ../../Shared/Shared.csproj ./
COPY ../../SampleApi.One/SampleApi.One.csproj ./
RUN dotnet restore SampleApi.One.csproj

COPY ../../Shared ./
COPY ../../SampleApi.One ./
RUN dotnet publish SampleApi.One.csproj -c Release -o out 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SampleApi.One.dll"]
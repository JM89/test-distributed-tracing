FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY ../../SampleApi.One/SampleApi.One.csproj ./SampleApi.One/
RUN dotnet restore ./SampleApi.One/SampleApi.One.csproj

COPY ../../Shared ./Shared/
COPY ../../MyLambda ./MyLambda/
COPY ../../SampleApi.One ./SampleApi.One/
RUN dotnet publish ./SampleApi.One/SampleApi.One.csproj -c Release -o out 

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SampleApi.One.dll"]
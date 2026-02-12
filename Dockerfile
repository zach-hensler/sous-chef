FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
#ARG TARGETARCH
COPY . /source
WORKDIR /source
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -o=out sousChef.sln

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /source/out .
ENTRYPOINT ["dotnet", "entrypoint.dll"]
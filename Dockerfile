FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
#ARG TARGETARCH
COPY . /source
WORKDIR /source/src
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish --use-current-runtime --self-contained false -o /app \
#-a ${TARGETARCH/amd64/x64}

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "entrypoint.dll"]
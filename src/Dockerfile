FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers

# Copy everything else and build
COPY ./NuGet.Config /root/.nuget/NuGet/
COPY . ./

RUN dotnet restore -r linux-x64

RUN dotnet publish -r linux-x64 -c Debug -o /app/out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
RUN mv Wiql.CommandLine wiql
RUN chmod +x wiql



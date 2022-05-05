FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /aspnet-core/src/toyiyo.todo.Web.Mvc
COPY . .

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Restore as distinct layers
RUN dotnet restore
RUN libman restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "toyiyo.todo.Web.Mvc.dll"]

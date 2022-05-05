FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app
COPY . .

# Restore as distinct layers
WORKDIR /app/aspnet-core/src/toyiyo.todo.Web.Mvc
# install libman library
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
# restore dependencies
RUN dotnet restore && libman restore

# Build and publish a release
WORKDIR /app
RUN dotnet publish aspnet-core/src/toyiyo.todo.Web.Mvc/toyiyo.todo.Web.Mvc.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "toyiyo.todo.Web.Mvc.dll"]

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app
COPY . .

# install libman library
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli --version 2.1.175
ENV PATH="$PATH:/root/.dotnet/tools"

# restore dependencies
WORKDIR /app/toyiyo.todo.Web.Mvc
RUN dotnet restore
RUN libman restore

# Build and publish a release
WORKDIR /app
RUN dotnet publish toyiyo.todo.Web.Mvc/toyiyo.todo.Web.Mvc.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "toyiyo.todo.Web.Mvc.dll"]
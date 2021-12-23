# Important


# Introduction

This is a template to create **ASP.NET Core MVC / Angular** based startup projects for [ASP.NET Boilerplate](https://aspnetboilerplate.com/Pages/Documents). It has 2 different versions:

1. [ASP.NET Core MVC & jQuery](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Core) (server rendered multi-page application).
2. [ASP.NET Core & Angular](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular) (single page application).
 
User Interface is based on [AdminLTE theme](https://github.com/ColorlibHQ/AdminLTE).
 
# Download


# Screenshots


# Documentation

* [ASP.NET Core MVC & jQuery version.](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Core)
* [ASP.NET Core & Angular  version.](https://aspnetboilerplate.com/Pages/Documents/Zero/Startup-Template-Angular)

# License

[MIT](LICENSE).

# Getting started

## Data Store
* for development, install a local postgres instance, or use [our Heroku free option](https://data.heroku.com/datastores/5d9a293a-04f9-4210-b48c-b483cacc4cc4)
* connecting to heroku looks like `Default": "User ID=[User];Password=[Password];Host=[Host].compute-1.amazonaws.com;Port=5432;Database=[Database];Pooling=true;Pooling=true;SSL Mode=Require;trustservercertificate=true;"`
* `SSL Mode=Require;trustservercertificate=true;` are a must to connect to Heroku
* Save the connection string to your local environment under the name `ToyiyoDb` - this would be your local instance

## known issues

### secrets.json file
* [dotnet 6 made the secrets file mandatory](https://stackoverflow.com/questions/70056612/the-configuration-file-secrets-json-was-not-found-and-is-not-optional-net-6).  
* The project will already have a configured UserSecretsId at the project level and will look for a file to reside there.
* Create a secrets.json file in the path specified on the error message when you try the command `dotnet run`
* If you want to use the secrets functionality - Copy over the info from `appsettings.json` and update the connection string to your local connection string
* The file is meant for local development

![Build and Test](https://github.com/toyiyo/todo/actions/workflows/dotnet.yml/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=toyiyo_todo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=toyiyo_todo)

# Introduction

Hi there, welcome to toyiyo app, your simple project management tool make your day more productive and achieve more.

We are just getting started, so documentation is on the light side at the moment, but we'll fix that soon.  Stay tuned

# License

[MIT](LICENSE).

# Getting started
* Open your solution in Visual Studio 2019 (v16.4)+ and build the solution.
* Select the 'Web.Mvc' project as the startup project.
* Check the connection string in the appsettings.json file of the Web.Mvc project, change it if you want.
* Open Package Manager Console and run the Update-Database command to create your database (ensure that the Default project is selected as .EntityFrameworkCore in the Package Manager Console window).
* Since it uses libman, go to Web.Mvc project. Right click to libman.json file. Then click to Restore Client-Side Libraries.
* (If you are not using Visual Studio and/or you are on a mac you can use Libman CLI . After installing it while in Web.Mvc folder run `libman restore`)
* Run the application.


## Data Store
* for development, install a local postgres instance, or use [our Heroku free option](https://data.heroku.com/datastores/5d9a293a-04f9-4210-b48c-b483cacc4cc4)
* connecting to heroku looks like `Default": "User ID=[User];Password=[Password];Host=[Host].compute-1.amazonaws.com;Port=5432;Database=[Database];Pooling=true;Pooling=true;SSL Mode=Require;trustservercertificate=true;"`
* `SSL Mode=Require;trustservercertificate=true;` are a must to connect to Heroku
* Save the connection string to your local environment under the name `ToyiyoDb` - this would be your local instance

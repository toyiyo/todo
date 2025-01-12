![Build and Test](https://github.com/toyiyo/todo/actions/workflows/dotnet.yml/badge.svg)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=toyiyo_todo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=toyiyo_todo)
[![CodeQL](https://github.com/toyiyo/todo/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/toyiyo/todo/actions/workflows/github-code-scanning/codeql)

# Toyiyo Todo Application

A modern project management tool built with ASP.NET Core and ASP.NET Boilerplate framework.

## 🏗 Architecture Overview

This application follows the best practices of Domain-Driven Design (DDD) through ASP.NET Boilerplate's modular architecture:

### Core Layers

- **Domain Layer**: Contains entities, domain services, and business logic. This layer is responsible for the core functionality of the application and is independent of other layers.
- **Application Layer**: Orchestrates use cases and application flow. It interacts with the domain layer to execute business rules and coordinates tasks such as sending emails and managing transactions.
- **Infrastructure Layer**: Handles data persistence and external services. This layer includes repositories, data access implementations, and integrations with external systems like email services.
- **Web Layer**: Provides the MVC-based user interface. This layer includes controllers, views, and client-side assets.

### Key Features

- **Dependency Injection**: Leverages ASP.NET Core's built-in DI container enhanced by Castle Windsor for managing dependencies.
- **Unit of Work**: Automatic transaction management via ABP's UnitOfWork pattern ensures that all database operations are performed within a transaction.
- **Domain Events**: Built-in event bus for loosely coupled domain events, allowing different parts of the application to communicate without tight coupling.
- **Logging**: Integrated logging with Sentry for error tracking and monitoring.
- **Authentication**: Google OAuth support and built-in user management for secure authentication and authorization.

## 🚀 Getting Started

### Prerequisites

- .NET 6.0 SDK
- PostgreSQL database
- Visual Studio 2019 (v16.4)+ or VS Code

### Quick Start

1. **Clone & Build**:
    ```bash
    git clone https://github.com/toyiyo/todo.git
    cd todo
    dotnet build
    ```
## ⚙️ Configuration Settings

To execute the application when deployed, you need to set up the following configuration settings. These settings can be provided through environment variables or configuration files such as `appsettings.json`.

### Required Configuration Settings

- **DefaultPassword**: The default password for new users.
- **GoogleClientId**: The client ID for Google OAuth authentication.
- **GoogleClientSecret**: The client secret for Google OAuth authentication.
- **ToyiyoDb**: The connection string for the PostgreSQL database.
- **StripeAPIKeyProduction**: The API key for Stripe payment processing (production environment).
- **StripeWebhookSecret**: The webhook secret for Stripe to verify events.
- **SendGridApiKey**: The API key for SendGrid email service.
- **FromTransactionalEmail**: The email address used for sending transactional emails.
- **SenderDisplayName**: The display name for the sender of transactional emails.

### Example Configuration

Here is an example of how to set these configuration settings using environment variables:

```bash
export DefaultPassword="your default password"
export GoogleClientId="[id].apps.googleusercontent.com"
export GoogleClientSecret="[secret]"
export ToyiyoDb="User ID=[user];Password=[password];Host=[host];Port=[port];Database=[toyiyo];Pooling=true;SSL Mode=Require;trustservercertificate=true;"
export StripeAPIKeyProduction="[StripeKey]"
export StripeWebhookSecret="[StripeWebHookSecret]"
export SendGridApiKey="[SendgridAPIKey]"
export FromTransactionalEmail="[ValidatedSenderInSendgrid]"
export SenderDisplayName="[ValidatedSenderNameInSendgrid]"
```

2. **Database Setup**:
    - Ensure PostgreSQL is installed and running.
    - Ensure your connection string to `ToyiyoDb` has been set in as an environment variable
    - Run the database migrations:
      ```bash
      dotnet ef database update -p aspnet-core/src/toyiyo.todo.EntityFrameworkCore
      ```

3. **Run the Application**:
    ```bash
    dotnet run -p aspnet-core/src/toyiyo.todo.Web.Mvc
    ```

4. **Access the Application**:
    - Open your browser and navigate to `https://localhost:5001`.

## 📚 Project Structure

### Domain Layer

- **Entities**: Core business objects (e.g., `UserInvitation`, `Project`).
- **Domain Services**: Business logic and rules (e.g., `UserInvitationManager`, `ProjectManager`).

### Application Layer

- **Application Services**: Orchestrate use cases (e.g., `UserInvitationAppService`, `ProjectAppService`).
- **DTOs**: Data Transfer Objects for communication between layers.

### Infrastructure Layer

- **Repositories**: Data access implementations (e.g., `UserInvitationRepository`, `ProjectRepository`).
- **External Services**: Integrations with external systems (e.g., `SendGridEmailSender`).

### Web Layer

- **Controllers**: Handle HTTP requests and responses (e.g., `UserInvitationController`, `ProjectController`).
- **Views**: Razor views for rendering HTML (e.g., `Index.cshtml`, `Privacy.cshtml`).
- **Client-Side Assets**: JavaScript, CSS, and other static files.

## 🛠 Best Practices

### Separation of Concerns

- **Domain Layer**: Focuses on business logic and rules, independent of other layers.
- **Application Layer**: Coordinates tasks and use cases, interacts with the domain layer.
- **Infrastructure Layer**: Handles data persistence and external integrations.
- **Web Layer**: Provides the user interface and handles HTTP requests.

### Dependency Injection

- **ASP.NET Core DI**: Used throughout the application for managing dependencies.
- **Castle Windsor**: Enhances the built-in DI container for more advanced scenarios.

### Unit of Work

- **Automatic Transaction Management**: Ensures all database operations are performed within a transaction, reducing the risk of data inconsistencies.

### Domain Events

- **Event Bus**: Allows different parts of the application to communicate without tight coupling, promoting a more modular architecture.

### Logging

- **Integrated Logging**: Uses Sentry for error tracking and monitoring, providing insights into application performance and issues.

### Authentication

- **Google OAuth**: Supports secure authentication and authorization using Google OAuth.
- **Built-In User Management**: Provides user management features out of the box.

### Setting Up Canny for User Feedback

To enable user feedback using Canny, follow these steps:

1. **Sign Up for Canny**

   Go to [Canny's website](https://canny.io/) and sign up for an account.

2. **Create a New Board**

   After signing up, create a new board in Canny where users can submit their feedback.

3. **Get Your Canny App ID**

   Once your board is created, you will be provided with an App ID. This App ID is required to integrate Canny with your application.

4. **Set the `CANNY_APP_ID` Environment Variable**

   Set the `CANNY_APP_ID` environment variable to the App ID you obtained from Canny.

   **For macOS/Linux:**

   Add the following line to your `~/.zshrc` or `~/.bashrc` file:

   ```sh
   export CANNY_APP_ID="your-app-id-here"

5. **Verify the configuration**
    Verify the Configuration

    Ensure that the CANNY_APP_ID environment variable is correctly set by running the following command:

    `echo $CANNY_APP_ID`

    You should see your Canny App ID in the output.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgements

- [ASP.NET Boilerplate](https://aspnetboilerplate.com/) for providing the foundation and best practices for building modern web applications.

## 📞 Contact

For any questions or support, please open an issue on the [GitHub repository](https://github.com/toyiyo/todo/issues).
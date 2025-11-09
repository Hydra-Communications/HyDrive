# HyDrive
An open source and self-hostable file storage service.

# Project Structure

This solution follows a **Clean Architecture** approach, separating concerns across different projects:

- **API**  
  The entry point of the application. Handles HTTP requests, routes them to the Application layer, and returns responses. Contains controllers, middleware, and API-specific configurations.

- **Application**  
  Contains the business logic and use cases of the application. Defines services, commands, queries, and interfaces for interacting with other layers. It does **not** depend on Infrastructure or API.

- **Domain**  
  Holds the core entities, value objects, and domain logic. This is the heart of the application and is independent of any external framework or infrastructure.

- **Infrastructure**  
  Implements interfaces defined in the Application layer, such as data access, external services, file storage, or email sending. Responsible for connecting the application to external resources.

- **Tests**  
  Contains automated tests for the solution, including unit, integration, and functional tests. Tests validate the behavior of Domain, Application, and Infrastructure components.


**Running migrations**

We have to run migration commands like this since we split out the projects. I didn't want to copy code over and didn't want to think about how to avoid it, so here we go. Anyone who wants to can try to fix it and I'll merge it in.
```dotnet ef migrations add [MigrationName] --project HyDrive.Infrastructure --startup-project HyDrive.Api
```
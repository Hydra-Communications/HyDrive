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

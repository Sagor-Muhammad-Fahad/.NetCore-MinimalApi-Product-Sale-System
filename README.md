# Product & Sale Management Minimal API

A clean and efficient RESTful Web API built using **.NET Core Minimal API** architecture and **Entity Framework Core**. This project demonstrates Master-Detail data relationships, EF Core migrations, file upload handling, and custom endpoint routing.

### 🚀 Key Features
* **Master-Detail CRUD Operations:** Full HTTP endpoints (GET, POST, PUT, DELETE) handling Products along with their related Sales entities.
* **Complex Data Updating:** Handles cascade updates and deletion of child `Sale` records during a `Product` update.
* **Image File Upload:** Built-in endpoint supporting JPEG/PNG image uploads with strict validation (file size and extension checks).
* **Entity Framework Core:** Uses EF Core Code-First approach with database seeding using `OnModelCreating`.
* **Circular Reference Handling:** Handled using `[JsonIgnore]` attributes and `ReferenceHandler.IgnoreCycles`.
* **CORS & OpenAPI:** Pre-configured CORS policies and Swagger UI documentation for easy integration and API testing.

### 🛠️ Tech Stack
* **Framework:** .NET Core (Minimal API)
* **ORM:** Entity Framework Core
* **Database:** Microsoft SQL Server
* **Documentation:** Swagger / OpenAPI

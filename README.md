# Cinema Booking System

A modern, enterprise-level ASP.NET Core MVC Cinema Booking System built using Clean Architecture and CQRS principles.

## 🚀 Features

### **Admin Area**
- **Dashboard:** Animated statistics and revenue tracking with `Chart.js` and `countUp.js`.
- **Movies Management:** Full CRUD with poster image upload and DataTables.
- **Cinema & Hall Management:** Dynamic grid views and relationship mapping.
- **ShowTime Scheduling:** Advanced date/time pickers and relational drop-downs.
- **Bookings Viewer:** Comprehensive list of customer bookings with visual status indicators.

### **Customer Portal**
- **Premium UI:** Netflix-style glassmorphism interface, dark mode, and smooth `AOS.js` animations.
- **Movie Browsing:** Dynamic `Swiper.js` carousel and detailed movie pages with backdrop posters.
- **Interactive Booking:** Multi-step ticket booking flow with real-time price calculation and interactive seat counters.
- **User Dashboard:** "My Tickets" area featuring realistic digital ticket stubs with QR codes.
- **Authentication:** Secure Identity-based login, registration, and profile management with full-screen cinematic backgrounds.

## 🏗️ Architecture

This project is built using **Clean Architecture** to ensure separation of concerns, testability, and maintainability.

```mermaid
graph TD
    subgraph "Presentation Layer"
        Web[CinemaBooking.Web (ASP.NET Core MVC)]
    end

    subgraph "Application Layer (Core)"
        App[CinemaBooking.Application]
        CQRS[CQRS with MediatR]
        Validation[FluentValidation]
        DTOs[Data Transfer Objects]
        Interfaces[Application Interfaces]
        
        App --> CQRS
        App --> Validation
        App --> DTOs
        App --> Interfaces
    end

    subgraph "Domain Layer (Core)"
        Domain[CinemaBooking.Domain]
        Entities[Entities & Value Objects]
        Exceptions[Domain Exceptions]
        DomainInterfaces[Repository Interfaces]
        
        Domain --> Entities
        Domain --> Exceptions
        Domain --> DomainInterfaces
    end

    subgraph "Infrastructure Layer"
        Infra[CinemaBooking.Infrastructure]
        Identity[ASP.NET Core Identity]
        FileStorage[File Storage Service]
        
        Infra --> Identity
        Infra --> FileStorage
    end

    subgraph "Persistence Layer"
        Persist[CinemaBooking.Persistence]
        EF[Entity Framework Core]
        DbCtx[AppDbContext]
        Repos[Repository Implementations]
        
        Persist --> EF
        Persist --> DbCtx
        Persist --> Repos
    end

    %% Dependencies
    Web -->|Depends on| App
    Web -->|Depends on| Infra
    Web -->|Depends on| Persist
    
    App -->|Depends on| Domain
    
    Infra -->|Implements| Interfaces
    Infra -->|Depends on| Domain
    
    Persist -->|Implements| DomainInterfaces
    Persist -->|Depends on| Domain
    
    %% Cross-cutting
    Shared[CinemaBooking.Shared] -.-> Web
    Shared -.-> App
    Shared -.-> Domain
    Shared -.-> Infra
    Shared -.-> Persist
```

### **Layer Breakdown**
1. **Domain Layer:** Contains all enterprise logic, entities (`Movie`, `Cinema`, `Booking`), and repository interfaces. Has NO dependencies on other layers.
2. **Application Layer:** Contains the business logic, CQRS (`MediatR`) commands/queries, `FluentValidation` validators, and DTOs. Depends only on the Domain layer.
3. **Infrastructure Layer:** Handles external concerns like Identity (User Management) and File Storage (saving uploaded posters).
4. **Persistence Layer:** Handles database access using Entity Framework Core, defining the `AppDbContext` and implementing the repository interfaces.
5. **Web Layer (Presentation):** The ASP.NET Core MVC application. It depends on the Application layer to execute commands/queries and the Persistence/Infrastructure layers for Dependency Injection setup.

## 🛠️ Tech Stack
- **Framework:** .NET 8 / ASP.NET Core MVC
- **Architecture:** Clean Architecture, CQRS (MediatR), Repository & Unit of Work Patterns
- **Database:** Entity Framework Core (SQL Server / SQLite)
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Frontend UI:** Bootstrap 5.3, custom CSS (Glassmorphism), Font Awesome 6
- **Frontend JS:** DataTables, SweetAlert2, AOS (Animate on Scroll), Swiper.js, Chart.js

## ⚙️ Running the Project

1. Ensure you have the .NET SDK installed.
2. Navigate to the Web project directory:
   ```bash
   cd CinemaBooking.Web
   ```
3. Run the application:
   ```bash
   dotnet run --urls "http://localhost:5169"
   ```
4. Open your browser and navigate to `http://localhost:5169`.

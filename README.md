# **🏥 OMAB \- Online Medical Appointment & Booking**

## **📖 Overview**

**OMAB** (Online Medical Appointment & Booking) is a web application designed to streamline the scheduling and management of medical appointments. It bridges the gap between patients and healthcare professionals by providing a seamless, reliable, and secure booking experience.

The core backend [](./OMAB) is engineered with **Domain-Driven Design (DDD)** principles, strictly adhering to **Clean Architecture** and **CQRS**, ensuring the system is scalable, easily maintainable, and loosely coupled.

**⚠️ Project Status & Repository Structure**

- This project is developed primarily for **portfolio and learning purposes**.
- **Status:** Ongoing
- **Structure** [OMAB](./OMAB): The new, refactored backend built with Clean Architecture and CQRS **(This README focuses on this version)**.
    - [Backend](./Backend): The legacy backend version (kept for historical reference/comparison).
    - [frontend-hospital-app](./frontend-hospital-app): The upcoming React frontend (Work in Progress).

## **✨ Role-Based Access Control**

Three distinct user roles:

- 🧑‍⚕️ **Doctor:** Manage availability, view schedules, and update appointment statuses (Diagnosis, Prescriptions).
- 👤 **Patient:** Search for doctors by specialty, book/cancel appointments, view medical history, rating doctors, ....
- 🛡️ **Admin:** Manage user accounts, oversee platform activities, and handle master data (Specialties, Diseases).

## **🗄️ Database Design**

![Database Overview](./docs/database-diagram.png)

## **🛠️ Tech Stack**

**Backend (/OMAB)**

- **Framework:** ASP.NET Core Web API
- **Database:** PostgreSQL (currently Sqlite for testing)
- **Architecture & Patterns:** Clean Architecture, CQRS, Repository Pattern, Dependency Injection (DI).
- **ORM & Tools:** Entity Framework (EF) Core, MediatR, AutoMapper, Swagger/OpenAPI.

**Frontend (/frontend-hospital-app)**: ReactTS, TailwindCSS, TanStack Query

## **🏗️ Architecture Design**

The backend solution is divided into loosely coupled layers to separate the core business logic from infrastructure concerns:

1. **Domain Layer:** Contains Enterprise-wide logic and types (Entities, Enums, Constants). Zero external dependencies.
2. **Application Layer:** Contains Business logic (Use Cases). Implements CQRS (Commands/Queries), MediatR handlers, and define Interfaces.
3. **Infrastructure Layer:** Implements external concerns (EF Core DbContext, PostgreSQL connections, Authentication Services, Repositories).
4. **Presentation (API) Layer:** ASP.NET Core Web API serving as the entry point, handling HTTP requests and responses.

## **🚀 Getting Started**

**1\. Backend Setup**

```bash
cd OMAB/OMAB.Api
dotnet ef database update \--project ../OMAB.Infrastructure \--startup-project .

dotnet run
```

_The API documentation will be available at https://localhost:5049/swagger (or your configured port)._

**2\. Frontend Setup (WIP)**

```bash
cd frontend-hospital-app
npm install
npm run dev
```

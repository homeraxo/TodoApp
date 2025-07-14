# Real-Time Data Synchronization To-Do Application

This project is a To-Do List application that features **real-time data synchronization** across multiple desktop clients. Developed with **.NET Framework**, the application showcases a robust architecture, concurrency handling, and instant UI updates on all connected clients.

---

## Project Objective

The main goal is to build a solution that addresses the following key requirements:

* **Real-time Synchronization:** Changes (add, edit, delete, mark complete/incomplete) made to a task on one client are instantly reflected on all other connected clients, without requiring a refresh.
* **Concurrency Management:** Implementation of an **editing lock mechanism** to prevent simultaneous modifications of the same task by different users.
* **Robust Architecture:** Utilization of proven design patterns and a clear separation of concerns.
* **Performance and Scalability:** Ability to support multiple concurrent clients and rapid task changes.

---

## Technologies Used

### Server

* **.NET Framework (ASP.NET Web API):** For building the backend services and hosting SignalR.
* **SignalR:** An ASP.NET library for adding real-time web functionality to applications. It's used for broadcasting task changes to all connected clients.
* **MS SQL Server:** Relational database management system for task persistence.
* **Entity Framework 6:** An ORM (Object-Relational Mapper) for interacting with the SQL Server database.

### Client

* **WPF (.NET Framework):** For developing the desktop application.
* **MVVM (Model-View-ViewModel):** An architectural pattern for clean separation between the UI, business logic, and data.
* **SignalR Client:** A library for the WPF client to connect to the server's SignalR Hub and receive/send real-time updates.
* **MahApps.Metro:** (Optional, but included) A WPF UI toolkit that provides a modern and fluent design style.

---

## Architecture and Design Patterns

The project follows an N-Tier architecture with a clear separation of responsibilities:

* **Presentation Layer (WPF Client):**
    * Uses the **MVVM pattern** to decouple the user interface (View) from the business logic (ViewModel) and data (Model).
    * **`TaskItemViewModel`**: Represents an individual task in the UI, managing its editing and locked states.
    * **`TaskListViewModel`**: Manages the collection of tasks, addition logic, and interaction with the communication service.
* **Communication Service Layer (WPF Client):**
    * **`SignalRClientService`**: Encapsulates all logic for connecting and communicating with the server's SignalR Hub. It raises events for ViewModels to react to real-time updates.
* **Service Layer (Server - Business Logic):**
    * The **SignalR `TaskHub`** not only acts as a real-time connection point but also contains the business logic for CRUD operations and, crucially, **task locking management**.
    * Concurrency Handling: When attempting to edit or delete a task, its `IsLocked` and `LockedByClient` status are checked to prevent conflicts.
* **Data Access Layer (Server):**
    * **`ApplicationDbContext`**: The Entity Framework context that maps C# entities to database tables.
    * **Repository Pattern (`ITaskRepository`, `TaskRepository`):** Provides an abstraction over the persistence layer, enabling generic CRUD operations and facilitating testing and maintenance.
    * **Unit of Work Pattern (implicit with `DbContext.SaveChanges()`):** Operations are grouped into transactions to ensure atomicity.

### Choice of Communication Protocol: SignalR

**SignalR** was chosen as the primary real-time communication method for several reasons:

* **Abstraction:** It provides a high-level abstraction over real-time transport technologies like WebSockets, Server-Sent Events, and Long Polling, automatically choosing the best compatible method.
* **Ease of Use:** It greatly simplifies the implementation of bi-directional communication between the server and the client.
* **Scalability:** Designed to support a large number of concurrent connections and is compatible with scale-out scenarios.
* **.NET Integration:** It integrates natively with ASP.NET, making its setup and usage within the .NET Framework ecosystem straightforward.

---

## Setup and Execution Guide

Follow these steps to get the application up and running:

### 1. Database Setup (MS SQL Server)

Ensure you have an accessible MS SQL Server instance.

Execute the following SQL script to create the `ToDoAppDB` database and the `Tasks` table:

```sql
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ToDoAppDB')
BEGIN
    CREATE DATABASE ToDoAppDB;
END
GO

USE ToDoAppDB;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE Tasks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        IsCompleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        IsLocked BIT NOT NULL DEFAULT 0,
        LockedByClient NVARCHAR(255) NULL
    );

    CREATE INDEX IX_Tasks_IsCompleted ON Tasks (IsCompleted);
    CREATE INDEX IX_Tasks_Title ON Tasks (Title);
END
GO

# Food Truck Ordering System (ASP.NET Core MVC)

## Project Overview / Proposal

This project is a **location-aware food truck ordering system** built with **ASP.NET Core MVC**.  

The goal is to provide:

- A **customer-facing web app** where customers can:
  - See where the food truck is currently located.
  - Browse a **location-aware menu** that only shows items available for the current schedule.
  - Add items to a Cart and place pickup orders.
  - Track their order status in real time.

- An **admin/staff back-office** where staff can:
  - Manage trucks, locations, and schedules.
  - Manage menu categories and items (including many-to-many relationships).
  - View and process incoming orders through a kitchen queue.
  - Update order statuses (Pending → Accepted → InProgress → Ready → Completed/Cancelled).
  - Manage user roles and access (Customer, Staff, Admin).

The application is built as a **term project** to integrate the main concepts from the course:

- Data modeling with Entity Framework Core.
- ASP.NET Core MVC and Razor views.
- RESTful Web API.
- JavaScript + AJAX for interactivity.
- Bootstrap for responsive UI.
- Authentication + authorization with ASP.NET Core Identity.

---

## Data Model Summary

The data model uses **Entity Framework Core** with SQLite and includes multiple interconnected entities and at least one many-to-many relationship.

Key entities:

- **Truck**
  - Represents a physical food truck.
  - Properties: `Id`, `Name`, `Description`.
  - Relationships: One-to-many with `Schedule`.

- **Location**
  - Represents a stop/location where the truck can park.
  - Properties: `Id`, `Name`, `Address`, `Latitude`, `Longitude`.
  - Relationships: One-to-many with `Schedule`.

- **Schedule**
  - Represents when a given Truck is at a given Location.
  - Properties: `Id`, `TruckId`, `LocationId`, `StartTime`, `EndTime`, `IsActive`.
  - Relationships:
    - Many-to-one with `Truck`.
    - Many-to-one with `Location`.
  - Drives whether ordering is open and which location/menu is currently active.

- **MenuCategory**
  - Logical grouping of menu items (e.g., “Tacos”, “Drinks”).
  - Properties: `Id`, `Name`, `DisplayOrder`.
  - Relationships: One-to-many with `MenuItem`.

- **MenuItem**
  - Individual menu entries (e.g., "Challaw").
  - Properties: `Id`, `Name`, `Description`, `Price`, `IsAvailable`, `MenuCategoryId`.
  - Relationships:
    - Many-to-one with `MenuCategory`.
    - Many-to-many with `Tag` through `MenuItemTag`.

- **Tag**
  - Represents labels/categories for menu items (e.g., "Spicy", "Vegetarian", "Gluten-Free").
  - Properties: `Id`, `Name`, `BadgeColor`, `DisplayOrder`.
  - Relationships:
    - Many-to-many with `MenuItem` through `MenuItemTag`.

- **MenuItemTag** (Join Table)
  - Implements the many-to-many relationship between MenuItem and Tag.
  - Properties: `MenuItemId`, `TagId` (composite primary key).
  - Relationships:
    - Many-to-one with `MenuItem`.
    - Many-to-one with `Tag`.

- **Order**
  - Represents a placed order.
  - Properties: `Id`, `CustomerId` (optional), `ContactName`, `ContactPhone`,
    `CreatedAt`, `PickupEta`, `Status`, `Total`, `CancelReason`.
  - Relationships: One-to-many with `OrderItem`.

- **OrderItem**
  - Line item inside an Order.
  - Properties: `Id`, `OrderId`, `MenuItemId`, `Quantity`, `UnitPrice`, `Notes`.
  - Relationships:
    - Many-to-one with `Order`.
    - Many-to-one with `MenuItem`.

- **ApplicationUser** (Identity)
  - Extends ASP.NET Core Identity user.
  - Used for authenticated customers, staff, and admins.

---

## Main User Stories (Summary)

> Detailed user stories and flows are documented separately in the project docs (e.g. `docs/UserStories.md`). Below is a high-level summary.

### 1. Location-Aware Menu Browsing

- As a **customer**, I can see:
  - The **active truck** and its **current location** (or the next upcoming schedule).
  - A **menu filtered** to items that are available now (`IsAvailable = true`).
- I can **search** menu items by name/description and **filter by category**.
- I can view **item details** including tags like "Spicy" or "Vegetarian". 

### 2. Cart & Checkout

- As a customer, I can:
  - Add items with **selected items and notes** to a Cart.
  - Edit quantities and remove lines.
  - Proceed to **checkout** as a guest or signed-in user.
  - Receive an **estimated pickup time (ETA)** and order confirmation.

### 3. Real-Time Order Tracking

- As a customer, I can:
  - View my order’s status: `Pending → Accepted → InProgress → Ready → Completed` (with `Cancelled/Rejected` as terminal variants).
  - See the status **auto-update** without full page refresh.
  - View pickup instructions when status is `Ready`.

### 4. Scheduling & Location Management

- As staff/admin, I can:
  - Manage **Truck**, **Location**, and **Schedule** entries.
  - Prevent overlapping schedules for a truck.
  - Set a schedule as **Active Now**, automatically deactivating others.

### 5. Menu & Category Management

- As admin, I can:
  - Perform full **CRRUD** (Create, Read, Read-All, Update, Delete) on menu categories and items.
  - Manage `IsAvailable` and pricing.

### 6. Order Operations (Kitchen/Staff)

- As staff, I can:
  - See an **order queue** with summaries and notes.
  - Progress orders through statuses.
  - Edit or cancel orders before preparation, and have those changes reflected on the customer’s tracking view.

### 7. Authentication & Authorization

- As a customer, I can register, sign in, and view **order history**.
- As an admin, I can:
  - Define and assign roles **Customer, Staff, Admin**.
  - Limit access to admin/staff functions via role-based authorization attributes.

### 8. RESTful API

- Provide **public read endpoints** for active schedule and menu.
- Provide **order endpoints** for creating orders and tracking status.
- Provide **admin/staff endpoints** for secure management and status updates.

### 9. Non-Functional & UX

- Use **Bootstrap** and custom CSS for a responsive, accessible UI.
- Follow **WCAG 2.1 AA** guidelines where possible.
- Implement an intuitive navigation system: Home, Menu, Cart, My Orders, Staff/Admin sections.

---

## Tech Stack

**Platform & Framework**

- **.NET Core** / **ASP.NET Core MVC**
- **C#** (server-side code)
- **Razor** (server-side view engine / templating)
- **Entity Framework Core** (ORM for SQLite)
- **ASP.NET Core Identity** (authentication and roles)

**Client-side**

- **JavaScript** (ES6+)
- **AJAX** with `fetch()` for asynchronous operations
- **Bootstrap** for responsive layout and components
- Custom **CSS** for theming

**Database**

- **SQLite** (`FoodTruckDb.db`)
- Automatically created on first run using Code-first approach.

**Development Environment**

- **IDE**: Visual Studio Code
- **Hosting (dev)**: Local ASP.NET Core Kestrel server (`dotnet run`)

---

## How to Run the Application

### Prerequisites

- .NET SDK installed (version required by your project, e.g., .NET 7 or .NET 8).
- No additional database installation required (uses SQLite with file-based database).
- Database is automatically created on first run in the project directory.

## AI Use 

- Used AI to generate image for the logo.
- I had issues with restricting page where only Admin can edit orders.
- I was getting null reference exception in my CartController when adding items.  Asked for help in understanding why this is happening and how to debug it.

## Default Admin Account

- **Email:** newellp1@etsu.edu
- **Password:** Admin123!


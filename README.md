
# 🚀 **StarShip API – ASP.NET Core 8 + EF Core + Identity + JWT + Swagger**

A complete backend service built in **ASP.NET Core 8**, featuring:

* **SQL Server / LocalDB** with **Entity Framework Core**
* **JWT Authentication**
* **Role-based Authorization** (Admin / User)
* **Full CRUD for Starships**
* **User-specific Favorite Starships**
* **Unit Tests using EF InMemory**
* **Swagger UI with JWT Authorization**

This API is built as part of a full-stack Star Wars–themed demonstration project.

---

# 📁 **Project Structure**

```
StarShipApi/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── StarshipController.cs
│   └── FavoriteStarshipController.cs
│
├── Data/
│   ├── AppDbContext.cs
│   ├── Starship.cs
│   ├── FavoriteStarship.cs
│   └── ApplicationUser.cs (optional depending on setup)
│
│
├── Migrations/
│
├── Program.cs
├── appsettings.json
└── StarShipApi.Tests/ (unit test project)
```

---

# 🛠️ **Tech Stack**

* **ASP.NET Core 8**
* **Entity Framework Core 8**
* **Microsoft Identity**
* **JWT Bearer Authentication**
* **Swagger (Swashbuckle)**
* **SQL Server / LocalDB**
* **xUnit + EF InMemory** for Unit Testing

---

# ⚙️ **Environment Setup**

## 1️. Install .NET 8 SDK

[https://dotnet.microsoft.com](https://dotnet.microsoft.com)

## 2️. Restore packages

```
dotnet restore
```

## 3️. Update database (create tables)

```
dotnet ef database update
```

This creates all Identity tables, Starship, and FavoriteStarship tables.

---

# 🔑 **Authentication (JWT)**

The API uses **JWT Bearer Tokens**.

### Add to `appsettings.json`:

```json
"Jwt": {
  "Key": "THIS_IS_A_LONG_RANDOM_32BYTE_MINIMUM_SECRET_KEY",
  "Issuer": "starshipapi",
  "Audience": "starshipapi"
}
```

The key **must be at least 32 bytes**.

---

# 🔐 **Authentication Endpoints**

### **POST /api/Auth/register**

Registers a user
Returns nothing if successful

### **POST /api/Auth/login**

Returns:

```json
{
  "token": "JWT_TOKEN_HERE"
}
```

### **GET /api/Auth/seed-admin**

Seeds an Admin user with role “Admin”.

---

# 👤 **Role-Based Authorization**

### **Admin**

* Can CRUD **Starships** (the main database table)
* Used for system-level data control

### **User**

* Can CRUD **FavoriteStarship**
* Favorites belong to the logged-in user only

In Swagger, you will see:

```csharp
[Authorize(Roles = "Admin")]
```

or:

```csharp
[Authorize]
```

And public endpoints use:

```csharp
[AllowAnonymous]
```

---

# 📡 **Starship Endpoints (Admin Only - Excluding GET)**

### **GET /api/Starship**

List all starships.

### **POST /api/Starship**

Create a new starship.

### **PUT /api/Starship/{id}**

Update a starship.

### **DELETE /api/Starship/{id}**

Delete a starship.

---

# ⭐ **Favorite Starships (User-specific Data)**

### **GET /api/FavoriteStarship**

Returns only favorites belonging to the logged-in user.

### **POST /api/FavoriteStarship**

Create a favorite starship record tied to the current user.

### **PUT /api/FavoriteStarship/{id}**

Update a user’s favorite starship.

### **DELETE /api/FavoriteStarship/{id}**

Delete a user’s favorite starship.

Favorites require authentication, but the user **does not need admin permissions**.

---

# 📘 **Swagger UI**

Run the API:

```
dotnet run
```

Visit:

### 🔗 [http://localhost:5132/swagger](http://localhost:5132/swagger)

You will see the **Authorize 🔒 button** in the top right.

Paste your JWT there to access protected endpoints.

---

# 🧪 **Unit Testing**

Unit tests are located in:

```
StarShipApi.Tests/
```

### Uses:

* **xUnit**
* **EF Core InMemory**

### Install test dependencies:

```
dotnet add StarShipApi.Tests package Microsoft.EntityFrameworkCore.InMemory --version 8
dotnet add StarShipApi.Tests package xunit --version 8
dotnet add StarShipApi.Tests package xunit.runner.visualstudio --version 8
```

### Run tests:

```
dotnet test
```

### ✔ Tests implemented:

1. **Creating a Favorite Starship**
2. **Retrieving favorites by UserId**
3. **Updating a Favorite Starship**
4. **Deleting a Favorite Starship**

These tests verify **EF Core behavior, CRUD logic, filtering, and DB state changes** without any real SQL connection.

---

# 🔄 **CI/CD Workflows**

Two GitHub Actions pipelines keep the project healthy and ready to ship:

* **.NET Build & Test (`.github/workflows/dotnet.yml`)** runs on every push and pull request to `main` to validate restores, builds, and tests.
* **Deploy API (`.github/workflows/deploy.yml`)** runs on pushes to `main` (and via manual dispatch) to produce a release-ready artifact.

## Deploy workflow overview

The deploy workflow performs a release build, executes the full test suite, publishes the application, and uploads a compressed artifact for downstream deployment steps.

1. **Restore → Build → Test** using .NET 8 in Release mode.
2. **Publish** `StarShipApi/StarShipApi.csproj` to a `published/` directory.
3. **Archive & Upload** the publish output as `StarShipApi-release.tar.gz` for deployment.

---


# 🧠 **How EF InMemory Works (Short Explanation)**

* Each test uses:

  ```csharp
  .UseInMemoryDatabase(Guid.NewGuid().ToString())
  ```
* This ensures each test starts with **a clean database**.
* No external services, no SQL needed.
* Fast, isolated, deterministic tests.

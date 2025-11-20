# 🐳 Starship API – Docker Setup & Deployment Guide

This document explains how to run the **StarShipApi** backend inside Docker, seed the SQLite database, and persist data across container restarts.

The backend uses:

* **ASP.NET Core 8**
* **SQLite** (when running inside Docker)
* **Entity Framework Core**
* **Docker multi-stage build**
* **OpenAI Integration**

---

# 📁 Project Structure (Relevant to Docker)

```
StarShipApi/
│
├── Dockerfile
├── entrypoint.sh
├── Program.cs
├── starships.db          (created during migration)
└── ... other files ...
```

---

# 🏗 1. Build the Docker Image

From inside the `StarShipApi` folder:

```bash
docker build -t starshipapi .
```

This does the following:

* Restores dependencies
* Restores EF Tools
* Publishes the application
* Creates a production-ready image
* Applies EF migrations during build
* Copies the SQLite DB (`starships.db`) into the final image

---

# 📦 2. Create a Volume for Database Persistence

SQLite must live inside a **volume** so data survives container restarts:

```bash
docker volume create starshipsdb
```

This volume will store the file:

```
/app/starships.db
```

---

# 🌱 3. Seed the Database (One-Liner)

The backend supports a custom `--seed` flag that:

* Runs migrations
* Fetches starships from SWAPI
* Populates SQLite

Run this:

```powershell
docker run --rm -it -v starshipsdb:/app -e ASPNETCORE_ENVIRONMENT=Production -e OpenAI__Key="your_openai_key" starshipapi dotnet StarShipApi.dll --seed
```

What happens:

* Container starts
* `--seed` triggers seeding logic
* DB is stored in the `starshipsdb` volume
* Container exits after seeding

---

# 🚀 4. Run the API (One-Liner)

Now that the DB is seeded and stored in a volume:

```powershell
docker run -d -p 8080:8080 -v starshipsdb:/app -e OpenAI__Key="your_openai_key" --name starshipapi-container starshipapi
```

The API is now live at:

```
http://localhost:8080/api/starship
http://localhost:8080/api/auth/login
http://localhost:8080/api/ai/ask
```

---

# 🔧 5. Stopping / Removing the Container

Stop the running container:

```bash
docker stop starshipapi-container
```

Delete the container:

```bash
docker rm starshipapi-container
```

Your data is safe because it's in the volume:

```bash
docker volume inspect starshipsdb
```

---

# 🧽 6. Reset Everything (Optional)

Remove the persisted DB:

```bash
docker volume rm starshipsdb
```

Then recreate & reseed as above.

---

# 🧠 7. OpenAI Integration

The backend expects the environment variable:

```
OpenAI__Key=your_key_here
```

Used for:

* `/api/ai/ask` endpoint
* Embedded speaking logic in AiController

---

# 📜 8. Dockerfile Summary

Your Dockerfile:

* Uses **multi-stage builds**
* Installs EF tools
* Applies migrations in build stage
* Publishes a trimmed runtime image
* Copies `starships.db` for SQLite
* Uses `entrypoint.sh` as startup script

---

# 🚦 9. entrypoint.sh Summary

Logic:

```bash
if "--seed":
    run migrations + seeding
else:
    start API normally
```

This makes seeding repeatable and clean.

---

# 🎉 10. Deployment Ready

This Docker build can be deployed to:

* Azure Container Apps
* Azure Web App for Containers
* AWS ECS
* DigitalOcean App Platform
* Kubernetes
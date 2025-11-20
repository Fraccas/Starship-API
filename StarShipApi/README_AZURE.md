# 🚀 **Starship API – Azure Deployment Guide**

This guide documents the **exact, working, stable process** for deploying the Starship API (.NET 8 + Docker + SQLite) to **Azure App Service (Linux)** using **Azure Container Registry (ACR)**.

It avoids problematic CLI flags, avoids identity issues, and uses the **known-good workflow** validated in production.

---

# 📦 **1. Build & Tag Docker Image**

Build your container locally:

```sh
docker build -t starshipapi .
```

Tag it for your ACR:

```sh
docker tag starshipapi starshipreg123.azurecr.io/starshipapi:latest
```

---

# 📤 **2. Push Image to Azure Container Registry**

Login to ACR:

```sh
az acr login --name starshipreg123
```

Push:

```sh
docker push starshipreg123.azurecr.io/starshipapi:latest
```

---

# ☁️ **3. Configure Azure Web App to Use the Container**

> **Important:**
> Before setting the container, always clear the LinuxFxVersion if the App Service was previously misconfigured.

Reset container config:

```sh
az webapp config set --resource-group starship-rg --name starshipapiapp01 --linux-fx-version ""
```

Now set the working container configuration **using admin credentials**
(ACR identity-based pulls are avoided due to CLI incompatibility):

```sh
az webapp config container set --resource-group starship-rg --name starshipapiapp01 --docker-custom-image-name starshipreg123.azurecr.io/starshipapi:latest --docker-registry-server-url https://starshipreg123.azurecr.io --docker-registry-server-user starshipreg123 --docker-registry-server-password "<YOUR_ACR_PASSWORD>"
```

---

# 🔑 **4. How to Get Your ACR Password**

Enable admin user (if not already):

```sh
az acr update --name starshipreg123 --admin-enabled true
```

Retrieve username + password:

```sh
az acr credential show --name starshipreg123
```

You will use:

* **username**: `starshipreg123`
* **password**: `passwords[0].value`

---

# 🗄️ **5. Database: SQLite in Production**

The app uses:

```csharp
options.UseSqlite("Data Source=starships.db");
```

Azure App Service stores this file in:

```
/home/site/wwwroot/starships.db
```

This directory is **write-enabled**, so migrations and data persist across restarts.

---

# 🌱 **6. Production Auto-Migration & Auto-Seeding**

Azure App Service **cannot** pass command-line arguments and **ignores** your `entrypoint.sh`.

Therefore all seeding must occur automatically inside *Program.cs*.

This block handles production seeding:

```csharp
if (app.Environment.IsProduction())
{
    Console.WriteLine("Running production migrations and seed...");

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var swapi = scope.ServiceProvider.GetRequiredService<ISwapiService>();

    context.Database.Migrate();
    await DbInitializer.InitializeAsync(context, swapi);

    Console.WriteLine("Production seed complete.");
}
```

---

# 🔄 **7. Redeploying Updates**

Each update follows the same 3-step cycle:

```sh
docker build -t starshipapi .
docker tag starshipapi starshipreg123.azurecr.io/starshipapi:latest
docker push starshipreg123.azurecr.io/starshipapi:latest
```

Then restart your App Service:

```sh
az webapp restart --resource-group starship-rg --name starshipapiapp01
```

---

# 📡 **8. View Live Logs**

```sh
az webapp log tail --resource-group starship-rg --name starshipapiapp01
```

If logs stop instantly, this usually means **container config is corrupted**.
Fix it with:

```sh
az webapp config set --resource-group starship-rg --name starshipapiapp01 --linux-fx-version ""
```

Then reapply the container settings.

---

# 🛑 **9. Known Issues to Avoid**

### ❌ Do NOT use the following flags (unsupported in your CLI version):

```
--acr-use-managed-identity
--enable-managed-identity
--container-image-name
--container-registry-url
--container-registry-user
```

Using these caused:

* Corrupted LinuxFxVersion
* Silent ACR pull failures
* Infinite `:( Application Error`
* Empty log streams

Stick with:

✔ `--docker-custom-image-name`
✔ `--docker-registry-server-url`
✔ `--docker-registry-server-user`
✔ `--docker-registry-server-password`

---

# 🎉 **10. Success Indicators**

After restart + log tail you should see:

```
ASPNETCORE_ENVIRONMENT = Production
Running production migrations and seed...
Production seed complete.
```

Calling the API:

```
https://<your-site>.azurewebsites.net/starships
```

Should return **seeded Starship data**, not `[]`.


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StarShipApi.Data;
using StarShipApi.Data.Seed;
using StarShipApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Log environment to confirm during Docker runs
Console.WriteLine("ASPNETCORE_ENVIRONMENT = " + builder.Environment.EnvironmentName);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"] ?? throw new Exception("JWT Key is missing");
var key = Encoding.UTF8.GetBytes(keyString);

// ------------------------------------------------------------------------
// DATABASE CONFIG
// ------------------------------------------------------------------------
// Docker → SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});



// ------------------------------------------------------------------------
// KESTREL CONFIG (Docker must listen on 8080 only)
// ------------------------------------------------------------------------
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // DO NOT enable HTTPS inside Docker
});

// ------------------------------------------------------------------------
// CORS
// ------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "http://starshipuiapp01.azurewebsites.net",
            "https://starshipuiapp01.azurewebsites.net"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ------------------------------------------------------------------------
// Controllers + Swagger
// ------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Swagger gains access to endpoints

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "StarShipApi",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ------------------------------------------------------------------------
// HTTP Clients & Identity
// ------------------------------------------------------------------------
builder.Services.AddHttpClient<ISwapiService, SwapiService>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ------------------------------------------------------------------------
// JWT Authentication
// ------------------------------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// If --seed is present, perform migrations & seed, then exit
if (args.Contains("--seed"))
{
    Console.WriteLine("Running seeding...");

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var swapi = scope.ServiceProvider.GetRequiredService<ISwapiService>();

    // apply migrations
    context.Database.Migrate();

    // run custom seeding
    await DbInitializer.InitializeAsync(context, swapi);

    Console.WriteLine("Seeding finished");
    return;
}


// ------------------------------------------------------------------------
// Local development only: auto-seed + migrations on startup
// ------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var swapi = scope.ServiceProvider.GetRequiredService<ISwapiService>();

    context.Database.Migrate();
    await DbInitializer.InitializeAsync(context, swapi);
}

// ------------------------------------------------------------------------
// PIPELINE
// ------------------------------------------------------------------------
app.UseCors("AllowAngular");

app.UseStaticFiles(); // Swagger needs this when running in Docker
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

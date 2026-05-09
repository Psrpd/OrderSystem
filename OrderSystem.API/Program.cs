using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderSystem.Application.Interfaces;
using OrderSystem.Application.Services;
using OrderSystem.Infrastructure.Identity;
using OrderSystem.Infrastructure.Persistence;
using OrderSystem.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================= CONTROLLERS =================
builder.Services.AddControllers();

// ================= SWAGGER =================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order System API",
        Version = "v1"
    });

    // JWT AUTH IN SWAGGER
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    // Add security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ================= DATABASE =================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// ================= IDENTITY =================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ================= JWT AUTH =================
var key = Encoding.UTF8.GetBytes(
    builder.Configuration["Jwt:Key"]
    ?? "SuperSecretKeyForOrderSystem123!"
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(key)
        };
});

// ================= DEPENDENCY INJECTION =================
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var app = builder.Build();

// ================= SWAGGER =================
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Order System API V1"
    );

    options.RoutePrefix = string.Empty;
});

// ================= MIDDLEWARE =================
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// ================= DATABASE SEED =================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<ApplicationDbContext>();

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    await context.Database.EnsureCreatedAsync();

    // CREATE STORED PROCEDURE
    var spPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "OrderSystem.Infrastructure",
        "Persistence",
        "Scripts",
        "sp_ApproveOrder.sql"
    );

    if (File.Exists(spPath))
    {
        var spSql = await File.ReadAllTextAsync(spPath);

        await context.Database.ExecuteSqlRawAsync(spSql);
    }

    await DataSeeder.SeedAsync(context, userManager);
}

app.Run();
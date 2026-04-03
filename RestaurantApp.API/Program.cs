using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RestaurantApp.API.Data;
using MassTransit;
using FluentValidation;
using System.Reflection;
using Microsoft.OpenApi.Models;
// Fix for PostgreSQL DateTime Kind issue
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// =================== SERVICES ===================
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// 1. Database (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Suppress pending model changes warning in dev to avoid 500 on startup
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// 2. Authentication & JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
    // Support JWT in httpOnly cookies
    options.Events = new JwtBearerEvents {
        OnMessageReceived = context => {
            if (context.Request.Cookies.ContainsKey("accessToken")) {
                context.Token = context.Request.Cookies["accessToken"];
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

// 3. Redis Cache
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// 4. MassTransit (RabbitMQ) - Optional, skip if not running
try {
    builder.Services.AddMassTransit(x => {
        x.UsingRabbitMq((context, cfg) => {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h => {
                h.Username(builder.Configuration["RabbitMQ:Username"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
            });
        });
    });
} catch { /* RabbitMQ optional in dev */ }

// 5. === MODULE SERVICES ===
// Auth
builder.Services.AddSingleton<RestaurantApp.API.Modules.Auth.Services.JwtProvider>();
builder.Services.AddScoped<RestaurantApp.API.Modules.Auth.Services.IAuthService,
    RestaurantApp.API.Modules.Auth.Services.AuthService>();

// Restaurant
builder.Services.AddScoped<RestaurantApp.API.Modules.Restaurant.Services.IRestaurantService,
    RestaurantApp.API.Modules.Restaurant.Services.RestaurantService>();

// Branch
builder.Services.AddScoped<RestaurantApp.API.Modules.Branch.Services.IBranchService,
    RestaurantApp.API.Modules.Branch.Services.BranchService>();

// Table
builder.Services.AddScoped<RestaurantApp.API.Modules.Table.Services.ITableService,
    RestaurantApp.API.Modules.Table.Services.TableService>();

// Reservation
builder.Services.AddScoped<RestaurantApp.API.Modules.Reservation.Services.IReservationService,
    RestaurantApp.API.Modules.Reservation.Services.ReservationService>();

// Menu
builder.Services.AddScoped<RestaurantApp.API.Modules.Menu.Services.IMenuService,
    RestaurantApp.API.Modules.Menu.Services.MenuService>();

// Order
builder.Services.AddScoped<RestaurantApp.API.Modules.Order.Services.IOrderService,
    RestaurantApp.API.Modules.Order.Services.OrderService>();

// Payment
builder.Services.AddScoped<RestaurantApp.API.Modules.Payment.Services.IPaymentService,
    RestaurantApp.API.Modules.Payment.Services.PaymentService>();

// Customer & Loyalty
builder.Services.AddScoped<RestaurantApp.API.Modules.Customer.Services.ICustomerService,
    RestaurantApp.API.Modules.Customer.Services.CustomerService>();

// Employee & Shift
builder.Services.AddScoped<RestaurantApp.API.Modules.Employee.Services.IEmployeeService,
    RestaurantApp.API.Modules.Employee.Services.EmployeeService>();

// Inventory & Supplier
builder.Services.AddScoped<RestaurantApp.API.Modules.Inventory.Services.IInventoryService,
    RestaurantApp.API.Modules.Inventory.Services.InventoryService>();

// Promotion & Voucher
builder.Services.AddScoped<RestaurantApp.API.Modules.Promotion.Services.IPromotionService,
    RestaurantApp.API.Modules.Promotion.Services.PromotionService>();

// 6. AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// 7. FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// 8. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 9. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Restaurant Management System API",
        Version = "v1",
        Description = "API quan ly nha hang - RMS"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header. Example: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// 10. CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =================== PIPELINE ===================
var app = builder.Build();

// Auto migrate database & seed default data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbInitializer.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RMS API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();

app.Run();

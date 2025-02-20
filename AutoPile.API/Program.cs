using AutoPile.API;
using AutoPile.API.Validators;
using AutoPile.DATA.Cache;
using AutoPile.DATA.Cache.CacheRepository;
using AutoPile.DATA.Data;
using AutoPile.DATA.Middlewares;
using AutoPile.DOMAIN.Interface;
using AutoPile.DOMAIN.Models;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using AutoPile.SERVICE.Utilities;
using Azure.Storage.Queues;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Resend;
using Stripe;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Enhanced logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddAzureWebAppDiagnostics();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("AutoPile", LogLevel.Trace);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            }, new List<string>()
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("ISSUER") ?? builder.Configuration["Jwt:Issuer"],
        ValidAudience = Environment.GetEnvironmentVariable("AUDIENCE") ?? builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTKEY") ?? builder.Configuration["Jwt:Key"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["AuthToken"];
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireClaim("scope", "admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireClaim("scope", "user"));
});
builder.Services.AddScoped<IShoppingCartItemService, ShoppingCartItemService>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, AutoPile.SERVICE.Services.ReviewService>();
builder.Services.AddScoped<IBlobService, BlobService>();
builder.Services.AddScoped<IProductService, AutoPile.SERVICE.Services.ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRedisShoppingCartCache, RedisShoppingCartCache>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped(typeof(IRedisCache<>), typeof(RedisCache<>));
builder.Services.AddScoped<IProductCache, ProductCache>();
builder.Services.AddScoped<IReviewsCache, ReviewsCache>();
builder.Services.AddScoped<IUserInfoCache, UserInfoCache>();
builder.Services.AddScoped<IOrderCache, OrderCache>();
builder.Services.AddScoped<IStripeService, StripeService>();

// Register queue clients as singletons
builder.Services.AddSingleton<QueueClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<QueueClient>>();
    var blobConnectionString = Environment.GetEnvironmentVariable("BlobStorage");

    if (string.IsNullOrEmpty(blobConnectionString))
    {
        logger.LogError("BlobStorage connection string is empty");
        throw new InvalidOperationException("BlobStorage environment variable is not set");
    }

    logger.LogInformation("Creating email queue client");
    return new QueueClient(blobConnectionString, "email-queue",
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
});

builder.Services.AddSingleton<QueueClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<QueueClient>>();
    var blobConnectionString = Environment.GetEnvironmentVariable("BlobStorage");

    if (string.IsNullOrEmpty(blobConnectionString))
    {
        logger.LogError("BlobStorage connection string is empty");
        throw new InvalidOperationException("BlobStorage environment variable is not set");
    }

    logger.LogInformation("Creating inventory queue client");
    return new QueueClient(blobConnectionString, "inventory-queue",
        new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
});
// Add this to the service configuration section in Program.cs
// Register both QueueClients
// Register QueueClient for Email Queue
builder.Services.AddScoped<IEmailQueueService, EmailQueueService>();
builder.Services.AddScoped<IInventoryQueueService, InventoryQueueService>();

// Register background services
builder.Services.AddHostedService<EmailProcessingService>();
builder.Services.AddHostedService<InventoryProcessingService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UserSignupDTOValidator>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("REDIS");
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;              // MinimumLength(8)
    options.Password.RequireUppercase = true;         // Matches("[A-Z]")
    options.Password.RequireLowercase = true;         // Matches("[a-z]")
    options.Password.RequireDigit = true;             // Matches("[0-9]")
    options.Password.RequireNonAlphanumeric = true;   // Matches("[^a-zA-Z0-9]")
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext
builder.Services.AddDbContext<AutoPileManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

var mongoConnectionString = Environment.GetEnvironmentVariable("MongoDB") ??
                          Environment.GetEnvironmentVariable("MongoDB_ConnectionStrings") ??
                          builder.Configuration.GetSection("MongoDb:ConnectionStrings").Value;
var mongoDbName = "AutoPileDb";

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<IMongoClient>>();
    logger.LogInformation("Creating MongoClient with connection string");
    if (string.IsNullOrEmpty(mongoConnectionString))
    {
        logger.LogError("MongoDB connection string is empty");
        throw new InvalidOperationException("MongoDB connection string is not configured");
    }
    return new MongoClient(mongoConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AutoPileMongoDbContext>>();
    try
    {
        var client = sp.GetRequiredService<IMongoClient>();
        logger.LogInformation("Getting MongoDB database: {DatabaseName}", mongoDbName);
        var database = client.GetDatabase(mongoDbName);
        logger.LogInformation("Creating MongoDB context");
        return AutoPileMongoDbContext.Create(database);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to create MongoDB context");
        throw;
    }
});

//builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
//.AddEntityFrameworkStores<AutoPileManagementDbContext>()
//.AddDefaultTokenProviders();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<AutoPileManagementDbContext>()
    .AddSignInManager()
    .AddRoleManager<RoleManager<ApplicationRole>>()
    .AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    var token = Environment.GetEnvironmentVariable("RESEND_APITOKEN");
    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("WARNING: RESEND_APITOKEN environment variable is not set!");
    }

    o.ApiToken = token!;
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "https://www.autopile.store", "https://autopile-gafnbva6egabe5ap.australiaeast-01.azurewebsites.net")
                   .AllowAnyHeader()
                   .AllowAnyMethod().AllowCredentials();
        });
});
StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeKey");

var app = builder.Build();

// Get logger for logging app startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("====== Application Starting - {Time} ======", DateTime.UtcNow);

    // Log environment variables (excluding sensitive values)
    logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
    logger.LogInformation("Checking if key environment variables exist:");
    logger.LogInformation("JWTKEY exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWTKEY")));
    logger.LogInformation("MongoDB exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MongoDB")));
    logger.LogInformation("BlobStorage exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BlobStorage")));
    logger.LogInformation("REDIS exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDIS")));
    logger.LogInformation("StripeKey exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("StripeKey")));
    logger.LogInformation("RESEND_APITOKEN exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RESEND_APITOKEN")));
    logger.LogInformation("ISSUER exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ISSUER")));
    logger.LogInformation("AUDIENCE exists: {Exists}", !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AUDIENCE")));

    // Log middleware configuration
    logger.LogInformation("Core services registered, initializing middleware...");

    logger.LogInformation("Configuring middlewares - Start");

    app.UseMiddleware<UserIdExtractMiddleware>();
    logger.LogInformation("Configured UserIdExtractMiddleware");

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    logger.LogInformation("Configured ExceptionHandlingMiddleware");

    app.UseHsts();
    logger.LogInformation("Configured HSTS");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
        logger.LogInformation("Configured development specific settings");
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoPile API V1");
        c.RoutePrefix = string.Empty;
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
    });
    logger.LogInformation("Configured Swagger");

    app.UseHttpsRedirection();
    logger.LogInformation("Configured HTTPS redirection");

    app.UseRouting();
    logger.LogInformation("Configured routing");

    app.UseCors("AllowLocalhost3000");
    logger.LogInformation("Configured CORS");

    app.UseAuthentication();
    logger.LogInformation("Configured authentication");

    app.UseAuthorization();
    logger.LogInformation("Configured authorization");

    app.MapControllers();
    logger.LogInformation("Configured controller mapping");

    // Initialize roles
    using (var scope = app.Services.CreateScope())
    {
        logger.LogInformation("Checking and initializing roles...");
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    logger.LogInformation("Creating role: {Role}", role);
                    var newRole = new ApplicationRole
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    };
                    await roleManager.CreateAsync(newRole);
                }
                else
                {
                    logger.LogInformation("Role already exists: {Role}", role);
                }
            }
            logger.LogInformation("Role check completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during role initialization");
            throw;
        }
    }

    logger.LogInformation("====== Application configuration complete, starting to run ======");
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "====== APPLICATION STARTUP FAILED ======");
    throw;
}
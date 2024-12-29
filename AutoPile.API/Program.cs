using AutoPile.API;
using AutoPile.API.Validators;
using AutoPile.DATA.Data;
using AutoPile.DATA.Middlewares;
using AutoPile.DOMAIN.Models;
using AutoPile.DOMAIN.Models.Entities;
using AutoPile.SERVICE.Services;
using AutoPile.SERVICE.Utilities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Resend;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UserSignupDTOValidator>();

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

var mongoConnectionString = builder.Configuration.GetSection("MongoDb:ConnectionStrings").Value;
var mongoDbName = builder.Configuration.GetSection("MongoDb:DatabaseName").Value;

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(mongoConnectionString));

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(mongoDbName);
    return AutoPileMongoDbContext.Create(database);
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
            builder.WithOrigins("http://localhost:3000")
            .WithOrigins("https://www.autopile.store/")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

var app = builder.Build();
app.UseCors("AllowLocalhost3000");
app.UseMiddleware<UserIdExtractMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHsts();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var roles = new[] { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var newRole = new ApplicationRole
            {
                Name = role,
                NormalizedName = role.ToUpper()
            };
            await roleManager.CreateAsync(newRole);
        }
    }
}
app.Run();
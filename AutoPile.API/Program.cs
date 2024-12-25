using AutoPile.API.Mapping;
using AutoPile.DATA.Data;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    c =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext
builder.Services.AddDbContext<AutoPileManagementDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
.AddEntityFrameworkStores<AutoPileManagementDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
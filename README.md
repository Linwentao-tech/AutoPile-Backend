# 🚗 AutoPile API

An e-commerce backend system for automotive parts and accessories, built with .NET 9 and deployed on Azure.

## 🌐 Live Demo

The API is deployed and accessible at:
https://autopile-gafnbva6egabe5ap.australiaeast-01.azurewebsites.net/index.html

⚙️ **Frontend Repository**: [AutoPile Frontend](https://github.com/Linwentao-tech/Autopile-Frontend)

## 🏗️ Architecture

The solution follows a clean architecture pattern with four main projects:

- **AutoPile.API** - API endpoints and controllers
- **AutoPile.SERVICE** - Business logic and service implementations
- **AutoPile.DOMAIN** - Domain models, DTOs, and interfaces
- **AutoPile.DATA** - Data access, entity configurations, and caching
- **AutoPile.UnitTests** - Unit Tests with xUnit and Moq

## ✨ Key Features

- 🔐 JWT-based authentication and authorization
- 📧 Email verification and password reset functionality
- 🛒 Shopping cart management with Redis caching
- 💳 Stripe payment integration
- 📦 Order processing and inventory management
- ⭐ Product reviews and ratings system
- 🖼️ Azure Blob Storage for image uploads
- 📊 MongoDB for product catalog
- 🗄️ SQL Server for user and order management

## 🛠️ Technologies

- .NET 9
- Entity Framework Core
- MongoDB
- Redis Cache
- Azure Services:
  - App Service
  - Blob Storage
  - Queue Storage
  - SQL Database
- Stripe Payment Gateway
- Resend Email Service
- JWT Authentication
- AutoMapper
- FluentValidation

## 💾 Storage Solutions

- **SQL Server**: User accounts, orders, shopping carts
- **MongoDB**: Products, reviews
- **Redis**: Caching for products, shopping carts, reviews
- **Azure Blob Storage**: Image storage for product reviews

## 🔄 Background Services with Message Queue

- **EmailProcessingService**: Handles email notifications using Azure Queue
- **InventoryProcessingService**: Manages product inventory updates using Azure Queue

## 🔒 Security Features

- JWT token authentication
- Role-based authorization (Admin/User)
- Email verification
- Secure password reset
- Request validation using FluentValidation
- HTTPS enforcement
- CORS configuration

## 📝 API Documentation

Full API documentation is available through Swagger UI at the root endpoint of the deployed application.

## ⚡ Performance Features

- Redis caching for frequently accessed data
- Async/await patterns throughout
- Efficient database querying
- Background job processing


## 🔧 Environment Variables

The application requires the following environment variables:

```
JWTKEY - JWT signing key
ISSUER - JWT issuer
AUDIENCE - JWT audience
REDIS - Redis connection string
MongoDB - MongoDB connection string
BlobStorage - Azure Blob Storage connection string
RESEND_APITOKEN - Resend API token
StripeKey - Stripe API key
```

## 🚀 Deployment

The application is deployed to Azure Web App using GitHub Actions for continuous integration and deployment (CI/CD). The deployment process is fully automated and triggered on pushes to the master branch.

## 📦 Azure Resources Required

- Azure App Service
- Azure SQL Database
- Azure Cache for Redis
- Azure Blob Storage
- Azure Queue Storage



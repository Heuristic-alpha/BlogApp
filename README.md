# BlogApp 📝

A complete, full-featured web server for creating, editing, sharing, and managing personal blogs using **ASP.NET Core** with a modern, multi-layered architecture.

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)

---

## 🎯 Overview

BlogApp is an enterprise-grade blogging platform that combines multiple Microsoft web technologies to provide a robust, scalable, and user-friendly blogging solution. Whether you're a developer looking for a blogging backend or an end-user wanting to start your blog journey, BlogApp provides everything you need.

---

## ✨ Key Features

### 📱 Multi-Frontend Support
- **ASP.NET Core MVC** - Traditional server-side rendered application
- **Razor Pages** - Lightweight, page-focused approach for rapid development
- **Blazor WebAssembly** - Client-side SPA with full interactive capabilities

### 🔐 Security & Authentication
- **Microsoft Identity Platform** - Enterprise-grade identity management
- **JWT Authentication** - Stateless API authentication with JWT tokens
- **Cookie-based Authentication** - Traditional session management
- **Role-based Authorization** - Fine-grained access control
- **CORS & Rate Limiting** - API protection against abuse

### 📊 Data Management
- **Entity Framework Core 8.0** - ORM with LINQ support
- **SQL Server** - Relational database with dedicated Identity and Data contexts
- **Database Migrations** - Version control for database schema
- **Dual Database Context** - Separation of identity and business data

### 👤 User Management
- **User Registration & Authentication** - Secure user account creation and login
- **Profile Management** - User profiles with customizable information
- **User Profile Pictures** - Dynamic image upload and management
- **Session Management** - Distributed memory caching with session support

### 📝 Blog Features
- **Blog Creation & Editing** - Full CRUD operations for blog posts
- **Rich Content Support** - Support for formatted blog content
- **Blog Sharing** - Share and discover blogs from other users
- **Dynamic Content** - Organized image storage for blog content

### ⚙️ Developer Features
- **RESTful API** - Complete API endpoints for programmatic access
- **Swagger/OpenAPI Documentation** - Interactive API documentation (Development mode)
- **Dependency Injection** - Built-in service container
- **Scoped Services** - Application-level services for URL location, user management, and file handling
- **Middleware Pipeline** - Comprehensive request/response handling

### 🔧 Performance & Scalability
- **Rate Limiting** - Global rate limiter with configurable limits (45 requests per 60 seconds)
- **Distributed Memory Cache** - Session caching and distributed state management
- **Static File Optimization** - Efficient serving of static assets

---

## 🏗️ Project Architecture

### Solution Structure

```
BlogApp/
├── BlogApp                  # Main ASP.NET Core MVC + Razor Pages application
├── BlogApp.Shared          # Shared models and utilities across all projects
├── BlazorAdminPanel        # Blazor WebAssembly admin panel (SPA)
└── BlogApp.sln             # Solution file
```

### Project Details

#### **BlogApp** (Main Application)
- **Framework**: ASP.NET Core 8.0 Web SDK
- **Key Components**:
  - MVC Controllers for traditional request handling
  - Razor Pages for server-side rendered pages
  - Identity integration with custom user model
  - API endpoints with JWT authentication
  - Static file serving with dynamic image management
  - Tag helpers for enhanced HTML rendering

#### **BlogApp.Shared** (Shared Library)
- **Framework**: .NET 8.0 Class Library
- **Purpose**: Common models, DTOs, and business logic shared between backend and frontend
- **Dependencies**: ASP.NET Core Identity for shared user entities

#### **BlazorAdminPanel** (WebAssembly Frontend)
- **Framework**: Blazor WebAssembly 8.0
- **Features**:
  - Client-side SPA administration interface
  - Server-side authentication with JWT
  - Component-based UI architecture
  - Static base path: `/webassembly/`

---

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** - Local SQL Server instance or SQL Server Express
- **Visual Studio 2022** (Recommended) or Visual Studio Code with C# Dev Kit

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Heuristic-alpha/BlogApp.git
   cd BlogApp
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure connection strings**
   
   Update `appsettings.json` in the `BlogApp` project:
   ```json
   {
     "ConnectionStrings": {
       "DataDbConnection": "Server=.;Database=BlogAppData;Trusted_Connection=true;TrustServerCertificate=true;",
       "IdentityDbConnection": "Server=.;Database=BlogAppIdentity;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

4. **Create and seed databases**
   ```bash
   cd BlogApp
   dotnet ef database update --context DataDbContext
   dotnet ef database update --context IdentityContext
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   - Main application: `https://localhost:5001`
   - Swagger API docs (dev only): `https://localhost:5001/swagger`
   - Blazor WebAssembly admin: `https://localhost:5001/webassembly`

---

## 🔑 Authentication & Authorization

### Login Methods

The application supports multiple authentication methods:

1. **Cookie Authentication** - Traditional form-based login
   - Default scheme for browser-based access
   - Automatic redirect for unauthorized requests

2. **JWT Bearer Authentication** - For API clients
   - Token stored in Authorization header or cookies
   - Stateless validation using HS256
   - Configurable token expiration

### Identity Requirements

- **Email**: Must be unique
- **Password**: Minimum 6 characters (customizable)
- **No additional complexity requirements** by default

---

## 📡 API Usage

### Basic Authentication Flow

```bash
# 1. Register a new user
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "securepass123"
}

# 2. Login
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "securepass123"
}

# Response includes JWT token
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}

# 3. Use token in subsequent requests
GET /api/blogs
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Rate Limiting

- **Limit**: 45 requests per 60 seconds (per IP or API key)
- **Response**: HTTP 429 Too Many Requests
- **Header**: Use `X-API-Key` header for higher limits

---

## 🛠️ Development

### Build & Test

```bash
# Build the solution
dotnet build

# Run in development mode
dotnet run --environment Development

# Build release version
dotnet build --configuration Release
```

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName --context DataDbContext

# Apply pending migrations
dotnet ef database update --context DataDbContext

# View migration SQL
dotnet ef migrations script --context DataDbContext
```

### Project Dependencies

**BlogApp Project**
- Microsoft.AspNetCore.Authentication.JwtBearer (v8.0.22)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore (v8.0.22)
- Microsoft.EntityFrameworkCore.SqlServer (v8.0.22)
- Swashbuckle.AspNetCore (v8.0.0)
- System.IdentityModel.Tokens.Jwt (v8.17.0)

**BlazorAdminPanel Project**
- Microsoft.AspNetCore.Components.WebAssembly (v8.0.22)
- Microsoft.AspNetCore.Components.WebAssembly.Authentication (v8.0.22)

---

## 📁 Directory Structure

```
BlogApp/
├── Controllers/              # MVC Controllers & API endpoints
├── Pages/                    # Razor Pages
├── Components/               # Blazor Server & WebAssembly components
├── Models/                   # Data models and DbContexts
├── Services/                 # Business logic services
├── Infrastructures/          # Infrastructure utilities & TagHelpers
├── Migrations/               # Database migrations
├── wwwroot/
│   ├── css/                 # Stylesheets
│   ├── js/                  # JavaScript files
│   ├── images/
│   │   └── dynamic/         # User-uploaded content
│   │       └── userPictures/ # User profile pictures
│   └── ...
├── appsettings.json         # Configuration file
├── Program.cs               # Application startup
└── BlogApp.csproj           # Project file

BlazorAdminPanel/
├── Pages/                    # Blazor pages
├── Components/               # Blazor components
├── Services/                 # Frontend services
├── wwwroot/
│   ├── index.html           # Entry point
│   ├── css/                 # Styles
│   └── js/                  # JavaScript
├── App.razor                # Root component
└── BlazorAdminPanel.csproj
```

---

## 🔐 Security Considerations

- **Sensitive Data Logging**: Enabled only in development mode
- **HTTPS Enforcement**: Can be configured in production
- **JWT Secret**: Store securely in user secrets or environment variables
- **ANTIFORGERY**: Built-in ANTIFORGERY token validation
- **Rate Limiting**: Protects against abuse and DDoS attacks

---

## 📝 Configuration

Key configuration options in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DataDbConnection": "...",
    "IdentityDbConnection": "..."
  },
  "JwtAuthentication": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "your-app",
    "Audience": "your-app-users"
  }
}
```

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

Copyright © 2026 Heuristic-alpha

---

## 📞 Support & Contact

For issues, feature requests, or questions:
- Open an issue on [GitHub Issues](https://github.com/Heuristic-alpha/BlogApp/issues)
- Check existing discussions in the [GitHub Wiki](https://github.com/Heuristic-alpha/BlogApp/wiki)

---

## 🔄 Roadmap

Future enhancements planned:
- [ ] Blog search and filtering
- [ ] Comment system
- [ ] Like/reaction system
- [ ] Social media integration
- [ ] Analytics dashboard
- [ ] SEO optimization
- [ ] Multi-language support
- [ ] Mobile app

---

## 📚 Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | SQL Server | 2019+ |
| Frontend | Blazor (Server & WASM) | 8.0 |
| UI Rendering | Razor | 8.0 |
| Authentication | ASP.NET Identity | 8.0 |
| API Docs | Swagger/OpenAPI | 3.0 |
| Language | C# | Latest |

---

**Last Updated**: May 31, 2026  
**Maintainer**: [@Heuristic-alpha](https://github.com/Heuristic-alpha)

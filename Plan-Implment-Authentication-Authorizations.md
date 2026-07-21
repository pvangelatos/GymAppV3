# 🎯 Implement ASP.NET Core Identity Authentication with Role-Based Authorization

## Context
The GymAppV3 application is a .NET 10 ASP.NET Core Web API using minimal APIs with no current authentication/authorization. The application has Member and Trainer domain models with UserId references and an IUserContext abstraction that needs implementation.

## Authentication Design
- **Provider**: ASP.NET Core Identity with local database (username/password)
- **Roles**: Member, Trainer, Admin, TrainerAdmin (4 separate roles)
- **Authorization Policies**:
  - MemberOnly: Requires Member role
  - TrainerOnly: Requires Trainer role
  - AdminOnly: Requires Admin role
  - TrainerAdminOnly: Requires TrainerAdmin role
  - AnyAuthenticated: Any authenticated user
- **User Registration**: Self-registration as Member, admin-only registration for Trainer/Admin/TrainerAdmin

## Technical Decisions
1. Use ASP.NET Core Identity with IdentityUser/IdentityRole (no custom user entity initially)
2. ApplicationDbContext will inherit from IdentityDbContext<IdentityUser>
3. Implement UserContext from IUserContext to access HttpContext claims
4. Use cookie authentication for web + JWT Bearer for API clients (dual scheme)
5. Follow project conventions: separate endpoint/handler files, command/query pattern
6. Existing endpoints: Admin-only for CUD operations, authenticated users for Read operations

## Files to Modify
- `src/GymAppV3.Infrastructure/Data/ApplicationDbContext.cs` - Inherit from IdentityDbContext
- `GymAppv3.Server/Configuration/ApplicationConfiguration.cs` - Register Identity services
- `GymAppv3.Server/Program.cs` - Add authentication/authorization middleware
- `GymAppv3.Server/Endpoints/*/*.cs` - Add authorization to existing endpoints

## Files to Create
- `src/GymAppV3.Infrastructure/Identity/UserContext.cs` - IUserContext implementation
- `src/GymAppV3.Infrastructure/Identity/RoleConstants.cs` - Role name constants
- `GymAppv3.Server/Configuration/IdentityConfiguration.cs` - Identity setup extension methods
- `GymAppv3.Server/Endpoints/Auth/AuthEndpoints.cs` - Authentication endpoint registration
- `GymAppv3.Server/Endpoints/Auth/AuthHandlers.cs` - Login, register, logout handlers
- `GymAppv3.Server/Endpoints/Auth/RegisterRequest.cs` - Registration request models
- `GymAppv3.Server/Endpoints/Auth/LoginRequest.cs` - Login request models

**Progress**: 100% [██████████]

**Last Updated**: 2026-07-21 19:36:33

## 📝 Plan Steps
- ✅ **Add NuGet packages to GymAppv3.Server project**
- ✅ **Create RoleConstants class in Infrastructure**
- ✅ **Update ApplicationDbContext to use Identity**
- ✅ **Create EF Core migration for Identity schema**
- ✅ **Create UserContext implementation**
- ✅ **Create IdentityConfiguration extension class**
- ✅ **Create authentication request/response models**
- ✅ **Create authentication handlers**
- ✅ **Create authentication endpoints**
- ✅ **Update ApplicationConfiguration to register services**
- ✅ **Update Program.cs middleware pipeline**
- ✅ **Add authorization to existing endpoints**
- ✅ **Create role seeding (optional but recommended)**
- ✅ **Register Auth endpoints in Program.cs**
- ✅ **Test authentication flow**


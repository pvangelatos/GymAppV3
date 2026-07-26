# Gym Web App - Razor Pages Frontend

## Overview
A complete ASP.NET Core Razor Pages web application for gym management, providing role-based interfaces for members, staff, trainers, and administrators.

## Architecture
- **No API Layer**: Directly injects and calls Core/Infrastructure services
- **Shared Database**: Uses the same `ApplicationDbContext` as the server project
- **Role-Based Authorization**: Member, Trainer, Staff (Trainer/TrainerAdmin), and Admin policies
- **Command/Query Pattern**: Follows CQRS principles with immutable DTOs and commands

## Features by Role

### Members
- **Dashboard** (`/Members/Dashboard`): Overview of profile, active bookings, and memberships
- **Profile Management**:
  - Complete profile after registration (`/Members/CompleteProfile`)
  - View profile details (`/Members/Profile`)
  - Edit profile information (`/Members/EditProfile`)
- **Class Browsing & Booking**:
  - Browse class schedule with filtering (`/Classes/Schedule`)
  - View class details and book (`/Classes/Details`)
- **Booking Management**:
  - View all bookings with pagination (`/Members/Bookings/Index`)
  - Cancel confirmed future bookings (`/Members/Bookings/Cancel`)
- **Membership Management**:
  - Browse packages (`/Memberships/Packages`)
  - Purchase memberships (`/Memberships/Purchase`)
  - View active/expired memberships (`/Members/Memberships/Index`)

### Staff (Trainers & TrainerAdmins)
- **Member Management** (`/Staff/Members/`):
  - List all members with pagination
  - View member details including medical notes
  - Create new member records
  - Edit existing member profiles
- **Booking Oversight** (`/Staff/Bookings/Index`):
  - View members with active bookings
- **Class Session Management** (`/Staff/Classes/`):
  - List upcoming sessions with availability
  - View session details with occupancy statistics

### Administrators (Admin & TrainerAdmin)
- **Trainer Management** (`/Admin/Trainers/`):
  - List all trainers with specialties
  - View trainer details
  - Create new trainers (generates temporary password)
  - Edit trainer profiles and specialties
- **Class Session Scheduling** (`/Staff/Classes/Schedule`):
  - Schedule new class sessions
  - Select category, trainer, room, date/time, capacity
- **Membership Package Management** (`/Admin/MembershipPackages/`):
  - List all packages with pricing and details
  - Create new membership packages
  - Edit existing packages
- **Class Category Management** (`/Admin/ClassCategories/`):
  - List all class types
  - Create new categories
  - Edit category names
- **Room Management** (`/Admin/Rooms/`):
  - List all rooms with capacity and building
  - Create new rooms
  - Edit room name and capacity
- **Building Management** (`/Admin/Buildings/`):
  - List all gym locations with addresses
  - Create new buildings
  - Edit building details and contact info

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GymAppV3;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "DefaultAdmin": {
	"Email": "admin@gymapp.com",
	"Password": "Admin@123"
  }
}
```

### Role Seeding
On startup, the app automatically:
1. Creates roles: Member, Trainer, Admin, TrainerAdmin
2. Creates default admin user (if not exists)
3. Assigns Admin and TrainerAdmin roles to default admin

## Service Registration
All Core/Infrastructure services are registered in `Program.cs`:
- `IMemberQueryService` & `IMemberCommandService`
- `IClassSessionQueryService` & `IClassSessionCommandService`
- `IBookingQueryService` & `IBookingCommandService`
- `IMembershipQueryService` & `IMembershipCommandService`
- `IMembershipPackageQueryService`
- `ITrainerQueryService` & `ITrainerCommandService`
- `IClassCategoryQueryService`
- `IClassRoomQueryService`
- `IGymBuildingQueryService`
- `IAuthService`
- `IUserContext`, `IDateTimeProvider`, `IVatRateProvider`

Services with both query and command interfaces use `AddScopedShared<>` to ensure a single instance per request.

## Authorization Policies
- **MemberOnly**: Requires `Member` role
- **TrainerOnly**: Requires `Trainer` or `TrainerAdmin` role
- **StaffOnly**: Requires `Trainer`, `TrainerAdmin`, or `Admin` role
- **AdminOnly**: Requires `Admin` or `TrainerAdmin` role

## Pages Structure
```
Pages/
├── Index.cshtml                    # Landing page with role-based quick links
├── Shared/
│   └── _Layout.cshtml              # Role-aware navigation
├── Members/
│   ├── Dashboard.cshtml            # Member overview
│   ├── CompleteProfile.cshtml      # Initial profile setup
│   ├── Profile.cshtml              # View profile
│   ├── EditProfile.cshtml          # Edit profile
│   ├── Bookings/
│   │   ├── Index.cshtml            # List bookings
│   │   └── Cancel.cshtml           # Cancel confirmation
│   └── Memberships/
│       └── Index.cshtml            # List memberships
├── Classes/
│   ├── Schedule.cshtml             # Browse classes (all roles)
│   └── Details.cshtml              # Class details & booking
├── Memberships/
│   ├── Packages.cshtml             # Package catalog
│   └── Purchase.cshtml             # Purchase confirmation
├── Staff/
│   ├── Members/
│   │   ├── Index.cshtml            # Member list
│   │   ├── Details.cshtml          # Member details
│   │   ├── Create.cshtml           # Create member
│   │   └── Edit.cshtml             # Edit member
│   ├── Bookings/
│   │   └── Index.cshtml            # Active bookings overview
│   └── Classes/
│       ├── Index.cshtml            # Upcoming sessions
│       ├── Details.cshtml          # Session details
│       └── Schedule.cshtml         # [Admin] Create session
└── Admin/
	├── Trainers/
	│   ├── Index.cshtml            # Trainer list
	│   ├── Details.cshtml          # Trainer details
	│   ├── Create.cshtml           # Create trainer
	│   └── Edit.cshtml             # Edit trainer
	├── MembershipPackages/
	│   └── Index.cshtml            # Package list
	├── ClassCategories/
	│   └── Index.cshtml            # Category list
	├── Rooms/
	│   └── Index.cshtml            # Room list
	└── Buildings/
		└── Index.cshtml            # Building list
```

## Running the Application

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB

### Steps
1. Ensure database is migrated (run migrations from Infrastructure project if needed)
2. Run the app:
   ```bash
   dotnet run --project src/GymWebApp/GymWebApp.csproj
   ```
3. Navigate to `https://localhost:5001`
4. Register a new member account OR sign in with default admin:
   - Email: `admin@gymapp.com`
   - Password: `Admin@123`

## Key Patterns

### Page Model Structure
```csharp
[Authorize(Policy = "MemberOnly")]
public class DashboardModel : PageModel
{
	private readonly IMemberQueryService _memberQueryService;
	// ... inject services

	public MemberDetailDto Member { get; set; }

	public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
	{
		// Load data using query services
		Member = await _memberQueryService.GetByUserIdAsync(...);
		return Page();
	}

	public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
	{
		// Execute command using command services
		await _commandService.DoSomethingAsync(command, cancellationToken);
		return RedirectToPage(...);
	}
}
```

### Command Mapping
```csharp
var command = new UpdateMemberCommand(
	Input.Firstname,
	Input.Lastname,
	Input.Email,
	Input.Phone,
	new AddressDto(...),
	Input.BirthDate,
	Input.MedicalNotes
);
await _memberCommandService.UpdateAsync(memberId, command, cancellationToken);
```

### Pagination
```csharp
var options = new ListOptions(page: CurrentPage, pageSize: PageSize);
var result = await _queryService.GetAllAsync(new GetAllQuery(), options, cancellationToken);
// result.Items - current page items
// result.Count - total count
// result.TotalPages - calculated total pages
```

## Notes
- All admin entities now have full CRUD operations
- Medical notes are only visible to staff, not members viewing their own profile
- Trainer creation returns a temporary password that should be displayed once to the admin
- Room building assignment cannot be changed after creation (edit page shows it as read-only)
- All forms include validation attributes for data integrity

## Future Enhancements
- Payment processing integration
- Attendance tracking
- Report generation (revenue, attendance, utilization)
- Email notifications for bookings and expirations
- Advanced search and filtering
- Mobile-responsive improvements
- Soft-delete restoration UI
- Bulk operations (e.g., cancel all sessions for a date)

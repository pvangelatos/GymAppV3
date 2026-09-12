# GymAppV3

Σύστημα διαχείρισης γυμναστηρίου / Pilates studio, αναπτυγμένο ως τελικό project για το **Coding Factory 10 (AUEB)**.

Περιλαμβάνει διαχείριση μελών, εκπαιδευτών, χώρων, τμημάτων μαθημάτων (class sessions), συνδρομών (memberships), κρατήσεων (bookings) και πληρωμών, με authentication/authorization τόσο στο backend όσο και στο frontend.

## Τεχνολογίες

- **Backend:** .NET 10, ASP.NET Core Minimal APIs, Entity Framework Core
- **Frontend:** ASP.NET Core Razor Pages (Server-Side Rendering)
- **Βάση Δεδομένων:** Microsoft SQL Server
- **Authentication:** ASP.NET Core Identity — JWT Bearer στο API, Cookie auth στο Razor Pages app
- **Validation:** FluentValidation (με ελληνικό localization μηνυμάτων)
- **API Documentation:** Swagger / Swashbuckle (`/swagger`) και Scalar (`/scalar`)
- **Testing:** xUnit (unit tests, EF Core InMemory provider), Schemathesis (API fuzz testing), `.http` αρχεία ως living documentation

## Αρχιτεκτονική

Το project ακολουθεί **Clean Architecture** με Domain-Driven Design στο μοντέλο:
```
GymAppV3/
├── GymAppV3.slnx
├── GymAppV3.Tests/              # Unit tests (xUnit)
└── src/
├── GymAppV3.Core/           # Domain entities, DTOs, interfaces, validators, business rules
    ├── GymAppV3.Infrastructure/ # EF Core DbContext, Identity, repositories/services, migrations
    ├── GymAppV3.Server/         # REST API — JWT auth, Swagger/Scalar
    └── GymWebApp/               # Razor Pages UI — cookie auth, καλεί απευθείας τα Core/Infrastructure services (χωρίς ενδιάμεσο API layer)
``` 
Τα **GymAppV3.Server** και **GymWebApp** είναι δύο ανεξάρτητες εφαρμογές (ξεχωριστό `Program.cs`, ξεχωριστό auth scheme, ξεχωριστό pipeline) που μοιράζονται την ίδια βάση δεδομένων και τα ίδια Core/Infrastructure/Identity layers. **Δεν** είναι SSO — δεν μοιράζονται session μεταξύ τους.

## Λειτουργίες (Features)

### Διαχείριση Μελών & Εκπαιδευτών
- Εγγραφή μέλους (self-service, δύο βημάτων: δημιουργία λογαριασμού → συμπλήρωση προφίλ με διεύθυνση, ημ/νία γέννησης, ιατρικά στοιχεία)
- Δημιουργία μέλους από στελέχη/admin χωρίς λογαριασμό χρήστη (ledger entry)
- Δημιουργία εκπαιδευτή από admin, με αυτόματη δημιουργία λογαριασμού και one-time password
- Ανάθεση specialties (κατηγοριών μαθημάτων) σε κάθε εκπαιδευτή
- Ιατρικά στοιχεία μέλους με έλεγχο πρόσβασης: ορατά σε admin, στο ίδιο το μέλος, ή σε εκπαιδευτή με το μέλος σε δικό του session
- Soft delete σε μέλη/εκπαιδευτές (ιστορικό διατηρείται)

### Χώροι, Αίθουσες & Κατηγορίες Μαθημάτων
- CRUD για κτίρια γυμναστηρίου (GymBuilding), αίθουσες (ClassRoom) και κατηγορίες μαθημάτων (ClassCategory)

### Πακέτα Συνδρομών & Συνδρομές (Memberships)
- CRUD πακέτων συνδρομής (MembershipPackage)
- Αγορά συνδρομής, με έλεγχο διαθεσιμότητας θέσεων ανά κατηγορία μαθήματος (βλ. παρακάτω)
- Παύση/επανενεργοποίηση συνδρομής (admin-only)

### Διαθεσιμότητα Θέσεων ανά Κατηγορία (Capacity Management)
- Υπολογισμός διαθέσιμων θέσεων συνδρομής ανά κατηγορία μαθήματος, με βάση την εβδομαδιαία χωρητικότητα των προγραμματισμένων sessions και την τρέχουσα ζήτηση από ενεργές συνδρομές
- Αυτόματο μπλοκάρισμα αγοράς όταν δεν υπάρχει διαθέσιμη θέση (μέλος που ανανεώνει συνδρομή στην ίδια κατηγορία δεν μπλοκάρεται ποτέ)
- Προβολή διαθεσιμότητας στο staff panel, πριν και κατά τη δημιουργία συνδρομής

### Προγραμματισμός Μαθημάτων & Κρατήσεις (Bookings)
- Δημιουργία/διαχείριση class sessions (ημερομηνία, ώρα, αίθουσα, εκπαιδευτής, χωρητικότητα)
- Προβολή προγράμματος σε εβδομαδιαίο calendar view (μέλος και στελέχη)
- Κράτηση θέσης από μέλος, με μείωση διαθέσιμων sessions στη συνδρομή του
- Ακύρωση κράτησης (state change, όχι hard delete) — αντίστοιχη επιστροφή session στη συνδρομή
- Κράτηση εκ μέρους μέλους από στελέχη, απευθείας μέσα από το calendar

### Πληρωμές & Αναφορές
- Καταγραφή πληρωμών ανά μέλος
- Μηνιαία οικονομική αναφορά (σύνοψη εσόδων)

### Authentication / Authorization
- JWT-based auth στο REST API, cookie-based auth στο Razor Pages UI
- Ρόλοι: Member, Trainer, Admin, TrainerAdmin, με policy-based εξουσιοδότηση ανά endpoint/σελίδα
- Rate limiting σε login/register (προστασία από brute-force) και globally ανά χρήστη/IP

### Ποιότητα & Robustness
- Στρωματοποιημένη validation (FluentValidation) με μηνύματα στα ελληνικά
- Ενιαίος χειρισμός σφαλμάτων (IExceptionHandler + ProblemDetails) σε όλα τα endpoints
- Testing: unit tests (xUnit) + fuzz testing του API με Schemathesis

## Προαπαιτούμενα

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (τοπική εγκατάσταση — Developer/Express edition ή LocalDB)
- (προαιρετικά) EF Core CLI tools: `dotnet tool install --global dotnet-ef`
- Git

## Build & Deploy (αναλυτικά)

### 1. Clone του repository

```bash
git clone https://github.com/pvangelatos/GymAppV3.git
cd GymAppV3
```

### 2. Restore & Build

```bash
dotnet restore
dotnet build
```

### 3. Ρύθμιση Secrets

Για λόγους ασφαλείας, το connection string, το JWT signing key και τα admin credentials **δεν** βρίσκονται στο `appsettings.json` — ρυθμίζονται μέσω `dotnet user-secrets`. Επειδή τα δύο apps τρέχουν ως ξεχωριστές διεργασίες, χρειάζεται να τα ορίσεις **και στα δύο** projects.

**GymAppV3.Server:**

```bash
cd src/GymAppV3.Server
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GymAppV3;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<ένα μεγάλο, τυχαίο secret key — τουλάχιστον 32 χαρακτήρες>"
dotnet user-secrets set "Jwt:Issuer" "GymAppV3"
dotnet user-secrets set "Jwt:Audience" "GymAppV3Client"
dotnet user-secrets set "DefaultAdmin:Email" "admin@gymapp.com"
dotnet user-secrets set "DefaultAdmin:Password" "Admin123!"
```

**GymWebApp:**

```bash
cd ../GymWebApp
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GymAppV3;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "DefaultAdmin:Email" "admin@gymapp.com"
dotnet user-secrets set "DefaultAdmin:Password" "Admin123!"
```

> Χρησιμοποίησε **το ίδιο** connection string και στα δύο projects, ώστε να διαβάζουν την ίδια βάση (πίνακες Core + Infrastructure + Identity είναι κοινοί).

Αν η SQL Server σου τρέχει σε named instance (π.χ. `SQLEXPRESS`), προσάρμοσε ανάλογα το `Server=` στο connection string (π.χ. `Server=localhost\SQLEXPRESS;...`).

### 4. Δημιουργία / Migration της βάσης

Δεν χρειάζεται χειροκίνητο βήμα — και τα δύο apps καλούν `MigrateAsync()` στο startup, οπότε η βάση δημιουργείται και γίνεται migrate αυτόματα με το πρώτο `dotnet run`. Αν προτιμάς να το κάνεις χειροκίνητα πριν την πρώτη εκτέλεση:

```bash
cd src/GymAppV3.Server
dotnet ef database update --project ../GymAppV3.Infrastructure
```

### 5. Εκτέλεση της εφαρμογής

Άνοιξε **δύο ξεχωριστά terminal**, καθώς τα δύο apps τρέχουν παράλληλα ως ξεχωριστές διεργασίες:

**Terminal 1 — REST API (GymAppV3.Server):**

```bash
cd src/GymAppV3.Server
dotnet run
```

Η κονσόλα θα δείξει το URL (π.χ. `https://localhost:5001` ή αντίστοιχο — δες `Properties/launchSettings.json` για το ακριβές port). Διαθέσιμα:
- Swagger UI: `<url>/swagger`
- Scalar UI: `<url>/scalar`
- Auth: login μέσω `POST /api/auth/login`, μετά χρήση του JWT token στο κουμπί **Authorize** του Swagger/Scalar.

**Terminal 2 — Razor Pages UI (GymWebApp):**

```bash
cd src/GymWebApp
dotnet run
```

Τρέχει σε ξεχωριστό port (δες `Properties/launchSettings.json`). Το login/register γίνεται από τη δική του UI, με cookie auth και ρόλους Member/Trainer/Admin/TrainerAdmin.

### 6. Login ως Admin

Χρησιμοποίησε τα credentials που έβαλες στο `DefaultAdmin:Email` / `DefaultAdmin:Password` (π.χ. `admin@gymapp.com` / `Admin123!`) — δημιουργούνται/seed-άρονται αυτόματα στο πρώτο εκκίνημα, τόσο ο ρόλος Admin όσο και οι υπόλοιποι ρόλοι.

### Εναλλακτικά — SQL Server μέσω Docker

Αν προτιμάς να τρέξεις τη βάση σε container αντί για τοπική εγκατάσταση:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name gymappv3-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

και προσάρμοσε το connection string σε:

Server=localhost,1433;Database=GymAppV3;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True


## Testing

```bash
dotnet test
```

Unit tests στο `GymAppV3.Tests` (EF Core InMemory provider). Επιπλέον, το REST API ελέγχθηκε με **Schemathesis** πάνω στο Swagger/OpenAPI schema για fuzz/contract testing, και υπάρχουν `.http` αρχεία που λειτουργούν σαν ζωντανό documentation σεναρίων χρήσης.

## API Documentation

- Swagger UI (Swashbuckle): `<GymAppV3.Server URL>/swagger`
- Scalar (εναλλακτικό UI πάνω στο ίδιο OpenAPI schema): `<GymAppV3.Server URL>/scalar`

## Ρόλοι & Δικαιώματα

- **Member** — self-service: προφίλ, bookings, memberships
- **Trainer** — διαχείριση των δικών του class sessions/bookings
- **Admin / TrainerAdmin** — πλήρες CRUD σε Trainers, MembershipPackages, ClassCategories, Rooms, Buildings, Members

## Σημειώσεις

- Το `appsettings.json` περιέχει μόνο μη-ευαίσθητες ρυθμίσεις (business rules, logging). Ευαίσθητα στοιχεία πάνε πάντα μέσω `user-secrets` (dev) ή environment variables / secret manager (production).
- Rate limiting ενεργό στα auth endpoints (login/register) και globally ανά χρήστη/IP.

## Author

**Panagiotis Vangelatos**

[![GitHub](https://img.shields.io/badge/GitHub-pvangelatos-181717?style=flat&logo=github&logoColor=white)](https://github.com/pvangelatos)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Panagiotis%20Vangelatos-0A66C2?style=flat&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/panagiotis-vangelatos-71003525a/)

---

Αναπτύχθηκε από τον Παναγιώτη Βαγγελάτο ως τελικό project στο πλαίσιο της εκπαίδευσής του στο Coding Factory 10 — AUEB.
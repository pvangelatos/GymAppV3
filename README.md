# GymAppV3

Σύστημα διαχείρισης γυμναστηρίου / Pilates studio, αναπτυγμένο σε **.NET 10** με **Clean Architecture**, ως τελικό project για το **Coding Factory 9 (AUEB)**.

Καλύπτει μέλη, εκπαιδευτές, χώρους, πρόγραμμα μαθημάτων, συνδρομές, κρατήσεις και πληρωμές, μέσα από **δύο εφαρμογές πάνω στο ίδιο domain**: ένα REST API (JWT) και ένα server-rendered UI σε Razor Pages (cookies).

## Με μια ματιά

- **Clean Architecture** — Core (domain, κανόνες) → Infrastructure (EF Core, Identity) → δύο presentation apps
- **Authorization μέσα στα services**, όχι μόνο στα endpoints: το ίδιο domain προστατεύεται είτε το καλεί το API είτε το UI
- **Ενιαίος χειρισμός σφαλμάτων** με `IExceptionHandler` + `ProblemDetails` (400 / 403 / 404 / 409 / 422)
- **Optimistic concurrency** (`RowVersion`) στις κρατήσεις και στις θέσεις μαθημάτων
- **Soft delete** μέσω EF Core interceptor, με global query filters
- **64 unit tests** (xUnit) και **fuzz testing** του API με Schemathesis: 6.000+ test cases, **0 server errors**

## Τεχνολογίες

| Περιοχή | Επιλογή |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, Entity Framework Core 10 |
| Frontend | ASP.NET Core Razor Pages (SSR), Bootstrap 5, FullCalendar |
| Βάση | Microsoft SQL Server (LocalDB / Express / Developer) |
| Auth | ASP.NET Core Identity — JWT Bearer (API), Cookies (UI), reCAPTCHA v3 |
| Validation | FluentValidation, με μηνύματα στα ελληνικά |
| Docs | Swagger (Swashbuckle) και Scalar |
| Testing | xUnit, FluentAssertions, EF Core InMemory, Schemathesis |

## Αρχιτεκτονική

```
GymAppV3/
├── GymAppV3.slnx
├── src/
│   ├── GymAppV3.Core/            # Entities, DTOs, interfaces, validators, exceptions
│   ├── GymAppV3.Infrastructure/  # EF Core DbContext, mappings, migrations, Identity, services
│   ├── GymAppv3.Server/          # REST API — Minimal APIs, JWT, Swagger/Scalar, rate limiting
│   └── GymWebApp/                # Razor Pages UI — cookie auth, καλεί απευθείας τα services
└── test/
    └── GymAppV3.Tests/           # Unit tests (xUnit)
```

Τα **GymAppv3.Server** και **GymWebApp** είναι δύο ανεξάρτητες εφαρμογές: κάθε μία έχει δικό της `Program.cs`, auth scheme και pipeline. Μοιράζονται τη βάση και τα layers Core/Infrastructure/Identity. **Δεν** είναι SSO, οπότε δεν μοιράζονται session.

Το UI **δεν** καλεί το API. Χρησιμοποιεί απευθείας τα ίδια services μέσω DI. Γι' αυτό οι κανόνες και οι έλεγχοι δικαιωμάτων ζουν στα services: ισχύουν ό,τι κι αν είναι το «μπροστά» μέρος.

## Λειτουργίες

### Μέλη & Εκπαιδευτές
- Εγγραφή μέλους σε δύο βήματα: λογαριασμός και μετά προφίλ (διεύθυνση, ημ/νία γέννησης, ιατρικά στοιχεία)
- Δημιουργία μέλους από το προσωπικό, χωρίς λογαριασμό χρήστη
- Δημιουργία εκπαιδευτή από admin, με αυτόματο λογαριασμό και προσωρινό κωδικό
- Ειδικότητες εκπαιδευτών (κατηγορίες μαθημάτων)
- Ιατρικές σημειώσεις ορατές μόνο σε admin, στο ίδιο το μέλος, ή σε εκπαιδευτή που έχει το μέλος σε δικό του μάθημα
- Αναζήτηση, ταξινόμηση και σελιδοποίηση

### Χώροι & Κατηγορίες
- Κτίρια, αίθουσες (με χωρητικότητα) και κατηγορίες μαθημάτων

### Πρόγραμμα Μαθημάτων
- Προγραμματισμός μεμονωμένου μαθήματος, επαναλαμβανόμενου (εβδομαδιαία) και αντιγραφή ολόκληρης εβδομάδας, με έλεγχο συγκρούσεων αίθουσας και εκπαιδευτή (all-or-nothing)
- Εβδομαδιαίο calendar για μέλη και προσωπικό

### Συνδρομές & Διαθεσιμότητα
- Πακέτα συνδρομών (διάρκεια, αριθμός sessions, κατηγορία)
- Αγορά από το μέλος ή ανάθεση από το προσωπικό
- **Διαχείριση χωρητικότητας ανά κατηγορία:** η αγορά μπλοκάρεται όταν η εβδομαδιαία ζήτηση των ενεργών συνδρομών ξεπερνά τις διαθέσιμες θέσεις του προγράμματος (βλ. κανόνες). Το προσωπικό βλέπει τη διαθεσιμότητα σε ξεχωριστή σελίδα.

### Κρατήσεις
- Κράτηση από το μέλος ή εκ μέρους του από το προσωπικό, μέσα από το calendar
- Ακύρωση ως αλλαγή κατάστασης (όχι διαγραφή)

### Πληρωμές & Αναφορές
- Καταγραφή πληρωμών με αποθηκευμένο ΦΠΑ (snapshot του συντελεστή τη στιγμή της πληρωμής)
- Μηνιαία οικονομική αναφορά (μόνο ολοκληρωμένες πληρωμές)

## Επιχειρηματικοί κανόνες

- **Κράτηση:** απαιτεί ενεργή συνδρομή στην κατηγορία του μαθήματος με διαθέσιμα sessions. Χρεώνεται η συνδρομή που λήγει πρώτη, και η κράτηση θυμάται ποια χρεώθηκε.
- **Ακύρωση:** επιτρέπεται μόνο πριν ξεκινήσει το μάθημα. Η θέση ελευθερώνεται πάντα, ενώ το session επιστρέφει στη συνδρομή που χρεώθηκε **μόνο αν η ακύρωση γίνει τουλάχιστον 24 ώρες πριν**.
- **Πληρωμή αγοράς:** όταν αγοράζει το ίδιο το μέλος, η πληρωμή καταγράφεται ως **Pending** (δεν υπάρχει payment gateway). Όταν την αναθέτει το προσωπικό, καταγράφεται ως **Completed**. Μόνο το προσωπικό καταγράφει πληρωμές.
- **Χωρητικότητα κατηγορίας:** εβδομαδιαία προσφορά = άθροισμα θέσεων των μαθημάτων των επόμενων 28 ημερών ÷ 4. Εβδομαδιαία ζήτηση = άθροισμα (sessions ÷ εβδομάδες) των ενεργών συνδρομών. Μέλος που ανανεώνει συνδρομή στην ίδια κατηγορία δεν μπλοκάρεται ποτέ.
- **Διαγραφές:** μέλη, εκπαιδευτές, κτίρια, κατηγορίες και πακέτα διαγράφονται με soft delete. Όταν διαγράφεται μέλος, ακυρώνονται οι **μελλοντικές** κρατήσεις του και ελευθερώνονται οι θέσεις, ενώ το ιστορικό και οι πληρωμές μένουν. Κτίριο με αίθουσες και αίθουσα με μαθήματα δεν διαγράφονται.

## Ρόλοι & Δικαιώματα

| Ρόλος | Δικαιώματα |
|---|---|
| **Member** | Δικό του προφίλ, κρατήσεις, συνδρομές και πληρωμές |
| **Trainer** | Προσωπικό: μέλη, κρατήσεις εκ μέρους μελών, προγραμματισμός μαθημάτων από το UI |
| **TrainerAdmin / Admin** | Ό,τι ο Trainer, και επιπλέον πλήρης διαχείριση (εκπαιδευτές, πακέτα, χώροι, κατηγορίες, διαγραφές, οικονομικές αναφορές) |

Στο REST API ο προγραμματισμός μαθημάτων απαιτεί ρόλο admin.

## Σχεδιαστικές αποφάσεις

- **Authorization στα services (defense in depth).** Κάθε service ελέγχει ποιος καλεί (`IUserContext`), π.χ. ότι ένα μέλος κάνει κράτηση μόνο για τον εαυτό του. Τα endpoints και οι σελίδες έχουν επιπλέον policies, και ένα endpoint filter (`MemberScopedFilter`) απορρίπτει αμέσως requests σε `/api/members/{id}/...` άλλου μέλους.
- **Δύο εφαρμογές, ένα domain.** Το UI χρησιμοποιεί τα services απευθείας, χωρίς HTTP hop, ενώ το API μένει διαθέσιμο για μελλοντικό SPA ή mobile client.
- **Soft delete με interceptor.** Ένα `SaveChangesInterceptor` μετατρέπει τα `Remove()` σε `IsDeleted = true`, συμπληρώνει τα audit πεδία, και διατηρεί τα owned types (π.χ. `Address`) που μοιράζονται τον ίδιο πίνακα.
- **Optimistic concurrency.** Το `RowVersion` προστατεύει τις τελευταίες θέσεις ενός μαθήματος από ταυτόχρονες κρατήσεις. Το conflict επιστρέφεται ως **409**.
- **Ενιαία σφάλματα.** Τα domain exceptions (`NotFound`, `BusinessRule`, `Forbidden`) μετατρέπονται σε `ProblemDetails`. Παραβιάσεις constraints της βάσης γίνονται 409, και κανένα εσωτερικό μήνυμα δεν φτάνει στον client.
- **Projections με Expressions.** Ο `ObjectMapper` κρατά για κάθε entity ένα `Expression` (για `IQueryable`, ώστε να μεταφράζεται σε SQL) και μια compiled εκδοχή του για αντικείμενα στη μνήμη. Δεν χρησιμοποιεί reflection-based mapper.
- **Σελιδοποίηση με όρια.** Μέγιστο μέγεθος σελίδας 200, και ο υπολογισμός του `skip` γίνεται σε `long` για προστασία από overflow.
- **Αποδοτικός έλεγχος συγκρούσεων.** Ο επαναλαμβανόμενος προγραμματισμός ελέγχει όλες τις εμφανίσεις με ένα query, αντί για δύο ανά εμφάνιση.

## Ποιότητα & Testing

```bash
dotnet test
```

- **64 unit tests** (xUnit, FluentAssertions) στο `test/GymAppV3.Tests`: κανόνες κρατήσεων και ακυρώσεων, συνδρομές και χωρητικότητα, πληρωμές, authorization (π.χ. ότι ένα μέλος δεν ενεργεί για άλλο μέλος), soft delete, συγκρούσεις στο πρόγραμμα, validators.
- **Fuzz testing με [Schemathesis](https://schemathesis.io/):** property-based και stateful έλεγχος του API πάνω στο OpenAPI schema. Τελευταίο run: **6.107 test cases, 0 server errors**. Τα σφάλματα που βρήκαν τα προηγούμενα runs διορθώθηκαν.
- Το `src/GymAppv3.Server/GymAppv3.Server.http` περιέχει έτοιμα requests για χειροκίνητη δοκιμή.
- **Security:** rate limiting στο API (login 5/λεπτό, register 3/5 λεπτά, global ανά χρήστη/IP), account lockout και reCAPTCHA v3 στο UI, και ίδια απάντηση για λάθος κωδικό ή ανύπαρκτο email (χωρίς user enumeration).

## Εγκατάσταση & Εκτέλεση

### Προαπαιτούμενα

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server — το **LocalDB** έρχεται με το Visual Studio και αρκεί
- (προαιρετικά) `dotnet tool install --global dotnet-ef`

### 1. Clone & build

```bash
git clone https://github.com/pvangelatos/GymAppV3.git
cd GymAppV3
dotnet build
```

### 2. Βάση δεδομένων

Και τα δύο `appsettings.json` δείχνουν στο LocalDB (`(localdb)\MSSQLLocalDB`, βάση `GymAppV3`, Windows authentication). Αν χρησιμοποιείς άλλον SQL Server, κάνε override με user-secret **και στα δύο** projects:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GymAppV3;Trusted_Connection=True;TrustServerCertificate=True"
```

Η βάση δημιουργείται και γίνεται migrate **αυτόματα** στην εκκίνηση: από το WebApp πάντα, από το API σε Development.

### 3. Secrets

Τα ευαίσθητα στοιχεία δεν είναι στο repository. Ρυθμίζονται με `dotnet user-secrets`.

**API** (`src/GymAppv3.Server`):

```bash
cd src/GymAppv3.Server
dotnet user-secrets set "Jwt:Key" "<τυχαίο κλειδί, τουλάχιστον 32 χαρακτήρες>"
dotnet user-secrets set "Jwt:Issuer" "GymAppV3"
dotnet user-secrets set "Jwt:Audience" "GymAppV3"
dotnet user-secrets set "DefaultAdmin:Email" "admin@gymapp.com"
dotnet user-secrets set "DefaultAdmin:Password" "Admin123!"
```

**UI** (`src/GymWebApp`):

```bash
cd ../GymWebApp
dotnet user-secrets set "DefaultAdmin:Email" "admin@gymapp.com"
dotnet user-secrets set "DefaultAdmin:Password" "Admin123!"
dotnet user-secrets set "Recaptcha:SiteKey" "<site key>"
dotnet user-secrets set "Recaptcha:SecretKey" "<secret key>"
```

Το login και το register του UI χρησιμοποιούν **reCAPTCHA v3**. Δημιούργησε δικά σου κλειδιά στο [Google reCAPTCHA admin](https://www.google.com/recaptcha/admin) (τύπος v3, domain `localhost`). Το site key που υπάρχει στο `appsettings.Development.json` λειτουργεί μόνο με το secret του συγγραφέα.

Ο admin λογαριασμός και οι ρόλοι δημιουργούνται αυτόματα στην πρώτη εκκίνηση, από τα `DefaultAdmin:*`.

### 4. Εκτέλεση

Τα δύο apps τρέχουν παράλληλα, σε **δύο terminals**:

```bash
# Terminal 1 — REST API
cd src/GymAppv3.Server
dotnet run --launch-profile https
```

- Swagger: https://localhost:7108/swagger
- Scalar: https://localhost:7108/scalar
- Login με `POST /api/auth/login`, και μετά το token στο **Authorize**

```bash
# Terminal 2 — UI
cd src/GymWebApp
dotnet run --launch-profile https
```

- UI: https://localhost:7072

Το Swagger και το Scalar είναι διαθέσιμα μόνο σε Development, που είναι το default του `dotnet run`.

## Γνωστοί περιορισμοί & επόμενα βήματα

- **Πληρωμές:** δεν υπάρχει payment gateway, ούτε ακόμα ροή επιβεβαίωσης για τις Pending πληρωμές.
- **Tests:** τα unit tests τρέχουν σε EF Core InMemory, που δεν ελέγχει foreign keys, unique indexes ή case-insensitive collation. Αυτά καλύπτονται από χειροκίνητους ελέγχους και το Schemathesis. Επόμενο βήμα: integration tests με `WebApplicationFactory` και SQL Server (Testcontainers).
- **Χωρητικότητα:** ο έλεγχος διαθεσιμότητας κατά την αγορά δεν κλειδώνει, οπότε δύο ταυτόχρονες αγορές της τελευταίας θέσης μπορούν να περάσουν και οι δύο.
- **Rate limiting:** in-memory, για ένα instance. Σε πολλαπλά instances χρειάζεται κοινό store (π.χ. Redis).
- **OpenAPI:** οι κανόνες του FluentValidation (π.χ. μέγιστα μήκη) δεν εμφανίζονται στο schema.
- **Σχεδιάζονται:** λίστα αναμονής, ειδοποιήσεις μέσω email, παρουσιολόγιο, δεύτερο frontend σε Angular πάνω στο API.

## Συγγραφέας

**Παναγιώτης Βαγγελάτος**

[![GitHub](https://img.shields.io/badge/GitHub-pvangelatos-181717?style=flat&logo=github&logoColor=white)](https://github.com/pvangelatos)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Panagiotis%20Vangelatos-0A66C2?style=flat&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/panagiotis-vangelatos-71003525a/)

Τελικό project στο πλαίσιο του Coding Factory 9 — AUEB.
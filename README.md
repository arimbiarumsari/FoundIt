# FoundIt

Aplikasi untuk mencari barang hilang di lingkungan FT

Kelompok FoundIT
Ketua Kelompok: Muhammad Bintang Hidayatullah Marbun
Anggota 1: Arimbi Arum Sari - 24/541867/TK/60129
Anggota 2: Aston Hugo - 24/538303/TK/59700
Anggota 3: Muhammad Bintang Hidayatullah Marbun - 24/544012/TK/60468

## Backend progress

The first backend milestone provides:

- an ASP.NET Core API targeting .NET 10;
- a `/health` endpoint;
- the initial `Report` and lightweight `Listing` domain model;
- validated report submission DTOs and business rules;
- an in-memory repository boundary (no SQL dependency); and
- xUnit tests for report creation and incident-date validation.

The in-memory repository is development-only and loses data when the application
stops. It will be replaced after the assignment's permitted non-SQL database is
confirmed.

### Prerequisites and commands

Install the .NET 10 SDK, then run:

```bash
dotnet restore
dotnet test
dotnet run --project src/FoundIt.Api
```

The local API health check will be available at `/health` on the URL printed by
`dotnet run`.

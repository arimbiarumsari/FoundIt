using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;
using FoundIt.Api.Services;
using FoundIt.Data;

namespace FoundIt.Data;

public sealed class InMemoryAppStore
{
    private readonly List<User> _users = [];
    private readonly List<Report> _reports = [];
    private readonly List<Listing> _listings = [];
    private readonly List<Category> _categories = [];
    private readonly FoundItDatabase? _database;
    private readonly ReportService _reportService;

    // Tanpa database data hanya disimpan di memori (data contoh).
    // Dengan database, data dimuat dari PostgreSQL dan setiap perubahan ikut disimpan.
    public InMemoryAppStore(FoundItDatabase? database = null)
    {
        _database = database;
        _reportService = new ReportService(database is null
            ? new InMemoryReportRepository()
            : new PostgresReportRepository(database));

        if (database is null)
        {
            SeedData();
        }
        else
        {
            LoadFromDatabase(database);
        }
    }

    public bool UsesDatabase => _database is not null;

    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<Listing> Listings => _listings;

    public User Register(string? name, string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nama wajib diisi.");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email wajib diisi.");
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password wajib diisi.");

        name = name.Trim();
        email = email.Trim().ToLowerInvariant();

        if (name.Length < 2)
        {
            throw new ArgumentException("Nama minimal terdiri dari 2 karakter.");
        }

        if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
        {
            throw new ArgumentException("Format email tidak valid.");
        }

        if (password.Length < 8 || !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            throw new ArgumentException(
                "Password minimal 8 karakter dan memiliki huruf besar, huruf kecil, serta angka.");
        }

        if (_users.Any(user => user.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Email sudah terdaftar.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password)
        };
        _database?.InsertUser(user);
        _users.Add(user);
        return user;
    }

    public User? Login(string email, string password)
    {
        var user = _users.SingleOrDefault(candidate =>
            candidate.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

        return user is not null && user.Status == UserStatus.Verified &&
               PasswordHasher.Verify(password, user.PasswordHash)
            ? user
            : null;
    }

    public async Task<Report> SubmitReportAsync(
        User user,
        CreateReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_categories.Any(category => category.Id == request.CategoryId))
            throw new ArgumentException("Kategori tidak ditemukan. Pilih kategori yang tersedia.");

        var report = await _reportService.CreateAsync(
            user.Id.ToString(), request, DateTimeOffset.UtcNow, cancellationToken);
        user.SubmitReport(report);
        _reports.Add(report);
        return report;
    }

    public IEnumerable<Report> GetReportsFor(User user) =>
        _reports.Where(report => report.UserId == user.Id.ToString())
            .OrderByDescending(report => report.CreatedAtUtc);

    public IEnumerable<Report> GetPendingReports() =>
        _reports.Where(report => report.Status == ReportStatus.Pending)
            .OrderBy(report => report.CreatedAtUtc);

    public IEnumerable<Listing> SearchListings(string? keyword) =>
        _listings.Where(listing => listing.Status == ListingStatus.Active)
            .Where(listing => string.IsNullOrWhiteSpace(keyword) ||
                listing.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                listing.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                listing.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(listing => listing.PublishedAtUtc);

    public IEnumerable<Listing> AllListings =>
        _listings.OrderByDescending(listing => listing.PublishedAtUtc);

    public Listing? GetListing(Guid id) => _listings.SingleOrDefault(listing => listing.Id == id);

    // Endpoint publik mengikuti aturan visibilitas pencarian.
    public Listing? GetPublicListing(Guid id) =>
        _listings.SingleOrDefault(listing => listing.Id == id &&
            listing.Status == ListingStatus.Active && listing.Report.Status == ReportStatus.Approved);

    public Report? GetReport(Guid id) => _reports.SingleOrDefault(report => report.Id == id);

    public string GetUserName(string userId) =>
        _users.FirstOrDefault(user => user.Id.ToString() == userId)?.Name ?? "Sistem";

    public void CancelReport(Report report)
    {
        report.Delete(DateTimeOffset.UtcNow);
        _database?.UpdateReport(report);
    }

    public void SetListingStatus(Admin admin, Listing listing, ListingStatus status)
    {
        admin.UpdateListing(listing, status, DateTimeOffset.UtcNow);
        _database?.UpdateListing(listing);
    }

    public void DeleteListing(Admin admin, Listing listing)
    {
        admin.DeleteListing(listing, DateTimeOffset.UtcNow);
        _database?.UpdateListing(listing);
    }

    public Listing Approve(Admin admin, Report report)
    {
        var category = _categories.Single(category => category.Id == report.CategoryId);
        var listing = admin.VerifyReport(report, category, DateTimeOffset.UtcNow);
        _database?.ApproveReport(report, listing);
        _listings.Add(listing);
        return listing;
    }

    public void Reject(Admin admin, Report report, string reason)
    {
        report.Reject(admin.Id.ToString(), reason, DateTimeOffset.UtcNow);
        _database?.UpdateReport(report);
    }

    private void LoadFromDatabase(FoundItDatabase database)
    {
        _categories.AddRange(database.LoadCategories());
        if (_categories.Count == 0)
        {
            foreach (var category in CreateDefaultCategories())
            {
                database.InsertCategory(category);
                _categories.Add(category);
            }
        }

        _users.AddRange(database.LoadUsers());
        if (!_users.OfType<Admin>().Any())
        {
            var admin = CreateAdmin();
            if (admin is not null)
            {
                database.InsertUser(admin);
                _users.Add(admin);
            }
        }

        _reports.AddRange(database.LoadReports());
        foreach (var listing in database.LoadListings(_reports, _categories))
        {
            listing.Category.AddListing(listing);
            _listings.Add(listing);
        }
    }

    private static List<Category> CreateDefaultCategories() =>
    [
        new Category { Id = Guid.NewGuid(), Name = "Elektronik", Description = "Telepon, laptop, charger, dan perangkat elektronik lainnya." },
        new Category { Id = Guid.NewGuid(), Name = "Dokumen", Description = "Kartu identitas dan dokumen." },
        new Category { Id = Guid.NewGuid(), Name = "Tas", Description = "Tas, dompet, dan wadah barang." },
        new Category { Id = Guid.NewGuid(), Name = "Kunci", Description = "Kunci kendaraan, kamar, dan lainnya." },
        new Category { Id = Guid.NewGuid(), Name = "Lainnya", Description = "Barang di luar kategori utama." }
    ];

    private static Admin? CreateAdmin()
    {
        var adminPassword = DatabaseSettings.GetAdminPassword();
#if DEBUG
        // Hanya untuk build Debug: biar akun admin bisa dicoba tanpa set environment variable.
        adminPassword ??= "Admin12345";
#endif
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            return null;
        }

        return new Admin
        {
            Id = Guid.NewGuid(),
            Name = "Administrator FoundIt",
            Email = "admin@foundit.local",
            PasswordHash = PasswordHasher.Hash(adminPassword)
        };
    }

    private void SeedData()
    {
        _categories.AddRange(CreateDefaultCategories());
        var electronics = _categories[0];

        var admin = CreateAdmin();
        if (admin is not null)
        {
            _users.Add(admin);
        }

        SeedPublicListing(electronics);
        SeedPublicListing(_categories[2], "Dompet hitam", "Dompet hitam berisi KTM, hilang di sekitar perpustakaan.", "Perpustakaan FT", ReportType.Lost);
        SeedPublicListing(_categories[3], "Kunci motor", "Kunci motor dengan gantungan kucing, ditemukan di parkiran.", "Parkiran Teknik", ReportType.Found);
        SeedPublicListing(electronics, "Charger laptop", "Charger laptop hitam tertinggal di kantin.", "Kantin Teknik", ReportType.Lost);
        SeedPendingReport(_categories[1], "KTM atas nama Budi", "KTM ditemukan di depan musala FT, masih bagus.", "Musala FT", ReportType.Found);
    }

    private void SeedPendingReport(Category category, string title, string description, string location, ReportType type)
    {
        _reports.Add(new Report
        {
            Id = Guid.NewGuid(),
            UserId = "system",
            CategoryId = category.Id,
            Type = type,
            Title = title,
            Description = description,
            Location = location,
            IncidentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = ReportStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
    }

    private void SeedPublicListing(
        Category category,
        string title = "Botol minum biru",
        string description = "Botol minum ditemukan di ruang kelas Teknik Elektro.",
        string location = "Gedung DTETI",
        ReportType type = ReportType.Found)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = "system",
            CategoryId = category.Id,
            Type = type,
            Title = title,
            Description = description,
            Location = location,
            IncidentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = ReportStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        report.Approve("system", DateTimeOffset.UtcNow);
        var listing = Listing.Publish(report, category, DateTimeOffset.UtcNow);
        category.AddListing(listing);
        _reports.Add(report);
        _listings.Add(listing);
    }
}

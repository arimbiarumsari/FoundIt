using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;
using FoundIt.Api.Services;
using FoundIt.Infrastructure.Security;
using LiteDB;

namespace FoundIt.Infrastructure.Data;

public sealed class FoundItDatabase : IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<UserDocument> _userDocuments;
    private readonly ILiteCollection<CategoryDocument> _categoryDocuments;
    private readonly ILiteCollection<ReportDocument> _reportDocuments;
    private readonly ILiteCollection<ListingDocument> _listingDocuments;
    private readonly List<User> _users = [];
    private readonly List<Report> _reports = [];
    private readonly List<Listing> _listings = [];
    private readonly List<Category> _categories = [];
    private readonly ReportService _reportService = new(new InMemoryReportRepository());

    public FoundItDatabase(string? databasePath = null)
    {
        databasePath ??= DefaultDatabasePath();
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        _database = new LiteDatabase($"Filename={databasePath};Connection=shared");
        _userDocuments = _database.GetCollection<UserDocument>("users");
        _categoryDocuments = _database.GetCollection<CategoryDocument>("categories");
        _reportDocuments = _database.GetCollection<ReportDocument>("reports");
        _listingDocuments = _database.GetCollection<ListingDocument>("listings");

        ConfigureIndexes();
        SeedData();
        LoadDomainGraph();
    }

    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<Listing> Listings => _listings;

    public User Register(string name, string email, string password)
    {
        name = name.Trim();
        email = email.Trim().ToLowerInvariant();
        ValidateRegistration(name, email, password);

        if (_userDocuments.Exists(user => user.Email == email))
            throw new InvalidOperationException("Email sudah terdaftar.");

        var user = new User
        {
            Id = Guid.NewGuid(), Name = name, Email = email,
            PasswordHash = PasswordHasher.Hash(password)
        };
        _userDocuments.Insert(ToDocument(user));
        _users.Add(user);
        return user;
    }

    public User? Login(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = _users.SingleOrDefault(candidate => candidate.Email == normalizedEmail);
        return user is not null && user.Status == UserStatus.Verified &&
               PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
    }

    public async Task<Report> SubmitReportAsync(
        User user, CreateReportRequest request, CancellationToken cancellationToken = default)
    {
        var report = await _reportService.CreateAsync(
            user.Id.ToString(), request, DateTimeOffset.UtcNow, cancellationToken);
        user.SubmitReport(report);
        _reportDocuments.Insert(ToDocument(report));
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

    public Listing Approve(Admin admin, Report report)
    {
        var category = _categories.Single(item => item.Id == report.CategoryId);
        var listing = admin.VerifyReport(report, category, DateTimeOffset.UtcNow);

        _database.BeginTrans();
        try
        {
            _reportDocuments.Update(ToDocument(report));
            _listingDocuments.Insert(ToDocument(listing));
            _database.Commit();
        }
        catch
        {
            _database.Rollback();
            throw;
        }

        _listings.Add(listing);
        return listing;
    }

    public void Reject(Admin admin, Report report, string reason)
    {
        report.Reject(admin.Id.ToString(), reason, DateTimeOffset.UtcNow);
        _reportDocuments.Update(ToDocument(report));
    }

    public void Dispose() => _database.Dispose();

    private void ConfigureIndexes()
    {
        _userDocuments.EnsureIndex(user => user.Email, unique: true);
        _reportDocuments.EnsureIndex(report => report.UserId);
        _reportDocuments.EnsureIndex(report => report.Status);
        _listingDocuments.EnsureIndex(listing => listing.ReportId, unique: true);
        _listingDocuments.EnsureIndex(listing => listing.Status);
    }

    private void SeedData()
    {
        if (_categoryDocuments.Count() == 0)
        {
            _categoryDocuments.InsertBulk(new[]
            {
                Category("Elektronik", "Telepon, laptop, charger, dan perangkat elektronik lainnya."),
                Category("Dokumen", "Kartu identitas dan dokumen."),
                Category("Tas", "Tas, dompet, dan wadah barang."),
                Category("Kunci", "Kunci kendaraan, kamar, dan lainnya."),
                Category("Lainnya", "Barang di luar kategori utama.")
            });
        }

        var adminPassword = Environment.GetEnvironmentVariable("FOUNDIT_ADMIN_PASSWORD");
        if (!string.IsNullOrWhiteSpace(adminPassword) &&
            !_userDocuments.Exists(user => user.Email == "admin@foundit.local"))
        {
            _userDocuments.Insert(new UserDocument
            {
                Id = Guid.NewGuid(), Name = "Administrator FoundIt",
                Email = "admin@foundit.local", PasswordHash = PasswordHasher.Hash(adminPassword),
                Status = UserStatus.Verified, Role = UserRole.Admin
            });
        }
    }

    private void LoadDomainGraph()
    {
        _categories.AddRange(_categoryDocuments.FindAll().Select(document => new Category
        {
            Id = document.Id, Name = document.Name, Description = document.Description
        }));

        _users.AddRange(_userDocuments.FindAll().Select(document => document.Role == UserRole.Admin
            ? new Admin
            {
                Id = document.Id, Name = document.Name, Email = document.Email,
                PasswordHash = document.PasswordHash, Status = document.Status
            }
            : new User
            {
                Id = document.Id, Name = document.Name, Email = document.Email,
                PasswordHash = document.PasswordHash, Status = document.Status
            }));

        _reports.AddRange(_reportDocuments.FindAll().Select(FromDocument));

        foreach (var document in _listingDocuments.FindAll())
        {
            var report = _reports.Single(item => item.Id == document.ReportId);
            var category = _categories.Single(item => item.Id == document.CategoryId);
            var listing = new Listing
            {
                Id = document.Id, ReportId = document.ReportId, CategoryId = document.CategoryId,
                Report = report, Category = category, Status = document.Status,
                PublishedAtUtc = Utc(document.PublishedAtUtc), UpdatedAtUtc = Utc(document.UpdatedAtUtc)
            };
            category.AddListing(listing);
            _listings.Add(listing);
        }
    }

    private static UserDocument ToDocument(User user) => new()
    {
        Id = user.Id, Name = user.Name, Email = user.Email, PasswordHash = user.PasswordHash,
        Status = user.Status, Role = user.Role
    };

    private static ReportDocument ToDocument(Report report) => new()
    {
        Id = report.Id, UserId = report.UserId, CategoryId = report.CategoryId, Type = report.Type,
        Title = report.Title, Description = report.Description, Location = report.Location,
        IncidentDateUtc = report.IncidentDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
        ImageUrl = report.ImageUrl, Status = report.Status, RejectionReason = report.RejectionReason,
        ReviewedByUserId = report.ReviewedByUserId, ReviewedAtUtc = report.ReviewedAtUtc?.UtcDateTime,
        CreatedAtUtc = report.CreatedAtUtc.UtcDateTime, UpdatedAtUtc = report.UpdatedAtUtc.UtcDateTime
    };

    private static ListingDocument ToDocument(Listing listing) => new()
    {
        Id = listing.Id, ReportId = listing.ReportId, CategoryId = listing.CategoryId,
        Status = listing.Status, PublishedAtUtc = listing.PublishedAtUtc.UtcDateTime,
        UpdatedAtUtc = listing.UpdatedAtUtc.UtcDateTime
    };

    private static Report FromDocument(ReportDocument document) => new()
    {
        Id = document.Id, UserId = document.UserId, CategoryId = document.CategoryId,
        Type = document.Type, Title = document.Title, Description = document.Description,
        Location = document.Location, IncidentDate = DateOnly.FromDateTime(document.IncidentDateUtc),
        ImageUrl = document.ImageUrl, Status = document.Status,
        RejectionReason = document.RejectionReason, ReviewedByUserId = document.ReviewedByUserId,
        ReviewedAtUtc = document.ReviewedAtUtc is null ? null : Utc(document.ReviewedAtUtc.Value),
        CreatedAtUtc = Utc(document.CreatedAtUtc), UpdatedAtUtc = Utc(document.UpdatedAtUtc)
    };

    private static CategoryDocument Category(string name, string description) => new()
    {
        Id = Guid.NewGuid(), Name = name, Description = description
    };

    private static DateTimeOffset Utc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static string DefaultDatabasePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FoundIt", "foundit.db");

    private static void ValidateRegistration(string name, string email, string password)
    {
        if (name.Length < 2) throw new ArgumentException("Nama minimal terdiri dari 2 karakter.");
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
            throw new ArgumentException("Format email tidak valid.");
        if (password.Length < 8 || !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            throw new ArgumentException("Password minimal 8 karakter dan memiliki huruf besar, huruf kecil, serta angka.");
    }
}

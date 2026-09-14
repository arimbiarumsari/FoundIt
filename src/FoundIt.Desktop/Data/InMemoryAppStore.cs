using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;
using FoundIt.Api.Services;
using FoundIt.Desktop.Security;

namespace FoundIt.Desktop.Data;

public sealed class InMemoryAppStore
{
    private readonly List<User> _users = [];
    private readonly List<Report> _reports = [];
    private readonly List<Listing> _listings = [];
    private readonly List<Category> _categories = [];
    private readonly ReportService _reportService =
        new(new InMemoryReportRepository());

    public InMemoryAppStore()
    {
        SeedData();
    }

    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<Listing> Listings => _listings;

    public User Register(string name, string email, string password)
    {
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

    public Listing Approve(Admin admin, Report report)
    {
        var category = _categories.Single(category => category.Id == report.CategoryId);
        var listing = admin.VerifyReport(report, category, DateTimeOffset.UtcNow);
        _listings.Add(listing);
        return listing;
    }

    public void Reject(Admin admin, Report report, string reason) =>
        report.Reject(admin.Id.ToString(), reason, DateTimeOffset.UtcNow);

    private void SeedData()
    {
        var electronics = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Elektronik",
            Description = "Telepon, laptop, charger, dan perangkat elektronik lainnya."
        };
        _categories.AddRange([
            electronics,
            new Category { Id = Guid.NewGuid(), Name = "Dokumen", Description = "Kartu identitas dan dokumen." },
            new Category { Id = Guid.NewGuid(), Name = "Tas", Description = "Tas, dompet, dan wadah barang." },
            new Category { Id = Guid.NewGuid(), Name = "Kunci", Description = "Kunci kendaraan, kamar, dan lainnya." },
            new Category { Id = Guid.NewGuid(), Name = "Lainnya", Description = "Barang di luar kategori utama." }
        ]);

        var adminPassword = Environment.GetEnvironmentVariable("FOUNDIT_ADMIN_PASSWORD");
        if (!string.IsNullOrWhiteSpace(adminPassword))
        {
            _users.Add(new Admin
            {
                Id = Guid.NewGuid(),
                Name = "Administrator FoundIt",
                Email = "admin@foundit.local",
                PasswordHash = PasswordHasher.Hash(adminPassword)
            });
        }

        SeedPublicListing(electronics);
    }

    private void SeedPublicListing(Category category)
    {
        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = "system",
            CategoryId = category.Id,
            Type = ReportType.Found,
            Title = "Botol minum biru",
            Description = "Botol minum ditemukan di ruang kelas Teknik Elektro.",
            Location = "Gedung DTETI",
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

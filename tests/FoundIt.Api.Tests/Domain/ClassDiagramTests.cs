using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.Tests.Domain;

public sealed class ClassDiagramTests
{
    [Fact]
    public void VerifyReport_ApprovesReportAndCreatesActiveListing()
    {
        var now = new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero);
        var category = CreateCategory();
        var report = CreateReport(category.Id);
        var admin = CreateAdmin();

        var listing = admin.VerifyReport(report, category, now);

        Assert.Equal(ReportStatus.Approved, report.Status);
        Assert.Equal(admin.Id.ToString(), report.ReviewedByUserId);
        Assert.Equal(ListingStatus.Active, listing.Status);
        Assert.Equal(report.Id, listing.ReportId);
        Assert.Contains(listing, category.Listings);
    }

    [Fact]
    public void User_CannotDeleteAnotherUsersListing()
    {
        var now = new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero);
        var category = CreateCategory();
        var report = CreateReport(category.Id);
        var listing = CreateAdmin().VerifyReport(report, category, now);
        var anotherUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Other User",
            Email = "other@example.test",
            PasswordHash = "hashed-password"
        };

        Assert.Throws<UnauthorizedAccessException>(
            () => anotherUser.DeleteListing(listing, now));
    }

    [Fact]
    public void VerifyReport_CannotReviewApprovedReportTwice()
    {
        var now = new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero);
        var category = CreateCategory();
        var report = CreateReport(category.Id);
        var admin = CreateAdmin();
        admin.VerifyReport(report, category, now);

        Assert.Throws<InvalidOperationException>(
            () => admin.VerifyReport(report, category, now));
    }

    private static Admin CreateAdmin() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Administrator",
        Email = "admin@example.test",
        PasswordHash = "hashed-password"
    };

    private static Category CreateCategory() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Electronics",
        Description = "Electronic devices"
    };

    private static Report CreateReport(Guid categoryId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid().ToString(),
        CategoryId = categoryId,
        Type = ReportType.Lost,
        Title = "Lost phone",
        Description = "A black phone was lost near the library.",
        Location = "Library",
        IncidentDate = new DateOnly(2026, 9, 12),
        Status = ReportStatus.Pending,
        CreatedAtUtc = new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero),
        UpdatedAtUtc = new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.Zero)
    };
}

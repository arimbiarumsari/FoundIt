using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.Domain.Entities;

// Item details remain on Report. Listing only represents public publication.
public sealed class Listing
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public Guid CategoryId { get; init; }
    public required Report Report { get; init; }
    public required Category Category { get; init; }
    public ListingStatus Status { get; set; }
    public DateTimeOffset PublishedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string Title => Report.Title;
    public string Description => Report.Description;
    public string Location => Report.Location;
    public DateOnly IncidentDate => Report.IncidentDate;
    public string? ImageUrl => Report.ImageUrl;

    public static Listing Publish(Report report, Category category, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(category);

        if (report.Status != ReportStatus.Approved)
        {
            throw new InvalidOperationException("Only approved reports can become listings.");
        }

        if (report.CategoryId != category.Id)
        {
            throw new InvalidOperationException("The report and listing category must match.");
        }

        return new Listing
        {
            Id = Guid.NewGuid(),
            ReportId = report.Id,
            CategoryId = category.Id,
            Report = report,
            Category = category,
            Status = ListingStatus.Active,
            PublishedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    public void UpdateStatus(ListingStatus status, DateTimeOffset nowUtc)
    {
        if (Status == ListingStatus.Deleted)
        {
            throw new InvalidOperationException("A deleted listing cannot be updated.");
        }

        Status = status;
        UpdatedAtUtc = nowUtc;
    }

    public void Delete(DateTimeOffset nowUtc) =>
        UpdateStatus(ListingStatus.Deleted, nowUtc);

    public string ViewListing() => $"{Title} - {Location} ({IncidentDate:yyyy-MM-dd})";
}

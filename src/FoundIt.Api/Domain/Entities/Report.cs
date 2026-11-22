using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.Domain.Entities;

public sealed class Report
{
    public Guid Id { get; init; }
    public required string UserId { get; init; }
    public Guid CategoryId { get; init; }
    public ReportType Type { get; init; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public DateOnly IncidentDate { get; set; }
    public string? ImageUrl { get; set; }
    public ReportStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public void Update(
        string title,
        string description,
        string location,
        DateOnly incidentDate,
        DateTimeOffset nowUtc)
    {
        EnsurePending();
        Title = RequireText(title, nameof(title));
        Description = RequireText(description, nameof(description));
        Location = RequireText(location, nameof(location));
        IncidentDate = incidentDate;
        UpdatedAtUtc = nowUtc;
    }

    public void Delete(DateTimeOffset nowUtc)
    {
        EnsurePending();
        Status = ReportStatus.Cancelled;
        UpdatedAtUtc = nowUtc;
    }

    public void Approve(string adminUserId, DateTimeOffset nowUtc)
    {
        Review(ReportStatus.Approved, adminUserId, null, nowUtc);
    }

    public void Reject(string adminUserId, string reason, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A rejection reason is required.", nameof(reason));
        }

        Review(ReportStatus.Rejected, adminUserId, reason.Trim(), nowUtc);
    }

    private void Review(
        ReportStatus result,
        string adminUserId,
        string? rejectionReason,
        DateTimeOffset nowUtc)
    {
        EnsurePending();
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            throw new ArgumentException("An administrator ID is required.", nameof(adminUserId));
        }

        Status = result;
        RejectionReason = rejectionReason;
        ReviewedByUserId = adminUserId;
        ReviewedAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    private void EnsurePending()
    {
        if (Status != ReportStatus.Pending)
        {
            throw new InvalidOperationException("Only pending reports can be changed or reviewed.");
        }
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value is required.", parameterName);
        }

        return value.Trim();
    }
}

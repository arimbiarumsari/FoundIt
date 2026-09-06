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
}

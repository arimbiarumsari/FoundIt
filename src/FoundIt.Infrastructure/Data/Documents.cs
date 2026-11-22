using FoundIt.Api.Domain.Enums;
using LiteDB;

namespace FoundIt.Infrastructure.Data;

internal sealed class UserDocument
{
    [BsonId] public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
}

internal sealed class CategoryDocument
{
    [BsonId] public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

internal sealed class ReportDocument
{
    [BsonId] public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ReportType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime IncidentDateUtc { get; set; }
    public string? ImageUrl { get; set; }
    public ReportStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

internal sealed class ListingDocument
{
    [BsonId] public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid CategoryId { get; set; }
    public ListingStatus Status { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

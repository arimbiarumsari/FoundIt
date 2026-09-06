using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.Domain.Entities;

// Item details remain on Report. Listing only represents public publication.
public sealed class Listing
{
    public Guid Id { get; init; }
    public Guid ReportId { get; init; }
    public ListingStatus Status { get; set; }
    public DateTimeOffset PublishedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}

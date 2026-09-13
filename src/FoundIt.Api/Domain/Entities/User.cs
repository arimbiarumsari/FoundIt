using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.Domain.Entities;

public class User
{
    private readonly List<Report> _reports = [];

    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Email { get; init; }

    // This stores only a secure hash produced by the authentication system.
    public required string PasswordHash { get; init; }

    public UserStatus Status { get; internal set; } = UserStatus.Verified;
    public UserRole Role { get; protected init; } = UserRole.User;
    public IReadOnlyCollection<Report> Reports => _reports.AsReadOnly();

    public void SubmitReport(Report report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (Status != UserStatus.Verified)
        {
            throw new InvalidOperationException("Only verified users can submit reports.");
        }

        if (report.UserId != Id.ToString())
        {
            throw new InvalidOperationException("The report must belong to this user.");
        }

        _reports.Add(report);
    }

    public IEnumerable<Listing> SearchListings(IEnumerable<Listing> listings, string? keyword)
    {
        var activeListings = listings.Where(listing => listing.Status == ListingStatus.Active);
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return activeListings;
        }

        return activeListings.Where(listing =>
            listing.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            listing.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            listing.Location.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public Listing? ViewListing(IEnumerable<Listing> listings, Guid listingId) =>
        listings.SingleOrDefault(listing =>
            listing.Id == listingId && listing.Status == ListingStatus.Active);

    public void UpdateListing(Listing listing, ListingStatus status, DateTimeOffset nowUtc)
    {
        EnsureListingOwnership(listing);
        listing.UpdateStatus(status, nowUtc);
    }

    public void DeleteListing(Listing listing, DateTimeOffset nowUtc)
    {
        EnsureListingOwnership(listing);
        listing.Delete(nowUtc);
    }

    private void EnsureListingOwnership(Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);
        if (listing.Report.UserId != Id.ToString())
        {
            throw new UnauthorizedAccessException("Users can manage only their own listings.");
        }
    }
}

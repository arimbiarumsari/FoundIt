using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.Domain.Entities;

// An administrator uses the same user identity and password system, with an Admin role.
public sealed class Admin : User
{
    public Admin()
    {
        Role = UserRole.Admin;
    }

    public void VerifyUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        user.Status = UserStatus.Verified;
    }

    public Listing VerifyReport(
        Report report,
        Category category,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(category);

        report.Approve(Id.ToString(), nowUtc);
        var listing = Listing.Publish(report, category, nowUtc);
        category.AddListing(listing);
        return listing;
    }

    public Listing AddListing(Report report, Category category, DateTimeOffset nowUtc) =>
        VerifyReport(report, category, nowUtc);

    public new void UpdateListing(Listing listing, ListingStatus status, DateTimeOffset nowUtc) =>
        listing.UpdateStatus(status, nowUtc);

    public new void DeleteListing(Listing listing, DateTimeOffset nowUtc) =>
        listing.Delete(nowUtc);
}

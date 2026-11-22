namespace FoundIt.Api.Domain.Entities;

public sealed class Category
{
    private readonly List<Listing> _listings = [];

    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public IReadOnlyCollection<Listing> Listings => _listings.AsReadOnly();

    public void AddListing(Listing listing)
    {
        ArgumentNullException.ThrowIfNull(listing);
        if (listing.CategoryId != Id)
        {
            throw new InvalidOperationException("The listing belongs to another category.");
        }

        _listings.Add(listing);
    }
}

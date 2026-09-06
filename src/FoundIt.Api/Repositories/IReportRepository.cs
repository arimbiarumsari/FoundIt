using FoundIt.Api.Domain.Entities;

namespace FoundIt.Api.Repositories;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);
    Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

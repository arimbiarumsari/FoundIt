using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Repositories;

namespace FoundIt.Data;

// Versi PostgreSQL dari IReportRepository. ReportService tidak berubah,
// cukup diberi repository ini supaya laporan tersimpan di database.
public sealed class PostgresReportRepository(FoundItDatabase database) : IReportRepository
{
    public Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        database.InsertReport(report);
        return Task.CompletedTask;
    }

    public Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(database.FindReport(id));
    }
}

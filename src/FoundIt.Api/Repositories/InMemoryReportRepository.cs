using System.Collections.Concurrent;
using FoundIt.Api.Domain.Entities;

namespace FoundIt.Api.Repositories;

public sealed class InMemoryReportRepository : IReportRepository
{
    private readonly ConcurrentDictionary<Guid, Report> _reports = new();

    public Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_reports.TryAdd(report.Id, report))
        {
            throw new InvalidOperationException("A report with this ID already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Report?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _reports.TryGetValue(id, out var report);
        return Task.FromResult(report);
    }
}

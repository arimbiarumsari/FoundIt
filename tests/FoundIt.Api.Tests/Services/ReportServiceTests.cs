using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;
using FoundIt.Api.Services;

namespace FoundIt.Api.Tests.Services;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPendingReportOwnedByUser()
    {
        var repository = new InMemoryReportRepository();
        var service = new ReportService(repository);
        var now = new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero);
        var request = new CreateReportRequest(
            Guid.NewGuid(), ReportType.Lost, "Blue wallet",
            "A blue leather wallet containing student cards.",
            "Engineering Faculty", new DateOnly(2026, 9, 5), null);

        var report = await service.CreateAsync("student-1", request, now);

        Assert.Equal(ReportStatus.Pending, report.Status);
        Assert.Equal("student-1", report.UserId);
        Assert.Equal(now, report.CreatedAtUtc);
        Assert.NotNull(await repository.GetByIdAsync(report.Id));
    }

    [Fact]
    public async Task CreateAsync_RejectsUnreasonableFutureIncidentDate()
    {
        var service = new ReportService(new InMemoryReportRepository());
        var now = new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero);
        var request = new CreateReportRequest(
            Guid.NewGuid(), ReportType.Found, "Found phone",
            "A black phone found near the library entrance.",
            "Library", new DateOnly(2026, 9, 9), null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync("student-1", request, now));
    }
}

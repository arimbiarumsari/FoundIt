using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;
using FoundIt.Api.Services;

namespace FoundIt.Api.Tests.Services;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task CreateAsync_EmptyTitle_ReturnsIndonesianMessageWithoutParameterSuffix()
    {
        var repository = new InMemoryReportRepository();
        var service = new ReportService(repository);
        var request = new CreateReportRequest(Guid.NewGuid(), ReportType.Lost, "",
            "Dompet coklat berisi kartu mahasiswa.", "Kantin Teknik",
            new DateOnly(2026, 9, 5), null);

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            "student-1", request, new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("Judul wajib diisi.", error.Message);
    }
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

    [Fact]
    public async Task CreateAsync_StoresValidImage() {
        var service = new ReportService(new InMemoryReportRepository());
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var request = new CreateReportRequest(Guid.NewGuid(), ReportType.Lost, "Dompet coklat",
            "Dompet coklat berisi KTM dan uang tunai.", "Kantin Teknik",
            new DateOnly(2026, 9, 5), null, png);

        var report = await service.CreateAsync("student-1", request,
            new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal(png, report.ImageData);
    }

    [Fact]
    public async Task CreateAsync_RejectsNonImageFile() {
        var service = new ReportService(new InMemoryReportRepository());
        var request = new CreateReportRequest(Guid.NewGuid(), ReportType.Lost, "Dompet coklat",
            "Dompet coklat berisi KTM dan uang tunai.", "Kantin Teknik",
            new DateOnly(2026, 9, 5), null, new byte[] { 1, 2, 3, 4, 5, 6 });

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync("student-1", request,
            new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task CreateAsync_RejectsImageLargerThan2Mb() {
        var service = new ReportService(new InMemoryReportRepository());
        var big = new byte[2 * 1024 * 1024 + 1];
        big[0] = 0xFF; big[1] = 0xD8; big[2] = 0xFF; big[3] = 0xE0;
        var request = new CreateReportRequest(Guid.NewGuid(), ReportType.Lost, "Dompet coklat",
            "Dompet coklat berisi KTM dan uang tunai.", "Kantin Teknik",
            new DateOnly(2026, 9, 5), null, big);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync("student-1", request,
            new DateTimeOffset(2026, 9, 6, 10, 0, 0, TimeSpan.Zero)));
    }
}

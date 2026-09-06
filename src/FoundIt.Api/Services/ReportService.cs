using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;

namespace FoundIt.Api.Services;

public sealed class ReportService(IReportRepository reports)
{
    public async Task<Report> CreateAsync(
        string userId,
        CreateReportRequest request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("A user ID is required.", nameof(userId));
        }

        if (request.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("A category ID is required.", nameof(request));
        }

        if (!Enum.IsDefined(request.Type))
        {
            throw new ArgumentException("Report type must be Lost or Found.", nameof(request));
        }

        if (request.IncidentDate > DateOnly.FromDateTime(nowUtc.UtcDateTime.AddDays(1)))
        {
            throw new ArgumentException("Incident date cannot be more than one day in the future.");
        }

        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = request.Location.Trim(),
            IncidentDate = request.IncidentDate,
            ImageUrl = request.ImageUrl,
            Status = ReportStatus.Pending,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };

        await reports.AddAsync(report, cancellationToken);
        return report;
    }
}

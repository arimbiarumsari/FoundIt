using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Api.Repositories;
using System.ComponentModel.DataAnnotations;

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
            throw new ArgumentException("Pengguna wajib diisi.");
        }

        if (request.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("Kategori wajib dipilih.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            throw new ArgumentException("Jenis laporan harus Lost atau Found.");
        }

        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true))
        {
            throw new ArgumentException(validationResults[0].ErrorMessage);
        }

        if (request.IncidentDate > DateOnly.FromDateTime(nowUtc.UtcDateTime.AddDays(1)))
        {
            throw new ArgumentException("Tanggal kejadian tidak boleh lebih dari besok.");
        }

        const int maxImageBytes = 2 * 1024 * 1024;
        if (request.ImageData is { Length: > maxImageBytes }) {
            throw new ArgumentException("Ukuran gambar maksimal 2 MB.");
        }
        if (request.ImageData is { Length: > 0 } && !LooksLikeImage(request.ImageData)) {
            throw new ArgumentException("File harus berupa gambar PNG atau JPG.");
        }

        var report = new Report
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl,
            ImageData = request.ImageData is { Length: > 0 } ? request.ImageData : null,
            Location = request.Location.Trim(),
            IncidentDate = request.IncidentDate,
            Status = ReportStatus.Pending,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };

        await reports.AddAsync(report, cancellationToken);
        return report;
    }

    // Cek tanda pengenal file (bukan ekstensi): PNG = 89 50 4E 47, JPEG = FF D8 FF
    private static bool LooksLikeImage(byte[] d) =>
        d.Length > 4 &&
        ((d[0] == 0x89 && d[1] == 0x50 && d[2] == 0x4E && d[3] == 0x47) ||
         (d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF));
}

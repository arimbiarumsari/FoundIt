using System.ComponentModel.DataAnnotations;
using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.DTOs.Reports;

public sealed record CreateReportRequest(
    [property: Required] Guid CategoryId,
    [property: EnumDataType(typeof(ReportType), ErrorMessage = "Jenis laporan harus Lost atau Found.")] ReportType Type,
    [property: Required(ErrorMessage = "Judul wajib diisi."), StringLength(120, MinimumLength = 3, ErrorMessage = "Judul harus terdiri dari 3 sampai 120 karakter.")] string Title,
    [property: Required(ErrorMessage = "Deskripsi wajib diisi."), StringLength(2_000, MinimumLength = 10, ErrorMessage = "Deskripsi harus terdiri dari 10 sampai 2000 karakter.")] string Description,
    [property: Required(ErrorMessage = "Lokasi wajib diisi."), StringLength(200, ErrorMessage = "Lokasi maksimal 200 karakter.")] string Location,
    [property: Required] DateOnly IncidentDate,
    [property: Url(ErrorMessage = "Alamat gambar harus berupa URL yang valid."), StringLength(500, ErrorMessage = "Alamat gambar maksimal 500 karakter.")] string? ImageUrl,
    byte[]? ImageData = null);

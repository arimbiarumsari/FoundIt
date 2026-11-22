using System.ComponentModel.DataAnnotations;
using FoundIt.Api.Domain.Enums;

namespace FoundIt.Api.DTOs.Reports;

public sealed record CreateReportRequest(
    [property: Required] Guid CategoryId,
    [property: EnumDataType(typeof(ReportType))] ReportType Type,
    [property: Required, StringLength(120, MinimumLength = 3)] string Title,
    [property: Required, StringLength(2_000, MinimumLength = 10)] string Description,
    [property: Required, StringLength(200)] string Location,
    [property: Required] DateOnly IncidentDate,
    [property: Url, StringLength(500)] string? ImageUrl);

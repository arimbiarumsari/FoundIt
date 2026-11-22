using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Data;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Masukkan email dan password akun FoundIt."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("basic", document)] = []
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var store = CreateStore();

app.MapGet("/", () => Results.Redirect("/swagger"));

// ---------- Publik ----------
app.MapGet("/api/listings", (string? keyword) =>
    store.SearchListings(keyword).Select(ToDto))
   .WithSummary("Cari listing aktif (opsional ?keyword=)");

app.MapGet("/api/listings/{id:guid}", (Guid id) =>
    store.GetPublicListing(id) is { } listing ? Results.Ok(ToDto(listing)) : Results.NotFound())
   .WithSummary("Detail satu listing");

app.MapGet("/api/categories", () =>
    store.Categories.Select(c => new { c.Id, c.Name, c.Description }))
   .WithSummary("Daftar kategori barang");

app.MapGet("/api/stats", () => new {
    TotalListings = store.AllListings.Count(),
    Active = store.AllListings.Count(l => l.Status == ListingStatus.Active),
    Resolved = store.AllListings.Count(l => l.Status == ListingStatus.Resolved),
    Lost = store.AllListings.Count(l => l.Report.Type == ReportType.Lost),
    Found = store.AllListings.Count(l => l.Report.Type == ReportType.Found),
    Categories = store.Categories.Select(c => new {
        c.Name,
        Count = store.AllListings.Count(l => l.CategoryId == c.Id)
    })
}).WithSummary("Statistik ringkas listing");

app.MapPost("/api/auth/register", (RegisterRequest body) => {
    try {
        var user = store.Register(body.Name, body.Email, body.Password);
        return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Name, user.Email });
    }
    catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    catch (InvalidOperationException e) { return Results.Conflict(new { error = e.Message }); }
}).WithSummary("Daftar akun baru");

// ---------- Perlu login (HTTP Basic: Authorization: Basic base64(email:password)) ----------
app.MapPost("/api/reports", async (HttpRequest http, CreateReportRequest body) => {
    var user = Authenticate(http);
    if (user is null) return Results.Unauthorized();
    try {
        var report = await store.SubmitReportAsync(user, body);
        return Results.Created($"/api/reports/{report.Id}",
            new { report.Id, Status = report.Status.ToString() });
    }
    catch (ArgumentException e) { return Results.BadRequest(new { error = e.Message }); }
    catch (InvalidOperationException e) { return Results.BadRequest(new { error = e.Message }); }
}).WithSummary("Kirim laporan barang hilang/ditemukan (perlu Basic Auth)");

app.MapGet("/api/reports/mine", (HttpRequest http) => {
    var user = Authenticate(http);
    if (user is null) return Results.Unauthorized();
    return Results.Ok(store.GetReportsFor(user).Select(r => new {
        r.Id,
        r.Title,
        r.Location,
        r.IncidentDate,
        Type = r.Type.ToString(),
        Status = r.Status.ToString(),
        r.RejectionReason
    }));
}).WithSummary("Laporan milik saya (perlu Basic Auth)");

app.MapGet("/api/listings/{id:guid}/image", (Guid id) => {
    var data = store.GetPublicListing(id)?.ImageData;
    if (data is not { Length: > 0 }) return Results.NotFound();
    return Results.File(data, data[0] == 0x89 ? "image/png" : "image/jpeg");
}).WithSummary("Foto listing");

app.Run();

// ---------- Fungsi bantu ----------
InMemoryAppStore CreateStore() {
    var connectionString = DatabaseSettings.GetConnectionString();
    if (connectionString is null) return new InMemoryAppStore();   // data sementara
    var database = new FoundItDatabase(connectionString);
    database.EnsureCreated();
    return new InMemoryAppStore(database);
}

User? Authenticate(HttpRequest request) {
    var header = request.Headers.Authorization.ToString();
    if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) return null;
    try {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..].Trim()));
        var parts = decoded.Split(':', 2);
        return parts.Length == 2 ? store.Login(parts[0], parts[1]) : null;
    }
    catch (FormatException) { return null; }
}

// Jangan kembalikan entity langsung: Category.Listings membuat JSON melingkar.
object ToDto(Listing l) => new {
    l.Id,
    l.Title,
    ReportedBy = store.GetUserName(l.Report.UserId),
    HasImage = l.ImageData is { Length: > 0 },
    l.Description,
    l.Location,
    l.IncidentDate,
    l.ImageUrl,
    Type = l.Report.Type.ToString(),
    Category = l.Category.Name,
    Status = l.Status.ToString(),
    l.PublishedAtUtc
};

public record RegisterRequest(string? Name, string? Email, string? Password);

public partial class Program { }   // dibutuhkan untuk test API (opsional)

using FoundIt.Api.Repositories;
using FoundIt.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSingleton<IReportRepository, InMemoryReportRepository>();
builder.Services.AddScoped<ReportService>();

var app = builder.Build();

app.UseExceptionHandler();
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestampUtc = DateTimeOffset.UtcNow
}));
app.MapControllers();

app.Run();

public partial class Program
{
}

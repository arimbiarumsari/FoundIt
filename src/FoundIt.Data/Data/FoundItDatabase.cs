using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using Npgsql;
using NpgsqlTypes;

namespace FoundIt.Data;

// Semua akses ke PostgreSQL ada di class ini (Npgsql + query SQL).
// Error dari database dibungkus jadi InvalidOperationException supaya form
// cukup menangkap exception yang sama seperti error validasi lainnya.
public sealed class FoundItDatabase
{
    private const string ReportColumns =
        "id, user_id, category_id, type, title, description, location, incident_date, " +
        "image_url, status, rejection_reason, reviewed_by, reviewed_at, created_at, updated_at, image_data";

    private readonly string _connectionString;

    public FoundItDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Membuat database (kalau belum ada) lalu menjalankan schema.sql.
    public void EnsureCreated()
    {
        Run(() =>
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.Database;
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("Connection string harus memuat Database=...");
            }

            builder.Database = "postgres";
            using (var server = new NpgsqlConnection(builder.ConnectionString))
            {
                server.Open();
                using var check = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", server);
                check.Parameters.AddWithValue("name", databaseName);
                if (check.ExecuteScalar() is null)
                {
                    // Nama database tidak bisa jadi parameter, jadi diberi tanda kutip sendiri.
                    var quoted = "\"" + databaseName.Replace("\"", "\"\"") + "\"";
                    using var create = new NpgsqlCommand($"CREATE DATABASE {quoted}", server);
                    create.ExecuteNonQuery();
                }
            }
            var assembly = typeof(FoundItDatabase).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(n => n.EndsWith("schema.sql", StringComparison.OrdinalIgnoreCase));
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var schemaSql = reader.ReadToEnd();

            using var connection = Open();
            using var schema = new NpgsqlCommand(schemaSql, connection);
            schema.ExecuteNonQuery();
        });
    }

    // ---------- Category ----------

    public List<Category> LoadCategories() => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand("SELECT id, name, description FROM categories ORDER BY name", connection);
        using var reader = command.ExecuteReader();
        var result = new List<Category>();
        while (reader.Read())
        {
            result.Add(new Category
            {
                Id = reader.GetGuid(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2)
            });
        }

        return result;
    });

    public void InsertCategory(Category category) => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            "INSERT INTO categories (id, name, description) VALUES (@id, @name, @description)", connection);
        Add(command, "id", NpgsqlDbType.Uuid, category.Id);
        Add(command, "name", NpgsqlDbType.Varchar, category.Name);
        Add(command, "description", NpgsqlDbType.Text, category.Description);
        command.ExecuteNonQuery();
    });

    // ---------- User dan Admin ----------

    public List<User> LoadUsers() => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            "SELECT id, name, email, password_hash, status, role FROM users ORDER BY name", connection);
        using var reader = command.ExecuteReader();
        var result = new List<User>();
        while (reader.Read())
        {
            // Status User hanya bisa diubah dari dalam domain, jadi akun yang belum Verified
            // (Pending atau Suspended) tidak dimuat. Efeknya sama: akun itu tidak bisa login.
            if (Enum.Parse<UserStatus>(reader.GetString(4)) != UserStatus.Verified)
            {
                continue;
            }

            if (Enum.Parse<UserRole>(reader.GetString(5)) == UserRole.Admin)
            {
                result.Add(new Admin
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3)
                });
            }
            else
            {
                result.Add(new User
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3)
                });
            }
        }

        return result;
    });

    public void InsertUser(User user) => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            "INSERT INTO users (id, name, email, password_hash, status, role) " +
            "VALUES (@id, @name, @email, @password_hash, @status, @role)", connection);
        Add(command, "id", NpgsqlDbType.Uuid, user.Id);
        Add(command, "name", NpgsqlDbType.Varchar, user.Name);
        Add(command, "email", NpgsqlDbType.Varchar, user.Email);
        Add(command, "password_hash", NpgsqlDbType.Varchar, user.PasswordHash);
        Add(command, "status", NpgsqlDbType.Varchar, user.Status.ToString());
        Add(command, "role", NpgsqlDbType.Varchar, user.Role.ToString());
        command.ExecuteNonQuery();
    });

    // ---------- Report ----------

    public List<Report> LoadReports() => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            $"SELECT {ReportColumns} FROM reports ORDER BY created_at", connection);
        using var reader = command.ExecuteReader();
        var result = new List<Report>();
        while (reader.Read())
        {
            result.Add(ReadReport(reader));
        }

        return result;
    });

    public Report? FindReport(Guid id) => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            $"SELECT {ReportColumns} FROM reports WHERE id = @id", connection);
        Add(command, "id", NpgsqlDbType.Uuid, id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadReport(reader) : null;
    });

    public void InsertReport(Report report) => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            "INSERT INTO reports (" + ReportColumns + ") VALUES (" +
            "@id, @user_id, @category_id, @type, @title, @description, @location, @incident_date, " +
            "@image_url, @status, @rejection_reason, @reviewed_by, @reviewed_at, @created_at, @updated_at, @image_data)",
            connection);
        BindReport(command, report, includeImmutable: true);
        command.ExecuteNonQuery();
    });

    public void UpdateReport(Report report) => Run(() =>
    {
        using var connection = Open();
        UpdateReport(connection, null, report);
    });

    // ---------- Listing ----------

    public List<Listing> LoadListings(
        IReadOnlyCollection<Report> reports,
        IReadOnlyCollection<Category> categories) => Run(() =>
    {
        var reportById = reports.ToDictionary(report => report.Id);
        var categoryById = categories.ToDictionary(category => category.Id);

        using var connection = Open();
        using var command = new NpgsqlCommand(
            "SELECT id, report_id, category_id, status, published_at, updated_at " +
            "FROM listings ORDER BY published_at", connection);
        using var reader = command.ExecuteReader();
        var result = new List<Listing>();
        while (reader.Read())
        {
            if (!reportById.TryGetValue(reader.GetGuid(1), out var report) ||
                !categoryById.TryGetValue(reader.GetGuid(2), out var category))
            {
                continue;
            }

            result.Add(new Listing
            {
                Id = reader.GetGuid(0),
                ReportId = report.Id,
                CategoryId = category.Id,
                Report = report,
                Category = category,
                Status = Enum.Parse<ListingStatus>(reader.GetString(3)),
                PublishedAtUtc = ReadTime(reader, 4),
                UpdatedAtUtc = ReadTime(reader, 5)
            });
        }

        return result;
    });

    public void UpdateListing(Listing listing) => Run(() =>
    {
        using var connection = Open();
        using var command = new NpgsqlCommand(
            "UPDATE listings SET status = @status, updated_at = @updated_at WHERE id = @id", connection);
        Add(command, "id", NpgsqlDbType.Uuid, listing.Id);
        Add(command, "status", NpgsqlDbType.Varchar, listing.Status.ToString());
        Add(command, "updated_at", NpgsqlDbType.TimestampTz, listing.UpdatedAtUtc.UtcDateTime);
        command.ExecuteNonQuery();
    });

    // Persetujuan admin: report diperbarui dan listing dibuat dalam satu transaksi,
    // jadi keduanya berhasil bersama atau dibatalkan bersama.
    public void ApproveReport(Report report, Listing listing) => Run(() =>
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();

        UpdateReport(connection, transaction, report);

        using var command = new NpgsqlCommand(
            "INSERT INTO listings (id, report_id, category_id, status, published_at, updated_at) " +
            "VALUES (@id, @report_id, @category_id, @status, @published_at, @updated_at)",
            connection, transaction);
        Add(command, "id", NpgsqlDbType.Uuid, listing.Id);
        Add(command, "report_id", NpgsqlDbType.Uuid, listing.ReportId);
        Add(command, "category_id", NpgsqlDbType.Uuid, listing.CategoryId);
        Add(command, "status", NpgsqlDbType.Varchar, listing.Status.ToString());
        Add(command, "published_at", NpgsqlDbType.TimestampTz, listing.PublishedAtUtc.UtcDateTime);
        Add(command, "updated_at", NpgsqlDbType.TimestampTz, listing.UpdatedAtUtc.UtcDateTime);
        command.ExecuteNonQuery();

        transaction.Commit();
    });

    // ---------- Helper ----------

    private NpgsqlConnection Open()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static void UpdateReport(NpgsqlConnection connection, NpgsqlTransaction? transaction, Report report)
    {
        using var command = new NpgsqlCommand(
            "UPDATE reports SET title = @title, description = @description, location = @location, " +
            "incident_date = @incident_date, image_url = @image_url, status = @status, " +
            "rejection_reason = @rejection_reason, reviewed_by = @reviewed_by, " +
            "reviewed_at = @reviewed_at, updated_at = @updated_at WHERE id = @id",
            connection, transaction);
        BindReport(command, report, includeImmutable: false);
        Add(command, "id", NpgsqlDbType.Uuid, report.Id);
        command.ExecuteNonQuery();
    }

    private static void BindReport(NpgsqlCommand command, Report report, bool includeImmutable)
    {
        if (includeImmutable)
        {
            Add(command, "id", NpgsqlDbType.Uuid, report.Id);
            Add(command, "user_id", NpgsqlDbType.Uuid, Guid.Parse(report.UserId));
            Add(command, "category_id", NpgsqlDbType.Uuid, report.CategoryId);
            Add(command, "type", NpgsqlDbType.Varchar, report.Type.ToString());
            Add(command, "created_at", NpgsqlDbType.TimestampTz, report.CreatedAtUtc.UtcDateTime);
            Add(command, "image_data", NpgsqlDbType.Bytea, report.ImageData);
        }

        Add(command, "title", NpgsqlDbType.Varchar, report.Title);
        Add(command, "description", NpgsqlDbType.Text, report.Description);
        Add(command, "location", NpgsqlDbType.Varchar, report.Location);
        Add(command, "incident_date", NpgsqlDbType.Date, report.IncidentDate);
        Add(command, "image_url", NpgsqlDbType.Varchar, report.ImageUrl);
        Add(command, "status", NpgsqlDbType.Varchar, report.Status.ToString());
        Add(command, "rejection_reason", NpgsqlDbType.Text, report.RejectionReason);
        Add(command, "reviewed_by", NpgsqlDbType.Uuid,
            report.ReviewedByUserId is null ? (object?)null : Guid.Parse(report.ReviewedByUserId));
        Add(command, "reviewed_at", NpgsqlDbType.TimestampTz, report.ReviewedAtUtc?.UtcDateTime);
        Add(command, "updated_at", NpgsqlDbType.TimestampTz, report.UpdatedAtUtc.UtcDateTime);
    }

    private static Report ReadReport(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        UserId = reader.GetGuid(1).ToString(),
        CategoryId = reader.GetGuid(2),
        Type = Enum.Parse<ReportType>(reader.GetString(3)),
        Title = reader.GetString(4),
        Description = reader.GetString(5),
        Location = reader.GetString(6),
        IncidentDate = reader.GetFieldValue<DateOnly>(7),
        ImageUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
        Status = Enum.Parse<ReportStatus>(reader.GetString(9)),
        RejectionReason = reader.IsDBNull(10) ? null : reader.GetString(10),
        ReviewedByUserId = reader.IsDBNull(11) ? null : reader.GetGuid(11).ToString(),
        ReviewedAtUtc = reader.IsDBNull(12) ? null : ReadTime(reader, 12),
        CreatedAtUtc = ReadTime(reader, 13),
        UpdatedAtUtc = ReadTime(reader, 14),
        ImageData = reader.IsDBNull(15) ? null : reader.GetFieldValue<byte[]>(15)
    };

    private static DateTimeOffset ReadTime(NpgsqlDataReader reader, int index) =>
        new(DateTime.SpecifyKind(reader.GetDateTime(index), DateTimeKind.Utc));

    private static void Add(NpgsqlCommand command, string name, NpgsqlDbType type, object? value)
    {
        command.Parameters.Add(new NpgsqlParameter(name, type) { Value = value ?? DBNull.Value });
    }

    private static T Run<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (NpgsqlException exception)
        {
            throw new InvalidOperationException(
                "Gagal mengakses database PostgreSQL: " + exception.Message, exception);
        }
    }

    private static void Run(Action action) => Run<object?>(() =>
    {
        action();
        return null;
    });
}

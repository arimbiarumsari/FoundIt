using System.Text.Json;

namespace FoundIt.Data;

// Urutan pencarian:
//   1) environment variable (FOUNDIT_DB / FOUNDIT_ADMIN_PASSWORD)
//   2) appsettings.json di samping exe
//   3) (khusus connection string) %LOCALAPPDATA%\FoundIt\connection.txt
public static class DatabaseSettings {
    private sealed class Settings {
        public string? ConnectionString { get; set; }
        public string? AdminPassword { get; set; }
    }

    public static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FoundIt", "connection.txt");

    private static Settings LoadFile() {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path)) return new Settings();
        try {
            return JsonSerializer.Deserialize<Settings>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Settings();
        }
        catch (JsonException) {
            return new Settings();
        }
    }

    public static string? GetConnectionString() {
        var fromEnv = Environment.GetEnvironmentVariable("FOUNDIT_DB");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv.Trim();

        var fromJson = LoadFile().ConnectionString;
        if (!string.IsNullOrWhiteSpace(fromJson)) return fromJson.Trim();

        if (!File.Exists(ConfigPath)) return null;
        var fromFile = File.ReadAllText(ConfigPath).Trim();
        return fromFile.Length == 0 ? null : fromFile;
    }

    public static string? GetAdminPassword() {
        var fromEnv = Environment.GetEnvironmentVariable("FOUNDIT_ADMIN_PASSWORD");
        if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

        var fromJson = LoadFile().AdminPassword;
        return string.IsNullOrWhiteSpace(fromJson) ? null : fromJson;
    }
}
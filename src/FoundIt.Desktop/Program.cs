using FoundIt.Data;
using FoundIt.Desktop.Forms;

namespace FoundIt.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm(CreateStore()));
    }

    // Pakai PostgreSQL kalau connection string tersedia dan database bisa dihubungi.
    // Kalau tidak, aplikasi tetap jalan dengan data sementara di memori.
    private static InMemoryAppStore CreateStore()
    {
        var connectionString = DatabaseSettings.GetConnectionString();
        if (connectionString is null)
        {
            MessageBox.Show(
                "Koneksi PostgreSQL belum diatur, jadi aplikasi memakai data sementara.\n\n" +
                "Isi connection string di:\n" + DatabaseSettings.ConfigPath,
                "FoundIt", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return new InMemoryAppStore();
        }

        try
        {
            var database = new FoundItDatabase(connectionString);
            database.EnsureCreated();
            return new InMemoryAppStore(database);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                "Tidak bisa terhubung ke PostgreSQL, jadi aplikasi memakai data sementara.\n\n" +
                exception.Message,
                "FoundIt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return new InMemoryAppStore();
        }
    }
}

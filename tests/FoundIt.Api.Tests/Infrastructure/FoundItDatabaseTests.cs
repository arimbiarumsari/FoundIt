using FoundIt.Infrastructure.Data;

namespace FoundIt.Api.Tests.Infrastructure;

public sealed class FoundItDatabaseTests
{
    [Fact]
    public void Register_PersistsUserAcrossDatabaseSessions()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"foundit-{Guid.NewGuid()}.db");
        try
        {
            using (var database = new FoundItDatabase(databasePath))
            {
                database.Register("Bintang", "bintang@example.test", "Password1");
            }

            using var reopened = new FoundItDatabase(databasePath);
            var user = reopened.Login("bintang@example.test", "Password1");

            Assert.NotNull(user);
            Assert.Equal("Bintang", user.Name);
        }
        finally
        {
            if (File.Exists(databasePath)) File.Delete(databasePath);
            if (File.Exists(databasePath + "-log")) File.Delete(databasePath + "-log");
        }
    }

    [Fact]
    public void Register_PreventsDuplicateEmail()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"foundit-{Guid.NewGuid()}.db");
        try
        {
            using var database = new FoundItDatabase(databasePath);
            database.Register("User Satu", "user@example.test", "Password1");

            Assert.Throws<InvalidOperationException>(() =>
                database.Register("User Dua", "user@example.test", "Password2"));
        }
        finally
        {
            if (File.Exists(databasePath)) File.Delete(databasePath);
            if (File.Exists(databasePath + "-log")) File.Delete(databasePath + "-log");
        }
    }
}

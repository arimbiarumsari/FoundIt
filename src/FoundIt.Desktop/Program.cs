using FoundIt.Desktop.Forms;
using FoundIt.Infrastructure.Data;

namespace FoundIt.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var store = new FoundItDatabase();
        Application.Run(new LoginForm(store));
    }
}

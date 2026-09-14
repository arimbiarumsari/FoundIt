using FoundIt.Desktop.Data;
using FoundIt.Desktop.Forms;

namespace FoundIt.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var store = new InMemoryAppStore();
        Application.Run(new LoginForm(store));
    }
}

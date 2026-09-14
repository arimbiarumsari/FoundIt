using FoundIt.Desktop.Data;

namespace FoundIt.Desktop.Forms;

public sealed class LoginForm : Form
{
    private readonly InMemoryAppStore _store;
    private readonly TextBox _email = new() { Width = 300 };
    private readonly TextBox _password = new() { Width = 300, UseSystemPasswordChar = true };

    public LoginForm(InMemoryAppStore store)
    {
        _store = store;
        Text = "FoundIt - Login";
        ClientSize = new Size(900, 560);
        MinimumSize = new Size(760, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;

        var card = new TableLayoutPanel
        {
            AutoSize = true,
            Padding = new Padding(42),
            BackColor = Color.White,
            ColumnCount = 1,
            Anchor = AnchorStyles.None
        };
        card.Controls.Add(new Label
        {
            Text = "FoundIt",
            AutoSize = true,
            ForeColor = UiTheme.Primary,
            Font = new Font("Segoe UI", 30, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 4)
        });
        card.Controls.Add(new Label
        {
            Text = "Temukan kembali barangmu di lingkungan FT",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 0, 0, 24)
        });
        card.Controls.Add(FieldLabel("Email"));
        card.Controls.Add(_email);
        card.Controls.Add(FieldLabel("Password"));
        card.Controls.Add(_password);

        var actions = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 24, 0, 0) };
        var login = UiTheme.Button("Masuk");
        var register = UiTheme.Button("Daftar", false);
        login.Click += Login_Click;
        register.Click += (_, _) => new RegisterForm(_store).ShowDialog(this);
        actions.Controls.Add(login);
        actions.Controls.Add(register);
        card.Controls.Add(actions);

        var host = new TableLayoutPanel { Dock = DockStyle.Fill };
        host.Controls.Add(card, 0, 0);
        Controls.Add(host);
        AcceptButton = login;
    }

    private void Login_Click(object? sender, EventArgs e)
    {
        var user = _store.Login(_email.Text, _password.Text);
        if (user is null)
        {
            MessageBox.Show("Email atau password salah.", "Login gagal",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Hide();
        using var main = new MainForm(_store, user);
        main.ShowDialog(this);
        _password.Clear();
        Show();
    }

    private static Label FieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Margin = new Padding(0, 12, 0, 5)
    };
}

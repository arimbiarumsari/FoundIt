using FoundIt.Desktop.Data;

namespace FoundIt.Desktop.Forms;

public sealed class RegisterForm : Form
{
    private readonly InMemoryAppStore _store;
    private readonly TextBox _name = new() { Width = 320 };
    private readonly TextBox _email = new() { Width = 320 };
    private readonly TextBox _password = new() { Width = 320, UseSystemPasswordChar = true };

    public RegisterForm(InMemoryAppStore store)
    {
        _store = store;
        Text = "FoundIt - Registrasi";
        ClientSize = new Size(470, 470);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiTheme.Background;

        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(45),
            ColumnCount = 1
        };
        form.Controls.Add(UiTheme.Heading("Buat akun"));
        AddField(form, "Nama", _name);
        AddField(form, "Email", _email);
        AddField(form, "Password", _password);
        form.Controls.Add(new Label
        {
            Text = "Minimal 8 karakter, huruf besar, huruf kecil, dan angka.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 6, 0, 18)
        });
        var submit = UiTheme.Button("Daftar");
        submit.Click += Register_Click;
        form.Controls.Add(submit);
        Controls.Add(form);
        AcceptButton = submit;
    }

    private void Register_Click(object? sender, EventArgs e)
    {
        try
        {
            _store.Register(_name.Text, _email.Text, _password.Text);
            MessageBox.Show("Akun berhasil dibuat. Silakan login.", "Registrasi berhasil",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(exception.Message, "Registrasi gagal",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static void AddField(TableLayoutPanel form, string label, Control input)
    {
        form.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 10, 0, 5)
        });
        form.Controls.Add(input);
    }
}

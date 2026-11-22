namespace FoundIt.Desktop.Forms;

internal static class UiTheme
{
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryDark = Color.FromArgb(30, 64, 175);
    public static readonly Color Background = Color.FromArgb(244, 247, 252);
    public static readonly Color Text = Color.FromArgb(31, 41, 55);

    public static Button Button(string text, bool primary = true) => new()
    {
        Text = text,
        AutoSize = true,
        MinimumSize = new Size(110, 38),
        FlatStyle = FlatStyle.Flat,
        BackColor = primary ? Primary : Color.White,
        ForeColor = primary ? Color.White : PrimaryDark,
        Cursor = Cursors.Hand,
        Padding = new Padding(10, 3, 10, 3),
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
    };

    public static Label Heading(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = Text,
        Font = new Font("Segoe UI", 22, FontStyle.Bold),
        Margin = new Padding(0, 0, 0, 18)
    };

    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None,
        RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
}

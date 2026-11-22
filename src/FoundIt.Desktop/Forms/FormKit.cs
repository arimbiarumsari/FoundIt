using FoundIt.Api.Domain.Enums;

namespace FoundIt.Desktop.Forms;

// Helper kecil untuk form dialog (detail, review, kelola) supaya tampilannya seragam.
internal static class FormKit
{
    public static void AddPicture(FlowLayoutPanel body, byte[]? data, int width) {
        var image = ImageHelper.FromBytes(data);
        if (image is null) return;

        body.Controls.Add(new PictureBox {
            Image = image,
            SizeMode = PictureBoxSizeMode.Zoom,
            Width = width,
            Height = 260,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(0, 6, 0, 6)
        });
    }
    public static void Setup(Form form, string title, int width, int height)
    {
        form.Text = title;
        form.ClientSize = new Size(width, height);
        form.StartPosition = FormStartPosition.CenterParent;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.BackColor = UiTheme.Background;
        form.Font = new Font("Segoe UI", 10);
    }

    public static FlowLayoutPanel Body() => new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        Padding = new Padding(28, 20, 28, 10)
    };

    public static FlowLayoutPanel Actions(params Control[] buttons)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(20, 14, 20, 10),
            BackColor = Color.White
        };
        panel.Controls.AddRange(buttons);
        return panel;
    }

    public static Label Heading(string text, int width) => new()
    {
        Text = text,
        AutoSize = true,
        MaximumSize = new Size(width, 0),
        ForeColor = UiTheme.Text,
        Font = new Font("Segoe UI", 18, FontStyle.Bold),
        Margin = new Padding(0, 0, 0, 8)
    };

    public static void AddRow(FlowLayoutPanel body, string caption, string value, int width)
    {
        body.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            Margin = new Padding(0, 10, 0, 2)
        });
        body.Controls.Add(new Label
        {
            Text = value,
            AutoSize = true,
            MaximumSize = new Size(width, 0),
            ForeColor = UiTheme.Text,
            Margin = new Padding(0, 0, 0, 2)
        });
    }

    public static string TypeText(ReportType type) =>
        type == ReportType.Lost ? "Barang hilang" : "Barang ditemukan";

    public static string ReportStatusText(ReportStatus status) => status switch
    {
        ReportStatus.Pending => "Menunggu review admin",
        ReportStatus.Approved => "Disetujui",
        ReportStatus.Rejected => "Ditolak",
        ReportStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };

    public static string ListingStatusText(ListingStatus status) => status switch
    {
        ListingStatus.Active => "Aktif",
        ListingStatus.Resolved => "Selesai",
        ListingStatus.Archived => "Diarsipkan",
        ListingStatus.Deleted => "Dihapus",
        _ => status.ToString()
    };
}

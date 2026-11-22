using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Data;

namespace FoundIt.Desktop.Forms;

// Detail laporan milik user. Dibuka dari tab "Laporan Saya" (klik dua kali pada baris).
// Laporan yang masih Pending bisa dibatalkan dari sini.
public sealed class ReportDetailForm : Form
{
    public ReportDetailForm(InMemoryAppStore store, Report report)
    {
        const int width = 460;
        FormKit.Setup(this, "FoundIt - Detail Laporan", 520, 600);

        var categoryName = store.Categories.FirstOrDefault(c => c.Id == report.CategoryId)?.Name ?? "-";

        var body = FormKit.Body();
        body.Controls.Add(FormKit.Heading(report.Title, width));
        FormKit.AddRow(body, "Jenis laporan", FormKit.TypeText(report.Type), width);
        FormKit.AddRow(body, "Kategori", categoryName, width);
        FormKit.AddRow(body, "Lokasi kejadian", report.Location, width);
        FormKit.AddRow(body, "Tanggal kejadian", report.IncidentDate.ToString("dd MMM yyyy"), width);
        FormKit.AddRow(body, "Deskripsi", report.Description, width);
        FormKit.AddPicture(body, report.ImageData, width);
        FormKit.AddRow(body, "Status laporan", FormKit.ReportStatusText(report.Status), width);
        if (!string.IsNullOrWhiteSpace(report.RejectionReason))
        {
            FormKit.AddRow(body, "Alasan penolakan", report.RejectionReason, width);
        }
        FormKit.AddRow(body, "Dikirim",
            report.CreatedAtUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm"), width);

        var close = UiTheme.Button("Tutup", false);
        close.Click += (_, _) => Close();
        var buttons = new List<Control> { close };

        if (report.Status == ReportStatus.Pending)
        {
            var cancel = UiTheme.Button("Batalkan laporan");
            cancel.Click += (_, _) =>
            {
                var answer = MessageBox.Show("Batalkan laporan ini?", "Konfirmasi",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;

                try
                {
                    store.CancelReport(report);
                    DialogResult = DialogResult.OK;
                }
                catch (InvalidOperationException exception)
                {
                    MessageBox.Show(exception.Message, "Gagal membatalkan",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            buttons.Insert(0, cancel);
        }

        Controls.Add(body);
        Controls.Add(FormKit.Actions(buttons.ToArray()));
        CancelButton = close;
    }
}

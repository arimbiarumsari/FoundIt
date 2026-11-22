using FoundIt.Api.Domain.Entities;
using FoundIt.Data;

namespace FoundIt.Desktop.Forms;

// Form review laporan khusus admin. Dibuka dari tab "Review Admin" (klik dua kali pada baris).
public sealed class ReviewReportForm : Form
{
    private readonly TextBox _reason = new()
    {
        Width = 460,
        Height = 70,
        Multiline = true,
        PlaceholderText = "Alasan penolakan (wajib diisi kalau laporan ditolak)"
    };

    public ReviewReportForm(InMemoryAppStore store, Admin admin, Report report)
    {
        const int width = 460;
        FormKit.Setup(this, "FoundIt - Review Laporan", 520, 700);

        var categoryName = store.Categories.FirstOrDefault(c => c.Id == report.CategoryId)?.Name ?? "-";

        var body = FormKit.Body();
        body.Controls.Add(FormKit.Heading(report.Title, width));
        FormKit.AddRow(body, "Pelapor", store.GetUserName(report.UserId), width);
        FormKit.AddRow(body, "Jenis laporan", FormKit.TypeText(report.Type), width);
        FormKit.AddRow(body, "Kategori", categoryName, width);
        FormKit.AddRow(body, "Lokasi kejadian", report.Location, width);
        FormKit.AddRow(body, "Tanggal kejadian", report.IncidentDate.ToString("dd MMM yyyy"), width);
        FormKit.AddRow(body, "Deskripsi", report.Description, width);
        FormKit.AddRow(body, "Dikirim",
            report.CreatedAtUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm"), width);
        FormKit.AddRow(body, "Alasan penolakan", "", width);
        body.Controls.Add(_reason);

        var approve = UiTheme.Button("Setujui");
        var reject = UiTheme.Button("Tolak", false);
        var cancel = UiTheme.Button("Batal", false);

        approve.Click += (_, _) => Finish(() => store.Approve(admin, report));
        reject.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_reason.Text))
            {
                MessageBox.Show("Isi alasan penolakan terlebih dahulu.", "Alasan kosong",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Finish(() => store.Reject(admin, report, _reason.Text));
        };
        cancel.Click += (_, _) => Close();

        Controls.Add(body);
        Controls.Add(FormKit.Actions(approve, reject, cancel));
        CancelButton = cancel;
    }

    private void Finish(Action action)
    {
        try
        {
            action();
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(exception.Message, "Review gagal",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

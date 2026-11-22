using FoundIt.Api.Domain.Entities;

namespace FoundIt.Desktop.Forms;

// Detail satu listing publik. Dibuka dari tab "Cari Listing" (klik dua kali pada baris).
public sealed class ListingDetailForm : Form
{
    public ListingDetailForm(Listing listing)
    {
        const int width = 460;
        FormKit.Setup(this, "FoundIt - Detail Listing", 520, 620);

        var body = FormKit.Body();
        body.Controls.Add(FormKit.Heading(listing.Title, width));
        FormKit.AddRow(body, "Jenis", FormKit.TypeText(listing.Report.Type), width);
        FormKit.AddRow(body, "Kategori", listing.Category.Name, width);
        FormKit.AddRow(body, "Lokasi", listing.Location, width);
        FormKit.AddRow(body, "Tanggal kejadian", listing.IncidentDate.ToString("dd MMM yyyy"), width);
        FormKit.AddRow(body, "Deskripsi", listing.Description, width);
        FormKit.AddRow(body, "Status listing", FormKit.ListingStatusText(listing.Status), width);
        FormKit.AddRow(body, "Dipublikasikan",
            listing.PublishedAtUtc.ToLocalTime().ToString("dd MMM yyyy HH:mm"), width);

        var close = UiTheme.Button("Tutup");
        close.Click += (_, _) => Close();

        Controls.Add(body);
        Controls.Add(FormKit.Actions(close));
        AcceptButton = close;
        CancelButton = close;
    }
}

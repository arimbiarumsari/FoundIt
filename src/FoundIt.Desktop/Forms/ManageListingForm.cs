using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Data;

namespace FoundIt.Desktop.Forms;

// Kelola listing khusus admin (activity "Admin manage-listing"): ubah status atau hapus listing.
// Dibuka dari tombol "Kelola Listing" di header MainForm.
public sealed class ManageListingForm : Form
{
    private readonly InMemoryAppStore _store;
    private readonly Admin _admin;
    private readonly DataGridView _grid = UiTheme.Grid();
    private readonly ComboBox _status = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

    public ManageListingForm(InMemoryAppStore store, Admin admin)
    {
        _store = store;
        _admin = admin;
        FormKit.Setup(this, "FoundIt - Kelola Listing", 900, 520);

        _status.DataSource = new[] { ListingStatus.Active, ListingStatus.Resolved, ListingStatus.Archived };

        var heading = new Label
        {
            Text = "Kelola Listing",
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(24, 14, 0, 0),
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = UiTheme.Text
        };

        var update = UiTheme.Button("Ubah status");
        var delete = UiTheme.Button("Hapus listing", false);
        var close = UiTheme.Button("Tutup", false);
        update.Click += (_, _) => Apply(deleteListing: false);
        delete.Click += (_, _) => Apply(deleteListing: true);
        close.Click += (_, _) => Close();

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            Padding = new Padding(20, 14, 20, 10),
            BackColor = Color.White
        };
        actions.Controls.Add(new Label
        {
            Text = "Status baru:",
            AutoSize = true,
            Margin = new Padding(0, 10, 8, 0)
        });
        actions.Controls.Add(_status);
        actions.Controls.Add(update);
        actions.Controls.Add(delete);
        actions.Controls.Add(close);

        Controls.Add(_grid);
        Controls.Add(heading);
        Controls.Add(actions);
        CancelButton = close;

        LoadListings();
    }

    private void LoadListings()
    {
        _grid.DataSource = _store.AllListings.Select(listing => new
        {
            listing.Id,
            Judul = listing.Title,
            Jenis = FormKit.TypeText(listing.Report.Type),
            Kategori = listing.Category.Name,
            Lokasi = listing.Location,
            Status = FormKit.ListingStatusText(listing.Status)
        }).ToList();

        if (_grid.Columns.Contains("Id")) _grid.Columns["Id"].Visible = false;
    }

    private void Apply(bool deleteListing)
    {
        if (_grid.CurrentRow?.Cells["Id"].Value is not Guid id)
        {
            MessageBox.Show("Pilih listing terlebih dahulu.");
            return;
        }

        var listing = _store.GetListing(id);
        if (listing is null) return;

        try
        {
            if (deleteListing)
            {
                var answer = MessageBox.Show("Hapus listing ini? Listing tidak akan tampil lagi di pencarian.",
                    "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;
                _store.DeleteListing(_admin, listing);
            }
            else
            {
                _store.SetListingStatus(_admin, listing, (ListingStatus)_status.SelectedItem!);
            }

            LoadListings();
        }
        catch (InvalidOperationException exception)
        {
            MessageBox.Show(exception.Message, "Gagal memperbarui listing",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

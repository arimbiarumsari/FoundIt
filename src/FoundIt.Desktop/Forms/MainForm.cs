using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Desktop.Data;

namespace FoundIt.Desktop.Forms;

public sealed class MainForm : Form
{
    private readonly InMemoryAppStore _store;
    private readonly User _currentUser;
    private readonly DataGridView _listingGrid = UiTheme.Grid();
    private readonly DataGridView _myReportsGrid = UiTheme.Grid();
    private readonly DataGridView _pendingGrid = UiTheme.Grid();
    private readonly TextBox _search = new() { Width = 320, PlaceholderText = "Cari barang atau lokasi..." };
    private readonly TextBox _title = new() { Width = 360 };
    private readonly TextBox _description = new() { Width = 360, Multiline = true, Height = 75 };
    private readonly TextBox _location = new() { Width = 360 };
    private readonly ComboBox _category = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _type = new() { Width = 360, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker _incidentDate = new() { Width = 360, MaxDate = DateTime.Today.AddDays(1) };
    private readonly TextBox _rejectionReason = new() { Width = 300, PlaceholderText = "Alasan penolakan" };

    public MainForm(InMemoryAppStore store, User currentUser)
    {
        _store = store;
        _currentUser = currentUser;
        Text = "FoundIt - Aplikasi Lost & Found";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1050, 680);
        BackColor = UiTheme.Background;

        var header = BuildHeader();
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
            Padding = new Point(18, 8)
        };
        tabs.TabPages.Add(BuildListingsTab());
        tabs.TabPages.Add(BuildSubmitTab());
        tabs.TabPages.Add(BuildMyReportsTab());
        if (currentUser is Admin admin)
        {
            tabs.TabPages.Add(BuildAdminTab(admin));
        }

        Controls.Add(tabs);
        Controls.Add(header);
        RefreshAllData();
    }

    private Control BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = UiTheme.PrimaryDark };
        header.Controls.Add(new Label
        {
            Text = "FoundIt",
            AutoSize = true,
            Location = new Point(28, 19),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 25, FontStyle.Bold)
        });
        header.Controls.Add(new Label
        {
            Text = $"Halo, {_currentUser.Name}  •  {_currentUser.Role}",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(760, 31),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11)
        });
        return header;
    }

    private TabPage BuildListingsTab()
    {
        var page = NewPage("Cari Listing");
        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 64,
            Padding = new Padding(20, 13, 20, 8)
        };
        var searchButton = UiTheme.Button("Cari");
        searchButton.Click += (_, _) => RefreshListings();
        _search.KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.KeyCode == Keys.Enter) RefreshListings();
        };
        top.Controls.Add(_search);
        top.Controls.Add(searchButton);
        page.Controls.Add(_listingGrid);
        page.Controls.Add(top);
        return page;
    }

    private TabPage BuildSubmitTab()
    {
        var page = NewPage("Buat Laporan");
        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 500,
            AutoScroll = true,
            Padding = new Padding(32),
            ColumnCount = 1
        };
        form.Controls.Add(UiTheme.Heading("Laporkan barang"));

        _type.DataSource = Enum.GetValues<ReportType>();
        _category.DataSource = _store.Categories.ToList();
        _category.DisplayMember = nameof(Category.Name);
        _category.ValueMember = nameof(Category.Id);

        AddField(form, "Jenis laporan", _type);
        AddField(form, "Kategori", _category);
        AddField(form, "Judul", _title);
        AddField(form, "Deskripsi", _description);
        AddField(form, "Lokasi kejadian", _location);
        AddField(form, "Tanggal kejadian", _incidentDate);

        var submit = UiTheme.Button("Kirim laporan");
        submit.Margin = new Padding(0, 22, 0, 0);
        submit.Click += SubmitReport_Click;
        form.Controls.Add(submit);
        page.Controls.Add(form);
        return page;
    }

    private TabPage BuildMyReportsTab()
    {
        var page = NewPage("Laporan Saya");
        var refresh = UiTheme.Button("Muat ulang", false);
        refresh.Dock = DockStyle.Top;
        refresh.Click += (_, _) => RefreshMyReports();
        page.Controls.Add(_myReportsGrid);
        page.Controls.Add(refresh);
        return page;
    }

    private TabPage BuildAdminTab(Admin admin)
    {
        var page = NewPage("Review Admin");
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 66,
            Padding = new Padding(16, 12, 16, 8)
        };
        var approve = UiTheme.Button("Setujui");
        var reject = UiTheme.Button("Tolak", false);
        approve.Click += (_, _) => ReviewSelected(admin, approve: true);
        reject.Click += (_, _) => ReviewSelected(admin, approve: false);
        actions.Controls.Add(approve);
        actions.Controls.Add(_rejectionReason);
        actions.Controls.Add(reject);
        page.Controls.Add(_pendingGrid);
        page.Controls.Add(actions);
        return page;
    }

    private async void SubmitReport_Click(object? sender, EventArgs e)
    {
        try
        {
            var request = new CreateReportRequest(
                (Guid)_category.SelectedValue,
                (ReportType)_type.SelectedItem!,
                _title.Text,
                _description.Text,
                _location.Text,
                DateOnly.FromDateTime(_incidentDate.Value),
                null);

            await _store.SubmitReportAsync(_currentUser, request);
            MessageBox.Show("Laporan berhasil dikirim dan menunggu review admin.",
                "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _title.Clear();
            _description.Clear();
            _location.Clear();
            RefreshAllData();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(exception.Message, "Laporan tidak valid",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ReviewSelected(Admin admin, bool approve)
    {
        if (_pendingGrid.CurrentRow?.Cells["Id"].Value is not Guid reportId)
        {
            MessageBox.Show("Pilih laporan terlebih dahulu.");
            return;
        }

        var report = _store.GetPendingReports().SingleOrDefault(item => item.Id == reportId);
        if (report is null) return;

        try
        {
            if (approve)
            {
                _store.Approve(admin, report);
            }
            else
            {
                _store.Reject(admin, report, _rejectionReason.Text);
                _rejectionReason.Clear();
            }

            RefreshAllData();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(exception.Message, "Review gagal",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RefreshAllData()
    {
        RefreshListings();
        RefreshMyReports();
        RefreshPendingReports();
    }

    private void RefreshListings()
    {
        _listingGrid.DataSource = _store.SearchListings(_search.Text).Select(listing => new
        {
            listing.Id,
            Jenis = listing.Report.Type,
            listing.Title,
            Kategori = listing.Category.Name,
            listing.Location,
            Tanggal = listing.IncidentDate,
            Status = listing.Status
        }).ToList();
        HideIdColumn(_listingGrid);
    }

    private void RefreshMyReports()
    {
        _myReportsGrid.DataSource = _store.GetReportsFor(_currentUser).Select(report => new
        {
            report.Id,
            Jenis = report.Type,
            report.Title,
            report.Location,
            Tanggal = report.IncidentDate,
            Status = report.Status,
            Alasan = report.RejectionReason
        }).ToList();
        HideIdColumn(_myReportsGrid);
    }

    private void RefreshPendingReports()
    {
        if (_currentUser is not Admin) return;
        _pendingGrid.DataSource = _store.GetPendingReports().Select(report => new
        {
            report.Id,
            Jenis = report.Type,
            report.Title,
            report.Description,
            report.Location,
            Tanggal = report.IncidentDate
        }).ToList();
        HideIdColumn(_pendingGrid);
    }

    private static TabPage NewPage(string title) => new(title)
    {
        BackColor = UiTheme.Background,
        Padding = new Padding(10)
    };

    private static void AddField(TableLayoutPanel form, string label, Control input)
    {
        form.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 8, 0, 5)
        });
        form.Controls.Add(input);
    }

    private static void HideIdColumn(DataGridView grid)
    {
        if (grid.Columns.Contains("Id")) grid.Columns["Id"].Visible = false;
    }
}

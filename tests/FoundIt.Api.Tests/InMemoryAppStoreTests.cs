using FoundIt.Api.Domain.Entities;
using FoundIt.Api.Domain.Enums;
using FoundIt.Api.DTOs.Reports;
using FoundIt.Data;

namespace FoundIt.Api.Tests.Data;

public sealed class InMemoryAppStoreTests {
    private const string ValidPassword = "Passw0rd1";

    private static (InMemoryAppStore Store, Admin Admin) NewStoreWithAdmin() {
        Environment.SetEnvironmentVariable("FOUNDIT_ADMIN_PASSWORD", "Admin12345");
        var store = new InMemoryAppStore();
        var admin = Assert.IsType<Admin>(store.Login("admin@foundit.local", "Admin12345"));
        return (store, admin);
    }

    [Fact]
    public void Register_ValidData_CreatesVerifiedUserWithHashedPassword() {
        var store = new InMemoryAppStore();
        var user = store.Register("Budi", "budi@example.com", ValidPassword);

        Assert.Equal(UserStatus.Verified, user.Status);
        Assert.NotEqual(ValidPassword, user.PasswordHash);
    }

    [Theory]
    [InlineData("abc")]            // terlalu pendek
    [InlineData("passw0rd1")]      // tanpa huruf besar
    [InlineData("PASSW0RD1")]      // tanpa huruf kecil
    [InlineData("Password")]       // tanpa angka
    public void Register_WeakPassword_Throws(string password) {
        var store = new InMemoryAppStore();
        Assert.Throws<ArgumentException>(
            () => store.Register("Budi", "budi@example.com", password));
    }

    [Fact]
    public void Register_DuplicateEmail_Throws() {
        var store = new InMemoryAppStore();
        store.Register("Budi", "budi@example.com", ValidPassword);

        Assert.Throws<InvalidOperationException>(
            () => store.Register("Budi 2", "BUDI@example.com", ValidPassword));
    }

    [Fact]
    public void Login_CorrectAndWrongPassword() {
        var store = new InMemoryAppStore();
        store.Register("Budi", "budi@example.com", ValidPassword);

        Assert.NotNull(store.Login("budi@example.com", ValidPassword));
        Assert.Null(store.Login("budi@example.com", "SalahPass1"));
        Assert.Null(store.Login("tidakada@example.com", ValidPassword));
    }

    [Fact]
    public async Task SubmitReport_CreatesPendingReportListedInMyReports() {
        var store = new InMemoryAppStore();
        var user = store.Register("Budi", "budi@example.com", ValidPassword);
        var request = new CreateReportRequest(
            store.Categories[0].Id, ReportType.Lost, "Dompet coklat",
            "Dompet coklat berisi KTM dan uang tunai.", "Kantin Teknik",
            DateOnly.FromDateTime(DateTime.Today), null);

        var report = await store.SubmitReportAsync(user, request);

        Assert.Equal(ReportStatus.Pending, report.Status);
        Assert.Contains(store.GetReportsFor(user), r => r.Id == report.Id);
    }

    [Fact]
    public void Approve_PendingReport_CreatesActiveListing() {
        var (store, admin) = NewStoreWithAdmin();
        var report = store.GetPendingReports().First();

        var listing = store.Approve(admin, report);

        Assert.Equal(ReportStatus.Approved, report.Status);
        Assert.Equal(ListingStatus.Active, listing.Status);
        Assert.Contains(store.SearchListings(null), l => l.Id == listing.Id);
    }

    [Fact]
    public void Reject_PendingReport_StoresReasonAndDoesNotPublish() {
        var (store, admin) = NewStoreWithAdmin();
        var report = store.GetPendingReports().First();
        var before = store.SearchListings(null).Count();

        store.Reject(admin, report, "Informasi kurang jelas");

        Assert.Equal(ReportStatus.Rejected, report.Status);
        Assert.Equal("Informasi kurang jelas", report.RejectionReason);
        Assert.Equal(before, store.SearchListings(null).Count());
    }

    [Fact]
    public void SearchListings_ByKeyword_FindsSeedData() {
        var store = new InMemoryAppStore();
        Assert.NotEmpty(store.SearchListings("dompet"));
        Assert.Empty(store.SearchListings("zzzz-tidak-ada"));
    }
}
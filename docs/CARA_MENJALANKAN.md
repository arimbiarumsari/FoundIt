# Cara Menjalankan FoundIt dari Source (untuk Developer)

Pengguna biasa cukup mengunduh exe dari halaman **Releases** dan mengikuti [docs/PANDUAN_PENGGUNA.md](docs/PANDUAN_PENGGUNA.md). Dokumen ini untuk yang ingin membuka dan membangun source code.

## Yang dibutuhkan

- Windows 10 atau 11
- .NET 10 SDK (cek dengan `dotnet --version`, harus 10.x). Pasang: `winget install Microsoft.DotNet.SDK.10`
- Visual Studio 2022/2026 (opsional)
- Koneksi internet saat build pertama (paket NuGet diunduh otomatis)

## Konfigurasi

Salin `appsettings.example.json` menjadi `appsettings.json` pada project yang akan dijalankan (`src\FoundIt.Desktop` dan/atau `src\FoundIt.WebApi`), lalu isi:

```json
{
  "ConnectionString": "",
  "AdminPassword": "Admin12345"
}
```

- `ConnectionString` kosong berarti data sementara di memori (hilang saat aplikasi ditutup).
- `AdminPassword` adalah password akun `admin@foundit.local`. Pada build **Debug**, jika kosong, dipakai `Admin12345`. Pada build **Release**, admin hanya dibuat bila `AdminPassword` terisi.

Alternatif tanpa file: environment variable `FOUNDIT_DB` dan `FOUNDIT_ADMIN_PASSWORD`. Urutan pembacaan connection string: environment variable, lalu `appsettings.json`, lalu `%LOCALAPPDATA%\FoundIt\connection.txt`.

Jangan commit file yang berisi password asli.

## Menjalankan

Visual Studio: buka `FoundIt.slnx`, klik kanan project (`FoundIt.Desktop` atau `FoundIt.WebApi`) → **Set as Startup Project**, lalu tekan **F5**.

Terminal (di folder yang berisi `FoundIt.slnx`):

```powershell
dotnet run --project src\FoundIt.Desktop
dotnet run --project src\FoundIt.WebApi
```

Web API menampilkan alamatnya di konsol; buka `<alamat>/swagger`.

## Memakai PostgreSQL

1. Pasang PostgreSQL dan ingat password user `postgres`.
2. Isi `ConnectionString` di `appsettings.json`:
   `Host=localhost;Port=5432;Database=foundit;Username=postgres;Password=PASSWORDMU`
3. Jalankan aplikasi. Database `foundit` dan semua tabelnya (lihat `database\schema.sql`) dibuat otomatis.
4. Jika berhasil, header setelah login menampilkan **PostgreSQL**.

## Menjalankan test

```powershell
dotnet test
```

Atau di Visual Studio: **Test Explorer** → **Run All Tests**.

## Membuat exe

```powershell
dotnet publish src\FoundIt.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\FoundIt-Desktop
dotnet publish src\FoundIt.WebApi  -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\FoundIt-WebApi
```

Salin `appsettings.example.json` ke tiap folder hasil publish, lalu zip masing-masing folder untuk diunggah sebagai asset di GitHub Releases.

## Struktur singkat

- `src\FoundIt.Api` — domain (User, Admin, Report, Listing, Category) dan `ReportService`
- `src\FoundIt.Data` — penyimpanan (`InMemoryAppStore`, PostgreSQL, `schema.sql`) dan `PasswordHasher`
- `src\FoundIt.Desktop` — aplikasi Windows Forms (`Forms\` untuk tampilan)
- `src\FoundIt.WebApi` — layanan Web API
- `tests\FoundIt.Api.Tests` — unit test xUnit

# FoundIt

Aplikasi *Lost & Found* untuk mencari dan melaporkan barang hilang atau ditemukan di lingkungan Fakultas Teknik.

## Tim

| Peran | Nama | NIM |
|---|---|---|
| Ketua | Muhammad Bintang Hidayatullah Marbun | 24/544012/TK/60468 |
| Anggota 1 | Arimbi Arum Sari | 24/541867/TK/60129 |
| Anggota 2 | Aston Hugo | 24/538303/TK/59700 |

## Fitur

**Aplikasi Desktop (Windows Forms)**
- Registrasi dan login (password disimpan dengan hash PBKDF2, bukan teks biasa)
- Mencari listing barang berdasarkan kata kunci (nama barang atau lokasi)
- Mengirim laporan barang hilang atau ditemukan
- Melihat, membuka detail, dan membatalkan laporan milik sendiri
- Panel admin: menyetujui atau menolak laporan (dengan alasan), mengubah status dan menghapus listing
- Penyimpanan ke **PostgreSQL** (opsional). Tanpa database, aplikasi memakai data sementara di memori

**Web API**
- Mencari dan melihat listing, melihat kategori dan statistik
- Registrasi akun, mengirim laporan, dan melihat laporan sendiri lewat HTTP
- Dokumentasi interaktif dengan Swagger di `/swagger`

## Unduh dan Jalankan (paling cepat)

Tidak perlu memasang Visual Studio atau .NET.

1. Buka halaman **[Releases](../../releases)** repositori ini dan unduh:
   - `FoundIt-Desktop-v1.0.0-win-x64.zip`
   - `FoundIt-WebApi-v1.0.0-win-x64.zip` (opsional, jika ingin memakai API)
2. Ekstrak zip.
3. Di folder hasil ekstrak, salin `appsettings.example.json` menjadi `appsettings.json`, lalu isi (lihat bagian Konfigurasi).
4. Jalankan `FoundIt.Desktop.exe`.

> Windows SmartScreen dapat menampilkan peringatan karena exe belum ditandatangani. Klik **More info → Run anyway**.

Panduan lengkap penggunaan ada di **[docs/PANDUAN_PENGGUNA.md](docs/PANDUAN_PENGGUNA.md)**.

## Konfigurasi

Semua pengaturan ada di `appsettings.json` di samping exe:

```json
{
  "ConnectionString": "",
  "AdminPassword": "Admin12345"
}
```

| Kunci | Fungsi |
|---|---|
| `ConnectionString` | Koneksi PostgreSQL, misalnya `Host=localhost;Port=5432;Database=foundit;Username=postgres;Password=PASSWORDMU`. **Kosongkan** untuk memakai data sementara (hilang saat aplikasi ditutup). |
| `AdminPassword` | Password akun admin. Email admin selalu `admin@foundit.local`. |

Alternatifnya, pengaturan dapat diberikan lewat environment variable `FOUNDIT_DB` dan `FOUNDIT_ADMIN_PASSWORD`.

### Menyiapkan PostgreSQL

1. Pasang PostgreSQL dan catat password user `postgres`.
2. Isi `ConnectionString` di `appsettings.json`. Database `foundit` dan seluruh tabelnya dibuat otomatis saat aplikasi pertama kali dijalankan.
3. Setelah login, header aplikasi menampilkan **PostgreSQL**. Jika tertulis *Data sementara*, koneksi belum terpakai.

Skema tabel tersedia di [`database/schema.sql`](database/schema.sql) sebagai referensi.

> Akun demo (`AdminPassword` di atas) hanya untuk keperluan penilaian. Jangan memakai password asli di repositori publik.

## Web API

Jalankan `FoundIt.WebApi.exe`, lalu buka **http://localhost:5000/swagger**.
Jika port 5000 sedang dipakai: `FoundIt.WebApi.exe --urls http://localhost:5050`.

| Method | Endpoint | Login | Fungsi |
|---|---|---|---|
| GET | `/api/listings?keyword=` | – | Cari listing aktif |
| GET | `/api/listings/{id}` | – | Detail satu listing |
| GET | `/api/categories` | – | Daftar kategori |
| GET | `/api/stats` | – | Statistik ringkas listing |
| POST | `/api/auth/register` | – | Daftar akun baru |
| POST | `/api/reports` | Basic Auth | Kirim laporan barang |
| GET | `/api/reports/mine` | Basic Auth | Laporan milik saya |

Basic Auth memakai header `Authorization: Basic base64(email:password)`.

Contoh:

```powershell
curl http://localhost:5000/api/listings?keyword=dompet
curl http://localhost:5000/api/stats
curl -X POST http://localhost:5000/api/auth/register -H "Content-Type: application/json" `
     -d '{"name":"Budi","email":"budi@example.com","password":"Passw0rd1"}'
```

Contoh lengkap (termasuk kirim laporan) ada di [`src/FoundIt.WebApi/FoundIt.WebApi.http`](src/FoundIt.WebApi/FoundIt.WebApi.http).

## Class Diagram

<img width="520" height="2560" alt="Class diagram FoundIt" src="https://github.com/user-attachments/assets/0c820ad1-2638-4829-9815-7c88a1b70f51" />

Relasi yang diimplementasikan pada `FoundIt.Api/Domain`:

```text
User 1 -------- 0..* Report
Report 1 ------ 0..1 Listing
Category 1 ---- 0..* Listing
Admin --------- verifies User and Report
```

- `User` dapat mengirim laporan dan mencari listing
- `Admin` adalah user dengan role administrator yang memverifikasi laporan dan mengelola listing
- `Report` memiliki operasi update, pembatalan, persetujuan, dan penolakan
- `Listing` adalah publikasi dari report yang sudah disetujui
- `Category` memiliki kumpulan listing

## Struktur Repositori

```text
FoundIt/
├── FoundIt.slnx
├── src/
│   ├── FoundIt.Api/        Domain (User, Admin, Report, Listing, Category) dan ReportService
│   ├── FoundIt.Data/       Penyimpanan: InMemoryAppStore, PostgreSQL, PasswordHasher, schema.sql
│   ├── FoundIt.Desktop/    Aplikasi Windows Forms
│   └── FoundIt.WebApi/     Layanan Web API (ASP.NET Core minimal API)
├── tests/
│   └── FoundIt.Api.Tests/  Unit test xUnit
├── database/schema.sql     Skema PostgreSQL (referensi)
├── docs/
│   ├── PANDUAN_PENGGUNA.md
│   └── LAPORAN_PENGUJIAN.md
├── CARA_MENJALANKAN.md     Panduan build dari source (untuk developer)
└── README.md
```

## Build dari Source

Kebutuhan: Windows 10/11 dan .NET 10 SDK.

```powershell
dotnet build
dotnet test
dotnet run --project src\FoundIt.Desktop
dotnet run --project src\FoundIt.WebApi
```

Membuat exe siap pakai:

```powershell
dotnet publish src\FoundIt.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\FoundIt-Desktop
dotnet publish src\FoundIt.WebApi  -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\FoundIt-WebApi
```

Detail lebih lanjut ada di [docs/CARA_MENJALANKAN.md](docs/CARA_MENJALANKAN.md).

## Keterbatasan yang Diketahui

- Data dimuat ke memori saat aplikasi dimulai. Jika Desktop dan Web API berjalan bersamaan pada database yang sama, perubahan di salah satunya tidak otomatis muncul di yang lain sebelum dijalankan ulang.
- Web API memakai HTTP Basic Auth sederhana (cocok untuk demo, belum memakai token/JWT).

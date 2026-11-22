# Panduan Pengguna FoundIt

FoundIt membantu mahasiswa dan civitas Fakultas Teknik menemukan kembali barang yang hilang, serta melaporkan barang yang ditemukan.

## 1. Persyaratan

- Windows 10 atau 11 (64-bit)
- Tidak perlu memasang .NET atau Visual Studio
- PostgreSQL (opsional). Tanpa PostgreSQL, aplikasi tetap berjalan dengan data sementara yang hilang saat aplikasi ditutup

## 2. Instalasi dan Konfigurasi

### 2.1 Unduh

Unduh `FoundIt-Desktop-v1.0.0-win-x64.zip` dari halaman **Releases** di GitHub, lalu ekstrak ke folder mana pun.

Isi folder:

```text
FoundIt.Desktop.exe
appsettings.example.json
```

### 2.2 Konfigurasi

1. Salin `appsettings.example.json` lalu ganti namanya menjadi `appsettings.json`.
2. Buka dengan Notepad dan isi:

```json
{
  "ConnectionString": "",
  "AdminPassword": "Admin12345"
}
```

| Kunci | Penjelasan |
|---|---|
| `ConnectionString` | Kosongkan untuk data sementara. Isi untuk memakai PostgreSQL (lihat 2.3). |
| `AdminPassword` | Password untuk akun admin (`admin@foundit.local`). |

### 2.3 Memakai PostgreSQL (opsional)

1. Pasang PostgreSQL dan ingat password user `postgres`.
2. Isi `ConnectionString`:

```text
Host=localhost;Port=5432;Database=foundit;Username=postgres;Password=PASSWORDMU
```

3. Jalankan `FoundIt.Desktop.exe`. Database `foundit` beserta tabelnya dibuat otomatis.
4. Setelah login, bagian atas jendela menampilkan **PostgreSQL**. Jika tertulis *Data sementara*, aplikasi belum memakai database.

### 2.4 Menjalankan

Klik dua kali `FoundIt.Desktop.exe`. Jika muncul peringatan Windows SmartScreen, klik **More info → Run anyway**.

## 3. Aplikasi Desktop untuk Pengguna

### 3.1 Membuat akun

1. Di halaman login, klik **Daftar**.
2. Isi **Nama**, **Email**, dan **Password**. Password minimal 8 karakter dan memuat huruf besar, huruf kecil, serta angka (contoh: `Passw0rd1`).
3. Klik **Daftar**. Setelah muncul pesan *Akun berhasil dibuat*, kembali ke halaman login.

### 3.2 Masuk

Isi **Email** dan **Password**, lalu klik **Masuk**. Di bagian atas jendela tampil nama Anda, peran (User/Admin), dan jenis penyimpanan (PostgreSQL atau Data sementara). Klik **Keluar** untuk logout.

### 3.3 Mencari barang (tab *Cari Listing*)

1. Buka tab **Cari Listing**.
2. Ketik nama barang atau lokasi pada kolom pencarian, lalu klik **Cari**. Kosongkan kolom untuk menampilkan semua listing aktif.
3. Klik dua kali sebuah baris untuk melihat detail: jenis, kategori, lokasi, tanggal kejadian, deskripsi, status listing, dan waktu publikasi.
4. Tabel listing menampilkan kolom **Pelapor**. Pada jendela detail tampil **foto barang** (jika ada) dan **Dilaporkan oleh**.

### 3.4 Melaporkan barang (tab *Buat Laporan*)

1. Buka tab **Buat Laporan**.
2. Isi formulir:
   - **Jenis laporan**: hilang atau ditemukan
   - **Kategori**: pilih kategori barang
   - **Judul**: 3–120 karakter
   - **Deskripsi**: minimal 10 karakter (ciri barang, isi, warna, dan sebagainya)
   - **Lokasi kejadian**
   - **Tanggal kejadian**: tidak boleh lebih dari besok
   - **Foto barang (opsional)**: klik **Pilih foto...**, pilih file JPG atau PNG, dan pratinjau akan tampil. Klik **Hapus foto** untuk membatalkan. Foto otomatis diperkecil, dan file di atas 2 MB setelah diperkecil ditolak.
3. Klik **Kirim laporan**. Akan muncul pesan *Laporan berhasil dikirim dan menunggu review admin*.


Laporan baru berstatus **menunggu review** dan belum tampil di pencarian publik sampai admin menyetujuinya.

### 3.5 Melihat laporan saya (tab *Laporan Saya*)

1. Buka tab **Laporan Saya**. Klik **Muat ulang** untuk menyegarkan daftar.
2. Klik dua kali sebuah laporan untuk melihat detail dan statusnya. Jika laporan ditolak, alasan penolakan ditampilkan.
3. Untuk membatalkan laporan, klik **Batalkan laporan** pada jendela detail lalu konfirmasi.
4. Foto yang Anda unggah ikut tampil di jendela detail.

## 4. Aplikasi Desktop untuk Admin

Masuk dengan email `admin@foundit.local` dan password sesuai `AdminPassword` di `appsettings.json`. Akun admin memiliki tab tambahan dan tombol **Kelola Listing**.

### 4.1 Meninjau laporan (tab *Review Admin*)

1. Buka tab **Review Admin**. Daftar berisi laporan yang menunggu review.
2. Klik dua kali sebuah laporan untuk membuka jendela review (pelapor, jenis, kategori, lokasi, tanggal, deskripsi).
3. Pilih tindakan:
   - **Setujui**: laporan menjadi listing aktif dan muncul di pencarian publik.
   - **Tolak**: isi **Alasan penolakan** terlebih dahulu (wajib), lalu klik **Tolak**. Pelapor dapat melihat alasannya di *Laporan Saya*.
4. Tabel review menampilkan kolom **Pelapor**, dan jendela review menampilkan foto barang sehingga admin dapat memeriksanya sebelum menyetujui.

Tindakan yang sama juga tersedia langsung di tab melalui tombol **Setujui** dan **Tolak** setelah memilih laporan.

### 4.2 Mengelola listing (tombol *Kelola Listing*)

1. Klik **Kelola Listing** di bagian atas.
2. Pilih listing, lalu:
   - Pilih **Status baru** dan klik **Ubah status**, atau
   - Klik **Hapus listing** dan konfirmasi.

## 5. Menggunakan Web API

Web API adalah layanan tambahan agar data FoundIt dapat diakses dari aplikasi atau alat lain.

### 5.1 Menjalankan

1. Unduh dan ekstrak `FoundIt-WebApi-v1.0.0-win-x64.zip`.
2. Buat `appsettings.json` dari `appsettings.example.json` (sama seperti 2.2). Gunakan `ConnectionString` yang sama dengan aplikasi Desktop jika ingin berbagi database.
3. Jalankan `FoundIt.WebApi.exe`. Konsol menampilkan alamat layanan (biasanya `http://localhost:5000`). Biarkan jendela konsol tetap terbuka.
4. Buka `http://localhost:5000/swagger` di browser.

Jika port 5000 sedang dipakai: `FoundIt.WebApi.exe --urls http://localhost:5050`.

### 5.2 Daftar endpoint

| Method | Endpoint | Login | Fungsi |
|---|---|---|---|
| GET | `/api/listings?keyword=` | – | Cari listing aktif |
| GET | `/api/listings/{id}` | – | Detail listing |
| GET | `/api/categories` | – | Daftar kategori |
| GET | `/api/stats` | – | Statistik ringkas |
| POST | `/api/auth/register` | – | Daftar akun |
| POST | `/api/reports` | Basic Auth | Kirim laporan |
| GET | `/api/reports/mine` | Basic Auth | Laporan saya |

### 5.3 Contoh penggunaan

Cari listing:

```powershell
curl "http://localhost:5000/api/listings?keyword=dompet"
```

Daftar akun lalu kirim laporan (Basic Auth = `base64(email:password)`):

```powershell
curl -X POST http://localhost:5000/api/auth/register -H "Content-Type: application/json" `
     -d '{"name":"Budi","email":"budi@example.com","password":"Passw0rd1"}'

curl http://localhost:5000/api/categories      # salin salah satu "id" kategori

curl -X POST http://localhost:5000/api/reports `
     -H "Content-Type: application/json" `
     -H "Authorization: Basic YnVkaUBleGFtcGxlLmNvbTpQYXNzdzByZDE=" `
     -d '{"categoryId":"ID_KATEGORI","type":"Lost","title":"Dompet coklat","description":"Dompet coklat berisi KTM dan uang tunai.","location":"Kantin Teknik","incidentDate":"2026-11-20","imageUrl":null}'
```

Nilai `type` adalah `Lost` atau `Found`. Respons sukses: `201 Created` dengan status `Pending`.

Catatan: pada mode data sementara, ID kategori berubah setiap Web API dijalankan ulang, jadi ambil ulang dari `/api/categories`.

# Foto listing (simpan ke file)
curl http://localhost:5000/api/listings/ID_LISTING/image -o foto.jpg

## 6. Pemecahan Masalah

| Masalah | Penyebab dan solusi |
|---|---|
| Header menampilkan *Data sementara* padahal sudah mengisi database | `appsettings.json` belum dibuat atau `ConnectionString` kosong. Pastikan file bernama persis `appsettings.json` dan berada satu folder dengan exe. |
| Gagal terhubung ke PostgreSQL | Pastikan layanan PostgreSQL berjalan, serta host, port, user, dan password pada `ConnectionString` benar. |
| Admin tidak bisa login | Pastikan `AdminPassword` terisi di `appsettings.json` lalu jalankan ulang aplikasi. Email admin: `admin@foundit.local`. |
| *Email atau password salah* | Periksa huruf besar/kecil password. Pada data sementara, akun hilang saat aplikasi ditutup dan harus didaftarkan lagi. |
| Registrasi ditolak | Password minimal 8 karakter dengan huruf besar, huruf kecil, dan angka. Email yang sama tidak bisa didaftarkan dua kali. |
| Laporan tidak muncul di pencarian | Laporan harus disetujui admin terlebih dahulu. |
| API `400 Bad Request` saat kirim laporan | `categoryId` harus GUID asli dari `/api/categories`; judul 3–120 karakter; deskripsi minimal 10 karakter; tanggal tidak lebih dari besok. |
| API `401 Unauthorized` | Akun belum terdaftar atau header Basic Auth salah. |
| Peringatan SmartScreen | Klik **More info → Run anyway**. |
| File tidak dapat dibuka sebagai gambar | File yang dipilih bukan JPG/PNG yang valid. Pilih file gambar lain. |
| Ukuran gambar maksimal 2 MB | Terjadi pada API jika imageData terlalu besar. Aplikasi Desktop memperkecil foto otomatis. |
| File harus berupa gambar PNG atau JPG | Isi file bukan gambar (ekstensi saja tidak cukup). Pakai file JPG/PNG asli. |

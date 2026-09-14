# FoundIt

Aplikasi untuk mencari barang hilang di lingkungan FT

Kelompok FoundIT
Ketua Kelompok: Muhammad Bintang Hidayatullah Marbun
Anggota 1: Arimbi Arum Sari - 24/541867/TK/60129
Anggota 2: Aston Hugo - 24/538303/TK/59700
Anggota 3: Muhammad Bintang Hidayatullah Marbun - 24/544012/TK/60468

<img width="520" height="2560" alt="Media-_1_" src="https://github.com/user-attachments/assets/0c820ad1-2638-4829-9815-7c88a1b70f51" />

## Implementasi Class Diagram

Class diagram di atas telah diimplementasikan menggunakan C# pada folder
`src/FoundIt.Api/Domain`. Implementasi mencakup:

- `User` yang dapat mengirim laporan, mencari atau melihat listing, dan mengelola
  listing miliknya sendiri;
- `Admin` sebagai user dengan role administrator yang dapat memverifikasi user,
  menyetujui laporan, dan mengelola seluruh listing;
- `Report` dengan operasi update, pembatalan, persetujuan, dan penolakan;
- `Listing` sebagai publikasi dari report yang telah disetujui; dan
- `Category` yang memiliki kumpulan listing.

Password tidak disimpan dalam bentuk teks biasa. Properti `PasswordHash`
disediakan agar autentikasi nantinya dapat menggunakan mekanisme hashing yang
aman. Informasi barang tetap disimpan pada `Report`, sedangkan `Listing`
mengakses informasi tersebut melalui relasi agar tidak terjadi duplikasi data.

Relasi yang diimplementasikan:

```text
User 1 -------- 0..* Report
Report 1 ------ 0..1 Listing
Category 1 ---- 0..* Listing
Admin --------- verifies User and Report
```

## Aplikasi Desktop Windows Forms

FoundIt dikembangkan sebagai aplikasi desktop C# menggunakan Windows Forms,
sesuai pilihan framework pada Modul 1 Junior Project. Project antarmuka terdapat
pada folder `src/FoundIt.Desktop` dan menyediakan:

- halaman login dan registrasi;
- pencarian listing publik;
- formulir laporan barang hilang atau ditemukan;
- daftar laporan milik pengguna; dan
- halaman review laporan khusus administrator.

Penyimpanan saat ini masih menggunakan in-memory store sehingga data akan
direset ketika aplikasi ditutup. Password diproses menggunakan PBKDF2 dan tidak
disimpan sebagai teks biasa. Untuk membuat akun admin saat menjalankan aplikasi,
atur environment variable `FOUNDIT_ADMIN_PASSWORD`. Email admin adalah
`admin@foundit.local`.

Jalankan project pada Windows menggunakan Visual Studio atau perintah:

```powershell
$env:FOUNDIT_ADMIN_PASSWORD = "GantiDenganPasswordAman1"
dotnet run --project src/FoundIt.Desktop
```

# Jadwal Sholat

Aplikasi web jadwal sholat untuk tampilan masjid (Blazor WebAssembly .NET 10 + MudBlazor), sesuai
[Requirement.md](Requirement.md).

## Struktur proyek

```
JadwalSholat.sln
src/
  JadwalSholat.Core/     Model domain, kalkulasi jadwal/countdown, konversi Hijriyah — murni C#, tanpa dependensi browser.
  JadwalSholat.Web/      Aplikasi Blazor WebAssembly (dashboard + halaman pengaturan), MudBlazor, LocalStorage.
tests/
  JadwalSholat.Core.Tests/   xUnit — logika bisnis (52 test).
  JadwalSholat.Web.Tests/    xUnit + bUnit — komponen & layanan Web (24 test).
tools/
  generate_prayer_times.py  Script Python untuk mengambil data 1 tahun dari api.myquran.com.
```

## Menjalankan aplikasi

```bash
cd src/JadwalSholat.Web
dotnet run
```

Buka `http://localhost:5289` (dashboard) dan `http://localhost:5289/settings` (pengaturan).

## Menjalankan test

```bash
dotnet test JadwalSholat.sln
```

Repo ini punya git hook (`.githooks/pre-commit`, sudah diaktifkan via
`git config core.hooksPath .githooks`) yang menjalankan seluruh test suite di atas sebelum setiap
commit — jalannya di bawah 1 detik, jadi murah untuk digate di setiap commit. Kalau clone ulang
repo ini di mesin lain, jalankan `git config core.hooksPath .githooks` sekali untuk mengaktifkannya
kembali.

## Arsitektur & keputusan desain

- **Data jadwal sholat**: bukan dipanggil live ke API saat aplikasi jalan, melainkan di-generate
  sebelumnya (`tools/generate_prayer_times.py`) menjadi file statis
  `src/JadwalSholat.Web/wwwroot/data/prayertimes/{cityId}.json`, satu file per kota, masing-masing
  365 hari, bersumber dari `api.myquran.com`. Ini sesuai catatan di Requirement.md: "agar tidak
  perlu feed tiap hari/bulan". Jalankan ulang script ini setahun sekali untuk memperbarui data.
- **5 kota yang didukung**: Jakarta, Bandung, Semarang, Yogyakarta, Surabaya (`City.Bundled` di
  Core). Deteksi lokasi otomatis mencocokkan GPS ke kota terdekat dari 5 ini via haversine — bukan
  pencarian ke seluruh kota Indonesia, karena hanya 5 kota ini yang datanya di-bundle.
- **Kalender Hijriyah**: memakai `System.Globalization.HijriCalendar` bawaan .NET (kalkulasi
  tabular). Bisa berbeda 1 hari dari pengumuman rukyat Kemenag di sekitar pergantian bulan — hal
  ini didokumentasikan sebagai estimasi, bukan dianggap sebagai bug.
- **Mode tampilan dashboard**: dashboard punya dua mode yang berganti otomatis berdasarkan status
  waktu sholat (`PrayerStatusCalculator`), bukan berdasarkan interaksi pengguna:
  - **Ambient** (default): wallpaper berputar di background, jam kecil, grid waktu sholat tetap
    menempel di bawah.
  - **Focus**: dipicu otomatis ketika countdown ke sholat berikutnya ≤ 60 detik ATAU sedang dalam
    jendela "waktu sholat" aktif (Iqamah sampai selesai durasi). Wallpaper disembunyikan, jam dan
    countdown/banner membesar. Ini mengimplementasikan literal Requirement.md #31: "Setiap
    beberapa saat (bukan waktu iqamah atau waktu sholat), display utama akan berubah menjadi
    wallpaper."
- **Penyimpanan**: semua pengaturan (`AppSettings`) disimpan di LocalStorage browser via JS interop
  langsung (bukan library pihak ketiga) di bawah key `jadwalsholat.settings.v1`.
- **Wallpaper**: 4 wallpaper default (SVG gradient + siluet masjid, dibuat lokal) di-seed otomatis
  saat pertama kali dijalankan. Admin bisa menambah wallpaper lain lewat URL gambar di halaman
  Pengaturan.

## Keterbatasan yang diketahui

- Dataset jadwal sholat mencakup 12 bulan berjalan sejak terakhir di-generate. Di luar rentang itu,
  dashboard menampilkan pesan "jadwal belum tersedia" alih-alih menghitung sendiri — jalankan ulang
  `tools/generate_prayer_times.py` untuk memperbarui.
- Auto-detect lokasi hanya mencocokkan ke salah satu dari 5 kota bundel, bukan lokasi presisi.
- Tanggal Hijriyah adalah estimasi kalender tabular, bukan hasil rukyatul hilal resmi.

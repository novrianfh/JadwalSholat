# PRD

## Project Overview
- Nama Project      :  Jadwal Sholat Web Application
- Technology Stack  :  Blazor Webassembly .Net 10, MudBlazor
- Target Platform   : Web Browser (Responsive for Desktop and Mobile)
- Bahasa Default    : Bahasa Indonesia

## Core Features Specification

### 1. Prayer Times Display (Main Dashboard)
- **Description:** Display the 5 obligatory prayer times (Fajr, Dhuhr, Asr, Maghrib, Isha) plus the Shuruk (Sunrise) time. Terdapat nama masjid. Ditampilkan pula saat ini tanggal berapa (dalam hijriyah dan )
- **Data Source:** Integration with a public prayer times API (e.g., Aladhan API or Kemenag API) based on coordinates or city selection. e.g API :  https://api.myquran.com/doc#tag/Sholat
- **UI Components:**
  - A clean, modern card layout highlighting the *current/next prayer time* countdown.
  - A structured grid or list displaying all 6 times chronologically.
  - Highlighting the active prayer time row dynamically.
  - Letaknya di bagian menempel di layar bagian bawah.
- **Logic:** Real-time clock updating every second to check against prayer schedules. Lokasi bisa dideteksi otomatsi atau memasukkan pada setting. 
- setiap 1 menit menjelang waktu sholat, ada countdown.
- saat waktu sholat berlangsung (berdasarkan setting waktu sholat). Ditambahkan tulisan "Waktu sholat {nama sholat}"

### 2. Setting Page 
- Prayer Times Adjustment: Terdapat setting untuk menambah atau mengurangi jadwal waktu sholat dari API dalam menit. Input berupa integer.
- Iqamah Time Adjustment Tiap Waktu Sholat: Setting untuk mengubah waktu iqamah dari jadwal sholat. default 10 menit.
- Setting Waktu sholat: Tiap waktu sholat akan ada setting waktunya. Default 15 menit. dari waktu iqamah.
- Setting Nama Masjid: Terdapat setting untuk mengubah nama masjid dan alamat masjid


### 3. Wallpaper carousel
- Deskripsi: Setiap beberapa saat (bukan waktu iqamah atau waktu sholat), display utama akan berubah menjadi wallpaper. Jam akan mengecil. Waktu sholat tetap muncul.
- Default pergantian antar wallpaper selama 10 detik.
- Transisi pergantian wallpaper berupa fade-in fade-out
- terdapat setting untuk mengganti wallpaper


### Catatan: 
- penyimpanan menggunakan LocalStorage
- kalau bisa, ambil jadwal sholat dari kota besar di Indonesia selama 1 tahun agar tidak perlu feed tiap hari/bulan: awalnya Yogyakarta, Jakarta, Semarang, Surabaya, Bandung — diperluas ke seluruh 38 ibukota provinsi ditambah beberapa kota besar lain (lihat `City.Bundled`).




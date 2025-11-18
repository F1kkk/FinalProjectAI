# Prototipe AI Mobil Balap (AI Final Project)

![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=for-the-badge&logo=unity)

Sebuah prototipe game balap yang dibuat sebagai Proyek Akhir (Final Project) untuk mata kuliah Kecerdasan Buatan (Artificial Intelligence). Proyek ini mendemonstrasikan 5 konsep inti AI dalam game, menggunakan **Wheel Collider** untuk fisika mobil yang realistis dan **Path Following** untuk navigasi.

## 🎯 Latar Belakang Proyek

Proyek ini dibuat untuk memenuhi tugas Final Project mata kuliah Kecerdasan Buatan di **Politeknik Elektronika Negeri Surabaya (PENS)**. Tujuannya adalah untuk mengimplementasikan 5 konsep AI yang telah dipelajari dalam sebuah prototipe game yang fungsional dan dapat didemonstrasikan.

---

## 🤖 5 Elemen AI yang Diimplementasikan

Prototipe ini secara spesifik mendemonstrasikan 5 konsep inti AI dari materi perkuliahan:

### 1. AI Navigation / Pathfinding (Materi: day 10, 11)
* **Implementasi:** AI menggunakan `List<Transform> waypoints` yang diatur secara manual di sepanjang "racing line" (luar-dalam-luar) lintasan.
* **Logika:** AI selalu tahu target `waypoint` berikutnya dan akan beralih ke target selanjutnya secara otomatis ketika jaraknya sudah cukup dekat (ditentukan oleh `waypointRadius`).

### 2. Steering Behaviors (Materi: day 07, 08)
* **Implementasi:** Alih-alih "teleport" atau bergerak kaku, AI secara aktif "menyetir".
* **Logika:** Skrip menghitung sudut ke target (`angleToTarget`) di setiap `FixedUpdate()`. Nilai ini kemudian diterjemahkan langsung menjadi `steerAngle` untuk `Wheel Collider` depan, mensimulasikan *steering behavior* "Seek" (Mengejar).

### 3. Finite State Machine (FSM) (Materi: day 03, 04)
* **Implementasi:** Logika `FixedUpdate()` berfungsi sebagai FSM dinamis (Finite State Machine).
* **Logika:** AI memiliki beberapa "state" perilaku berdasarkan situasinya:
    1.  **State `GAS_PENUH`:** Jika jalur lurus (`angleToTarget` kecil).
    2.  **State `BELOK_BIASA`:** Jika tikungan biasa (`angleToTarget` sedang), AI mengurangi `motorTorque`.
    3.  **State `REM_TIKUNGAN_TAJAM`:** Jika tikungan sangat tajam (`angleToTarget` besar) DAN mobil melaju kencang, AI akan mengurangi gas dan mengaktifkan `brakeTorque`.
    4.  **State `START_TIKUNGAN`:** Jika tikungan tajam TAPI mobil diam (seperti di garis start), AI akan mengabaikan rem dan hanya menggunakan gas kecil agar bisa mulai bergerak.

### 4. Reflex Agents (Materi: day 02)
* **Implementasi:** Skrip `PowerUpCube.cs` yang dipasang pada setiap kotak power-up.
* **Logika:** Skrip ini bertindak sebagai *reflex agent* murni. Ia menggunakan `OnTriggerEnter` sebagai sensor (Kondisi). Saat mobil AI menyentuhnya, ia langsung bereaksi (Aksi) dengan menjalankan fungsi `HandleCollision`.

### 5. Probability (Materi: day 05)
* **Implementasi:** Di dalam fungsi `HandleCollision()` pada `PowerUpCube.cs`.
* **Logika:** Saat AI menabrak cube (Reflex), skrip akan menjalankan `Random.Range(0f, 1.0f)`. Ada **kemungkinan** (`chance`) AI akan mendapatkan *boost* (memanggil `ActivateSpeedBoost()`), dan ada **kemungkinan** dia tidak mendapatkan apa-apa.

---

## 🛠️ Cara Kerja Teknis

Proyek ini menggunakan dua skrip utama:

### `AICarController.cs`
* **Tugas:** Otak dan Otot utama mobil AI.
* **Setup:** Dipasang di `GameObject` mobil. Membutuhkan `Rigidbody` (Mass=1500, Drag=0.1) dan 4 `Wheel Collider` (WC_FL, WC_FR, ...).
* **Fisika:** Menggunakan `Rigidbody` dan `Wheel Collider` untuk pergerakan. `CenterOfMass` diatur rendah untuk anti-guling.
* **Navigasi:** Membaca `List<Transform> waypoints` untuk menentukan target.
* **FSM:** Logika `if-else` di `FixedUpdate()` mengatur `motorTorque` dan `brakeTorque` berdasarkan `angleToTarget` dan `currentSpeed`.
* **Boost:** Menyediakan fungsi `public IEnumerator ActivateSpeedBoost()` yang bisa dipanggil dari luar.

### `PowerUpCube.cs`
* **Tugas:** Mengatur logika Power-Up (Reflex & Probabilitas).
* **Setup:** Dipasang di setiap `GameObject` Power-Up.
* **Trigger:** Menggunakan `Box Collider` yang di-set ke **`Is Trigger`**.
* **Logika:** Saat `OnTriggerEnter` mendeteksi mobil AI, ia menjalankan `Random.Range()`. Jika berhasil, ia memanggil `ActivateSpeedBoost()` di skrip mobil dan memulai *coroutine* respawn.

---

## ⚙️ Setup & Konfigurasi

Untuk menjalankan proyek ini di Unity:

1.  **Setup Mobil AI:**
    * Pasang `Rigidbody` (Gunakan `Mass=1500`, `Linear Damping=0.1`, `Angular Damping=0.5`).
    * `Constraints`: `Freeze Rotation X` dan `Z`. **JANGAN** `Freeze Position Y`.
    * Buat 4 `GameObject` anak untuk `Wheel Collider` (WC) dan atur `Radius`, `Spring`, & `Damper`-nya.
    * Buat `GameObject` `CenterOfMass` dan posisikan rendah.
    * Pasang skrip `AICarController.cs`.
    * Isi semua slot di Inspector: 4 `WC`, 4 `Mesh Roda`, `CenterOfMass`, dan `Waypoints`.

2.  **Setup Lintasan:**
    * Pastikan lintasan/jalan memiliki `Mesh Collider`.
    * Buat `GameObject` "Waypoints" dan isi dengan 15-20 `GameObject` kosong (`WP1`, `WP2`, ...) mengikuti 'racing line' (luar-dalam-luar).
    * Isi `List Waypoints` di skrip mobil AI dengan `GameObject` `WP` tadi.

3.  **Setup Power-Up:**
    * Letakkan objek Cube (`PowerUpBox`) di lintasan.
    * Pasang skrip `PowerUpCube.cs`.
    * Pastikan `Box Collider`-nya di-set ke `Is Trigger`.
    * Beri `Tag` "PowerUpBox".

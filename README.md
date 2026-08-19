# Unity VAT (Vertex Animation Texture) System

Sistem VAT (Vertex Animation Texture) ini adalah solusi lengkap untuk memanggang (baking) animasi dari `SkinnedMeshRenderer` dan `Animator` ke dalam tekstur di Unity. Sistem ini sangat berguna untuk optimisasi performa (misalnya untuk sistem *crowd* atau *boids* menggunakan ECS/GPU Instancing) dengan memindahkan beban komputasi tulang (bone/rigging) dari CPU ke GPU.

## Fitur Utama
1. **Baking Terintegrasi**: Memanggang (Bake) posisi verteks dan normal secara otomatis dari klip animasi menjadi tekstur 2D.
2. **Runtime Animator**: Komponen `VATAnimator` untuk mengontrol dan berpindah antar animasi VAT secara real-time pada saat runtime.
3. **Database Animasi**: Menyimpan referensi material dan data animasi secara rapi menggunakan `VATAnimationDatabase` (ScriptableObject).
4. **Pembuatan Prefab Otomatis**: Secara otomatis merakit prefab karakter dan animasi yang sudah terkonfigurasi dengan material dan shader VAT.
5. **Shader URP (Universal Render Pipeline)**: Shader kustom (`Shader.Find("VAT/VAT")`) yang membaca tekstur posisi dan normal untuk menggerakkan mesh statis.
6. **Otomatisasi Rilis (CI/CD)**: Mendukung pembuatan `.unitypackage` secara otomatis menggunakan GitHub Actions.

## Struktur Folder
- `Editor/`: Berisi skrip editor untuk jendela antarmuka pengguna, *baker*, utilitas aset, dan pembuat prefab (*Prefab Builder*).
- `Runtime/`: Berisi skrip runtime seperti `VATAnimator`, `VATAnimationData`, dan `VATAnimationDatabase`.
- `Shaders/`: Berisi shader URP yang mengolah VAT.
- `Characters/`: Folder *default* yang digunakan oleh sistem untuk menyimpan mesh statis, material, tekstur, dan prefab hasil *baking*.

## Cara Penggunaan
1. **Buka Jendela Baker**:
   - Di menu Unity, buka `Window > VAT Baker`.
2. **Setup Baking**:
   - Masukkan `Source Character` (prefab/gameobject yang memiliki `Animator` dan `SkinnedMeshRenderer`).
   - Berikan nama pada karakter.
   - Tambahkan klip animasi yang ingin di-*bake* (misalnya: *Idle*, *Walk*, *Run*).
3. **Mulai Baking**:
   - Klik tombol **Bake All Animations**.
   - Sistem akan secara otomatis:
     - Merekam setiap frame animasi.
     - Menyimpan tekstur (Position & Normal) ke dalam folder aset.
     - Menggabungkan *multiple meshes* jika karakter memiliki beberapa bagian (*sub-meshes*).
     - Membuat material baru dengan shader VAT.
     - Menghasilkan *Animation Prefab* terpisah untuk setiap animasi.
     - Membuat *Character Prefab* utama yang dilengkapi dengan komponen `VATAnimator`.
     - Membuat `VATAnimationDatabase` yang menghubungkan semua aset.
4. **Runtime**:
   - Anda cukup meletakkan *Character Prefab* hasil *baking* ke dalam *Scene*.
   - Gunakan referensi `VATAnimator` di skrip lain untuk memanggil metode seperti `Play("Walk")` atau mengganti `currentAnimation`.

## Pengembangan & Kontribusi

Sistem ini diatur sebagai repositori mandiri. Untuk kemudahan distribusi, repository ini memiliki *GitHub Actions workflow* yang otomatis membundel (*package*) folder menjadi `.unitypackage`.

### Membuat Rilis (Release) Baru
Jika Anda menambahkan fitur baru dan ingin membagikan paket terbarunya:
1. *Commit* dan *Push* perubahan Anda ke *branch* utama (main).
2. Buat *tag* dengan format versi (misalnya `v1.0.0`):
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. GitHub Actions akan berjalan otomatis dan mengunggah file `VATSystem.unitypackage` ke dalam GitHub Releases di repositori Anda. Anda atau orang lain kemudian dapat mengunduh package tersebut dan mengimpornya langsung ke proyek Unity lain.

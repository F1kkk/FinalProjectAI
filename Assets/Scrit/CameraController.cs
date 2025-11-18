using UnityEngine;

public class CameraController : MonoBehaviour
{
    // ===================================================================
    // VARIABEL INSPECTOR
    // ===================================================================
    
    [Header("Target (Mobil)")]
    public Transform target; // Tarik (drag) GameObject mobilmu ke slot ini

    [Header("Pengaturan Kamera")]
    [Tooltip("Seberapa jauh kamera dari mobil (x=menyamping, y=tinggi, z=mundur)")]
    public Vector3 offset = new Vector3(0f, 5.0f, -10.0f);

    [Tooltip("Seberapa cepat kamera mengikuti posisi mobil")]
    public float followSpeed = 8f;

    [Tooltip("Seberapa cepat kamera berotasi melihat mobil")]
    public float turnSpeed = 4f;


    // ===================================================================
    // LOGIKA KAMERA
    // ===================================================================

    // Gunakan LateUpdate() untuk kamera.
    // Ini berjalan SETELAH semua Update() (seperti gerakan mobil) selesai.
    // Ini mencegah kamera gemetar (jitter).
    void LateUpdate()
    {
        // Jika tidak ada target (mobil), jangan lakukan apa-apa
        if (target == null)
        {
            return;
        }

        // --- 1. MENGHITUNG POSISI YANG DIINGINKAN ---
        
        // Kita hitung posisi target kamera.
        // Yaitu: posisi mobil + offset yang sudah diputar sesuai rotasi mobil.
        // Ini memastikan offset (0, 5, -10) akan SELALU di belakang mobil.
        Vector3 desiredPosition = target.position + (target.rotation * offset);

        
        // --- 2. MENGGERAKKAN KAMERA (Secara Mulus) ---
        
        // Alih-alih: transform.position = desiredPosition; (Ini akan kaku/patah-patah)
        // Kita gunakan Lerp (Linear Interpolation) agar pergerakannya mulus.
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        
        // --- 3. MEROTASI KAMERA (Secara Mulus) ---
        
        // Hitung arah dari kamera ke mobil
        Vector3 lookDirection = target.position - transform.position;

        // Buat rotasi yang melihat ke arah tersebut
        Quaternion desiredRotation = Quaternion.LookRotation(lookDirection);

        // Gunakan Slerp (Spherical Linear Interpolation) agar rotasi beloknya mulus
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
    }
}
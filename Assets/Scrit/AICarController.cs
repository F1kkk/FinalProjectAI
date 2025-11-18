using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AICarController : MonoBehaviour
{
    // ===================================================================
    // KOMPONEN 1: AI NAVIGATION (Waypoints Simpel)
    // ===================================================================
    [Header("1. Navigasi Path (Waypoints)")]
    [Tooltip("Tarik semua GameObject Waypoint kamu ke list ini")]
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    // ===================================================================
    // OTOT: WHEEL COLLIDERS
    // ===================================================================
    [Header("Setup Wheel Collider (Fisika)")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Setup Roda (Visual)")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;

    [Header("Setup Penting Lainnya")]
    public Transform centerOfMass;
    private Rigidbody rb;

    // ===================================================================
    // OTAK: PENGATUR TENAGA & BELOK
    // ===================================================================
    [Header("2. Steering (Tenaga & Belok)")]
    [Tooltip("TENAGA BESAR agar bisa gerakkan Mass 1500. Coba 25000")]
    public float maxMotorTorque = 25000f;
    [Tooltip("TENAGA REM BESAR. Coba 50000")]
    public float maxBrakeTorque = 50000f;
    [Tooltip("Sudut belok roda. Coba 35")]
    public float maxSteerAngle = 35f;
    [Tooltip("Radius (jarak) dari waypoint untuk ganti target. Coba 10")]
    public float waypointRadius = 10f;


    // ===================================================================
    // KOMPONEN 3: FSM (State & Visuals)
    // ===================================================================
    [Header("3. FSM (State & Visuals)")]
    public Material racingMaterial;
    public Material recoveringMaterial;
    
    // --- INI PERUBAHANNYA ---
    [Tooltip("Tarik GameObject bodi mobil (yang punya Mesh Renderer) ke sini")]
    public Renderer carBodyRenderer; // Slot baru untuk bodi mobil
    // -----------------------

    
    // ===================================================================
    // KOMPONEN 5: REVISI FSM (BOOST)
    // ===================================================================
    [Header("4. Efek Visual (Boost)")]
    [Tooltip("Material yang akan digunakan untuk kelap-kelip")]
    public Material boostFlashMaterial; 
    [Tooltip("Durasi setiap kelap-kelip (misal: 0.1 detik)")]
    public float flashDuration = 0.1f;
    [Tooltip("Berapa lama efek boost & kelap-kelip aktif (detik)")]
    public float boostDuration = 4f; 
    
    private Material originalRacingMaterial; 
    private Coroutine flashingEffectCoroutine; 


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Setup Center of Mass (SANGAT PENTING)
        if (centerOfMass)
        {
            rb.centerOfMass = centerOfMass.localPosition;
        }

        // Setup helper skrip untuk roda visual (Otomatis)
        SetupWheelMesh(meshFL, wheelFL);
        SetupWheelMesh(meshFR, wheelFR);
        SetupWheelMesh(meshRL, wheelRL);
        SetupWheelMesh(meshRR, wheelRR);
        
        // (Visual) Menggunakan slot carBodyRenderer baru
        if(carBodyRenderer) carBodyRenderer.material = racingMaterial;
        
        // Simpan material racing asli
        if (racingMaterial != null)
        {
            originalRacingMaterial = racingMaterial;
        }
        else if (carBodyRenderer != null) // Fallback jika racingMaterial lupa diisi
        {
            originalRacingMaterial = carBodyRenderer.material;
        }
    }

    // ===================================================================
    // FUNGSI FIXEDUPDATE (ANTI-MACET DI GARIS START)
    // ===================================================================
    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        // 1. (OTAK) Tentukan target waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // 2. (OTAK) Hitung arah dan sudut ke target
        Vector3 dirToTarget = targetWaypoint.position - transform.position;
        float distanceToTarget = dirToTarget.magnitude;
        float angleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

        // 3. (OTAK) Tentukan perintah
        float steer = Mathf.Clamp(angleToTarget, -maxSteerAngle, maxSteerAngle);
        float torque = maxMotorTorque;
        float brake = 0f;

        // --- Logika FSM (Anti-Macet) ---
        float currentSpeed = rb.velocity.magnitude;

        // JIKA TIKUNGAN SANGAT TAJAM (lebih dari 45 derajat)
        if (Mathf.Abs(angleToTarget) > 45f)
        {
            // DAN JIKA KITA SEDANG MELAJU KENCANG (misal, lebih dari 5 m/s)
            if (currentSpeed > 5f) 
            {
                torque = maxMotorTorque * 0.1f; // Gas 10%
                brake = maxBrakeTorque;         // REM PENUH!
            }
            // JIKA KITA DIAM (seperti di garis start)
            else
            {
                torque = maxMotorTorque * 0.3f; // Gas 30% (agar bisa start)
                brake = 0f;                     // JANGAN REM!
            }
        }
        // JIKA TIKUNGAN BIASA (15-45 derajat)
        else if (Mathf.Abs(angleToTarget) > 15f)
        {
            torque = maxMotorTorque * 0.6f; // Gas 60%
            brake = 0f;
        }
        // JIKA LURUS
        else
        {
            torque = maxMotorTorque;
            brake = 0f;
        }
        
        // 4. (OTOT) Eksekusi perintah ke Wheel Colliders
        wheelFL.steerAngle = steer;
        wheelFR.steerAngle = steer;

        wheelRL.motorTorque = torque;
        wheelRR.motorTorque = torque;

        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelRL.brakeTorque = brake;
        wheelRR.brakeTorque = brake;

        // 5. (NAVIGASI) Cek jika sudah sampai di waypoint
        if (distanceToTarget < waypointRadius)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0;
            }
        }
    }
    
    // ===================================================================
    // KOMPONEN 5: PROBABILITY (Fungsi Boost dengan FSM Kelap-kelip)
    // ===================================================================
    public IEnumerator ActivateSpeedBoost()
    {
        Debug.Log("PROBABILITY: Kena PowerUp, BERHASIL boost!");
        float originalTorque = maxMotorTorque;
        
        // Tenaga boost
        maxMotorTorque = originalTorque * 4f; 
        
        // Mulai coroutine kelap-kelip
        if (flashingEffectCoroutine != null) StopCoroutine(flashingEffectCoroutine);
        flashingEffectCoroutine = StartCoroutine(FlashMaterialEffect());

        // Tunggu durasi boost (menggunakan variabel)
        yield return new WaitForSeconds(boostDuration); 
        
        // Hentikan boost
        maxMotorTorque = originalTorque; 
        Debug.Log("Boost AI selesai.");

        // Hentikan coroutine kelap-kelip DAN kembalikan material
        if (flashingEffectCoroutine != null) StopCoroutine(flashingEffectCoroutine);
        if (carBodyRenderer != null && originalRacingMaterial != null)
        {
            carBodyRenderer.material = originalRacingMaterial; 
        }
    }
    
    // ===================================================================
    // FUNGSI COROUTINE BARU: Efek Visual Kelap-kelip
    // ===================================================================
    IEnumerator FlashMaterialEffect()
    {
        // Pastikan kita punya material untuk kelap-kelip dan renderer
        if (boostFlashMaterial == null || carBodyRenderer == null || originalRacingMaterial == null)
        {
            Debug.LogWarning("Material Boost atau Renderer Bodi belum di-set!");
            yield break; // Tidak bisa kelap-kelip jika material tidak diset
        }

        // Loop tak terbatas (akan dihentikan oleh ActivateSpeedBoost)
        while (true)
        {
            // Ubah ke material kelap-kelip
            carBodyRenderer.material = boostFlashMaterial;
            yield return new WaitForSeconds(flashDuration);

            // Ubah kembali ke material asli
            carBodyRenderer.material = originalRacingMaterial;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    // --- Fungsi Helper (Sama) ---

    void SetupWheelMesh(Transform mesh, WheelCollider collider)
    {
        if (mesh)
        {
            AIWheelMesh wheelScript = mesh.gameObject.GetComponent<AIWheelMesh>();
            if (wheelScript == null)
            {
                 wheelScript = mesh.gameObject.AddComponent<AIWheelMesh>();
            }
            wheelScript.collider = collider;
        }
    }
}
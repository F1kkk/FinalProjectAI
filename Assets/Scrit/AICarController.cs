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
    public List<Transform> waypoints; // Gunakan List simpel
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
    public float maxMotorTorque = 25000f; // TENAGA BESAR!
    [Tooltip("TENAGA REM BESAR. Coba 50000")]
    public float maxBrakeTorque = 50000f; // REM BESAR!
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
    private Renderer carRenderer;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carRenderer = GetComponent<Renderer>();

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
        
        // (Visual)
        if(carRenderer) carRenderer.material = racingMaterial;
    }

    // ===================================================================
    // FUNGSI FIXEDUPDATE BARU (ANTI-MACET DI GARIS START)
    // ===================================================================
    void FixedUpdate()
    {
        if (waypoints.Count == 0) return;

        // ===================================================================
        // LOGIKA AI (OTAK & OTOT) - VERSI BARU
        // ===================================================================
        
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

        // --- Logika FSM Baru (Anti-Macet) ---
        
        // Cek kecepatan mobil saat ini (dalam m/s)
        float currentSpeed = rb.velocity.magnitude;

        // JIKA TIKUNGAN SANGAT TAJAM (lebih dari 45 derajat)
        if (Mathf.Abs(angleToTarget) > 45f)
        {
            // DAN JIKA KITA SEDANG MELAJU KENCANG (misal, lebih dari 5 m/s)
            if (currentSpeed > 5f) 
            {
                // Baru kita REM dan kurangi GAS
                torque = maxMotorTorque * 0.1f; // Gas 10%
                brake = maxBrakeTorque;         // REM PENUH!
            }
            // JIKA KITA DIAM (seperti di garis start)
            else
            {
                // JANGAN REM. Cukup kurangi GAS agar bisa belok pelan-pelan
                torque = maxMotorTorque * 0.3f; // Gas 30% (agar bisa start)
                brake = 0f;                     // JANGAN REM!
            }
        }
        // JIKA TIKUNGAN BIASA (15-45 derajat)
        else if (Mathf.Abs(angleToTarget) > 15f)
        {
            // Kurangi gas sedikit, tidak perlu rem
            torque = maxMotorTorque * 0.6f; // Gas 60%
            brake = 0f;
        }
        // JIKA LURUS
        else
        {
            // Gas penuh, tidak ada rem
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
            // Jika sudah di waypoint terakhir, kembali ke awal (looping)
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0;
            }
        }
    }
    
    // ===================================================================
    // KOMPONEN 5: PROBABILITY (Fungsi Boost Sesuai Permintaan)
    // ===================================================================
    // (Akan dipanggil oleh PowerUpCube.cs)
    public IEnumerator ActivateSpeedBoost()
    {
        Debug.Log("PROBABILITY: Kena PowerUp, BERHASIL boost!");
        // Kita tidak bisa langsung set torque, jadi kita simpan
        float originalTorque = maxMotorTorque;
        
        // Sesuai permintaan: 4x tenaga
        maxMotorTorque = originalTorque * 4f; 
        
        // Sesuai permintaan: 4 detik
        yield return new WaitForSeconds(4f); 
        
        maxMotorTorque = originalTorque; // Kembali normal
        Debug.Log("Boost AI selesai.");
    }
    
    // --- Fungsi Helper (Sama) ---

    void SetupWheelMesh(Transform mesh, WheelCollider collider)
    {
        if (mesh)
        {
            // Cek dulu jika sudah ada skripnya
            AIWheelMesh wheelScript = mesh.gameObject.GetComponent<AIWheelMesh>();
            if (wheelScript == null)
            {
                 wheelScript = mesh.gameObject.AddComponent<AIWheelMesh>();
            }
            wheelScript.collider = collider;
        }
    }
}
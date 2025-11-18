using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Membutuhkan Box Collider yang di-set ke IS TRIGGER
[RequireComponent(typeof(BoxCollider))]
public class PowerUpCube : MonoBehaviour
{
    public float boostChance = 0.5f; // 50% chance
    public float respawnTime = 5.0f; // 5 detik

    private Collider col;
    private Renderer rend;

    void Start()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        
        // Pastikan collider adalah Trigger
        if (!col.isTrigger)
        {
            Debug.LogWarning("Collider di " + name + " HARUS 'Is Trigger'!");
            col.isTrigger = true;
        }

        if (!gameObject.CompareTag("PowerUpBox"))
        {
            Debug.LogWarning("Tag 'PowerUpBox' belum dipasang di " + name);
        }
    }

    // GANTI DARI OnCollisionEnter KE OnTriggerEnter
    void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang menabrak adalah mobil AI
        AICarController aiCar = other.gameObject.GetComponent<AICarController>();
        if (aiCar != null)
        {
            HandleCollision(aiCar);
        }
    }

    void HandleCollision(AICarController aiCar)
    {
        // ===================================================================
        // KOMPONEN 5: PROBABILITY (Logika Penuh)
        // ===================================================================
        float chance = Random.Range(0f, 1.0f);
        if (chance <= boostChance)
        {
            aiCar.StartCoroutine(aiCar.ActivateSpeedBoost());
        }
        else
        {
            Debug.Log("PROBABILITY: GAGAL boost (Chance: " + chance + ")");
        }
        
        StartCoroutine(DoRespawn());
    }

    IEnumerator DoRespawn()
    {
        // 1. Nonaktifkan (Hilangkan) cube
        rend.enabled = false;
        col.enabled = false; // Matikan juga collider agar tidak di-trigger 2x

        // 2. Tunggu
        yield return new WaitForSeconds(respawnTime);

        // 3. Aktifkan (Munculkan) cube kembali
        rend.enabled = true;
        col.enabled = true;
    }
}
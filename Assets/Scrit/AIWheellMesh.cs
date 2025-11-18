using UnityEngine;

public class AIWheelMesh : MonoBehaviour
{
    // Skrip ini akan dipasang oleh AICarController
    // ke GameObject roda visual (mesh)

    public WheelCollider collider; // WheelCollider fisika

    void Update()
    {
        if (collider == null) return;

        Vector3 pos;
        Quaternion rot;

        // Ambil posisi & rotasi dari WheelCollider (fisika)
        collider.GetWorldPose(out pos, out rot);

        // Terapkan ke Transform roda ini (visual)
        transform.position = pos;
        transform.rotation = rot;
    }
}
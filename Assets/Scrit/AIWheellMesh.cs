using UnityEngine;

public class AIWheelMesh : MonoBehaviour
{
    public WheelCollider collider; // WheelCollider fisika

    void Update()
    {
        if (collider == null) return;

        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        transform.position = pos;
        transform.rotation = rot;
    }
}
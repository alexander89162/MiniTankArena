using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class TurretAimCannon : MonoBehaviour
{
    private ConfigurableJoint joint;

    [Header("Aiming Settings")]
    public float aimSpeedMultiplier = 1f;      // per-turret speed variation
    public bool allowPitch = true;
    public bool allowYaw   = true;

    // Public method called by the main controller
    public void SetAimDelta(Vector2 delta)
    {
        if (joint == null) return;

        // delta.x = yaw input, delta.y = pitch input
        float yaw   = allowYaw   ? delta.x * aimSpeedMultiplier : 0f;
        float pitch = allowPitch ? delta.y * aimSpeedMultiplier : 0f;

        // Apply to joint target rotation
        Quaternion targetRot = Quaternion.Euler(-pitch, yaw, 0);
        joint.targetRotation = targetRot;
    }

    void Awake()
    {
        joint = GetComponent<ConfigurableJoint>();
        if (joint == null)
        {
            Debug.LogError("TurretAimJoint requires a ConfigurableJoint on " + name);
            enabled = false;
        }
    }
}
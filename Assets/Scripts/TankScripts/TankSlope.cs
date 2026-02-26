using UnityEngine;

public class TankSlope : MonoBehaviour
{
    [Header("Slope Settings")]
    public float alignSpeed = 5f;              // How fast it tilts (higher = snappier)
    public float rayDistance = 1.5f;           // Ray length down (match tank height/2 + margin)
    public LayerMask groundLayer;              // Ground layer only (assign in Inspector)
    public float minMoveSpeedToAlign = 0.5f;   // Only tilt if moving this fast

    [Header("References")]
    public Transform tankRoot;                 // Drag root "eo" here (for position/forward)

    private Vector3 currentVelocity;           // Passed from TankController

    // Public method: Called by TankController each frame
    public void UpdateAlignment(Vector3 velocity)
    {
        currentVelocity = velocity;

        // Only align if moving fast enough
        if (currentVelocity.magnitude < minMoveSpeedToAlign)
            return;

        AlignToSlope();
    }

    void AlignToSlope()
    {
        // Raycast down from tank center
        Vector3 rayStart = tankRoot.position + Vector3.up * 0.5f;  // Mid-body
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayer))
        {
            Vector3 groundNormal = hit.normal;

            // Project forward onto slope plane
            Vector3 forwardOnSlope = Vector3.ProjectOnPlane(tankRoot.forward, groundNormal).normalized;

            // Target rotation: forwardOnSlope as forward, groundNormal as up
            Quaternion targetRotation = Quaternion.LookRotation(forwardOnSlope, groundNormal);

            // Smoothly rotate THIS object (Tank Base)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignSpeed * Time.deltaTime);
        }
    }

    // Debug ray in Scene view
    void OnDrawGizmosSelected()
    {
        if (tankRoot == null) return;
        Gizmos.color = Color.yellow;
        Vector3 start = tankRoot.position + Vector3.up * 0.5f;
        Gizmos.DrawRay(start, Vector3.down * rayDistance);
    }
}
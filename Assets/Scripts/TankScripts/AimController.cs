using UnityEngine;
using UnityEngine.InputSystem;

public class AimController: MonoBehaviour
{
    private enum Axis
    {
        X,
        Y,
        Z
    }

    //Gets the aim input
    [SerializeField] InputActionReference aimDeltaAction; 

    //Variables to set users aim
    [SerializeField] float aimSensitivity = 4f;
    [SerializeField] float turretTurnSpeed = 120f;

    //grabs the obejects to transform
    [SerializeField] Transform turretTransform;   
    [SerializeField] Transform cannonTransform;   

    [Header("Axis Setup")]
    [SerializeField] Axis turretYawAxis = Axis.Y;

    [SerializeField] bool autoResolveAimAxis = true;

    [SerializeField] Vector3 manualLocalAimAxis = Vector3.forward;

    //private variables to use later
    private Vector2 aimInput;
    private float turretYawOffset;
    private Vector3 turretBaseEuler;
    private Vector3 cannonBaseEuler;
    private Vector3 resolvedLocalAimAxis = Vector3.forward;
    private bool hasResolvedAimAxis;

    void Awake()
    {
        if (turretTransform != null)
            turretBaseEuler = turretTransform.localEulerAngles;

        if (cannonTransform != null)
        {
            cannonBaseEuler = cannonTransform.localEulerAngles;
        }

        TryResolveAimAxis();
    }

    void Update()
    {
        //Gets the aim input
        aimInput = aimDeltaAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;

        if (turretTransform == null || cannonTransform == null)
            return;

        bool sharedTurretAndCannon = turretTransform == cannonTransform;

        //Rotates the turret weapon
        float yaw = aimInput.x * aimSensitivity * Time.deltaTime;
        turretYawOffset += yaw * turretTurnSpeed;

        float turretBaseYaw = NormalizeSignedAngle(GetAxisValue(turretBaseEuler, turretYawAxis));

        if (sharedTurretAndCannon)
        {
            Vector3 combinedEuler = turretBaseEuler;
            combinedEuler = SetAxisValue(combinedEuler, turretYawAxis, turretBaseYaw + turretYawOffset);
            turretTransform.localEulerAngles = combinedEuler;
            return;
        }

        turretTransform.localEulerAngles = SetAxisValue(turretBaseEuler, turretYawAxis, turretBaseYaw + turretYawOffset);

        // Keep cannon hard-attached to turret with no pitch changes.
        if (cannonTransform != null)
            cannonTransform.localEulerAngles = cannonBaseEuler;
    }

    public Vector3 GetAimDirection()
    {
        if (cannonTransform != null)
        {
            if (autoResolveAimAxis)
            {
                if (!hasResolvedAimAxis)
                    TryResolveAimAxis();

                return cannonTransform.TransformDirection(resolvedLocalAimAxis).normalized;
            }

            Vector3 manualAxis = manualLocalAimAxis.sqrMagnitude > 0.0001f ? manualLocalAimAxis.normalized : Vector3.forward;
            return cannonTransform.TransformDirection(manualAxis).normalized;
        }

        if (turretTransform != null) return turretTransform.forward;
        return transform.forward;
    }

    public Transform GetCannonTransform()
    {
        return cannonTransform;
    }

    public Transform GetTurretTransform()
    {
        return turretTransform;
    }

    private static float NormalizeSignedAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private static float GetAxisValue(Vector3 euler, Axis axis)
    {
        switch (axis)
        {
            case Axis.X: return euler.x;
            case Axis.Y: return euler.y;
            default: return euler.z;
        }
    }

    private static Vector3 SetAxisValue(Vector3 euler, Axis axis, float value)
    {
        switch (axis)
        {
            case Axis.X:
                euler.x = value;
                break;
            case Axis.Y:
                euler.y = value;
                break;
            default:
                euler.z = value;
                break;
        }

        return euler;
    }

    private Vector3 ResolveBestAimDirection(Transform source)
    {
        Vector3 referenceForward = turretTransform != null ? turretTransform.forward : transform.forward;
        Vector3 referencePlanar = Vector3.ProjectOnPlane(referenceForward, Vector3.up);

        if (referencePlanar.sqrMagnitude < 0.0001f)
            referencePlanar = referenceForward;

        referencePlanar.Normalize();

        Vector3[] localCandidates =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down
        };

        Vector3 bestLocalAxis = Vector3.forward;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < localCandidates.Length; i++)
        {
            Vector3 worldDirection = source.TransformDirection(localCandidates[i]).normalized;
            Vector3 worldPlanar = Vector3.ProjectOnPlane(worldDirection, Vector3.up);

            if (worldPlanar.sqrMagnitude < 0.0001f)
                continue;

            worldPlanar.Normalize();
            float score = Vector3.Dot(worldPlanar, referencePlanar);

            if (score > bestScore)
            {
                bestScore = score;
                bestLocalAxis = localCandidates[i];
            }
        }

        return bestLocalAxis.normalized;
    }

    private void TryResolveAimAxis()
    {
        if (cannonTransform == null)
        {
            hasResolvedAimAxis = false;
            return;
        }

        resolvedLocalAimAxis = ResolveBestAimDirection(cannonTransform);
        hasResolvedAimAxis = true;
    }
}
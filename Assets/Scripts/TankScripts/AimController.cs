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
    [SerializeField] float maxYawRate = 180f;

    //grabs the obejects to transform
    [SerializeField] Transform turretTransform;   
    [SerializeField] Transform cannonTransform;   

    [Header("Axis Setup")]
    [SerializeField] Axis turretYawAxis = Axis.Y;

    [SerializeField] bool autoResolveAimAxis = true;

    [SerializeField] Vector3 manualLocalAimAxis = Vector3.forward;

    [Header("Targeting")]
    [SerializeField] Camera mainCamera;
    [SerializeField] LayerMask targetLayerMask = -1;
    [SerializeField] LayerMask losBlockerMask = -1;
    [SerializeField] float maxCameraDistance = 1000f;
    [SerializeField] LayerMask ignoreLayerMask = 0;
    [SerializeField] bool useWeaponForwardAsPrimary = true;

    //private variables to use later
    private Vector2 aimInput;
    private float turretYawOffset;
    private Quaternion turretBaseLocalRotation;
    private Quaternion cannonBaseLocalRotation;
    private Vector3 resolvedLocalAimAxis = Vector3.forward;
    private bool hasResolvedAimAxis;
    private Vector3 cameraTarget;
    private Vector3 actualTarget;
    private bool hasValidTarget;

    void Awake()
    {
        if (turretTransform != null)
            turretBaseLocalRotation = turretTransform.localRotation;

        if (cannonTransform != null)
            cannonBaseLocalRotation = cannonTransform.localRotation;

        TryResolveAimAxis();

        if (mainCamera == null)
            mainCamera = ResolveGameplayCamera();

        ApplyTurretRotation();
    }

    void Update()
    {
        //Gets the aim input
        aimInput = aimDeltaAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;

        float rawYawRate = aimInput.x * aimSensitivity * turretTurnSpeed;
        float clampedYawRate = Mathf.Clamp(rawYawRate, -maxYawRate, maxYawRate);
        turretYawOffset += clampedYawRate * Time.deltaTime;

        turretYawOffset = Mathf.Repeat(turretYawOffset + 180f, 360f) - 180f;

        ApplyTurretRotation();

        UpdateTargeting();
    }

    private void ApplyTurretRotation()
    {
        if (turretTransform == null || cannonTransform == null)
            return;

        bool sharedTurretAndCannon = turretTransform == cannonTransform;

        Vector3 localYawAxis = GetAxisVector(turretYawAxis);
        Quaternion yawRotation = Quaternion.AngleAxis(turretYawOffset, localYawAxis);
        Quaternion targetTurretRotation = turretBaseLocalRotation * yawRotation;

        turretTransform.localRotation = targetTurretRotation;

        if (!sharedTurretAndCannon)
            cannonTransform.localRotation = cannonBaseLocalRotation;
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

    public Vector3 GetTargetPosition()
    {
        return actualTarget;
    }

    public Vector3 GetCameraTarget()
    {
        return cameraTarget;
    }

    public bool HasValidTarget()
    {
        return hasValidTarget;
    }

    private static Vector3 GetAxisVector(Axis axis)
    {
        switch (axis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            default:
                return Vector3.forward;
        }
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

    private void UpdateTargeting()
    {
        int cameraMask = targetLayerMask & ~ignoreLayerMask.value;
        int losMask = losBlockerMask & ~ignoreLayerMask.value;

        if (useWeaponForwardAsPrimary && cannonTransform != null)
        {
            Vector3 weaponRayDirection = GetAimDirection();
            Vector3 weaponRayOrigin = cannonTransform.position + weaponRayDirection * 0.35f;

            if (TryRaycastIgnoringSelf(weaponRayOrigin, weaponRayDirection, maxCameraDistance, losMask, out RaycastHit weaponHit))
                actualTarget = weaponHit.point;
            else
                actualTarget = weaponRayOrigin + weaponRayDirection * maxCameraDistance;

            cameraTarget = actualTarget;
            hasValidTarget = true;
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            hasValidTarget = false;
            return;
        }

        Vector3 cameraRayOrigin = mainCamera.transform.position;
        Vector3 cameraRayDirection = mainCamera.transform.forward;

        if (Physics.Raycast(cameraRayOrigin, cameraRayDirection, out RaycastHit cameraHit, maxCameraDistance, cameraMask))
            cameraTarget = cameraHit.point;
        else
            cameraTarget = cameraRayOrigin + cameraRayDirection * maxCameraDistance;

        Transform weapon = cannonTransform != null ? cannonTransform : turretTransform;
        if (weapon != null)
        {
            Vector3 weaponToTarget = (cameraTarget - weapon.position).normalized;
            float distanceToTarget = Vector3.Distance(weapon.position, cameraTarget);

            Vector3 weaponOrigin = weapon.position + weaponToTarget * 0.35f;
            if (TryRaycastIgnoringSelf(weaponOrigin, weaponToTarget, distanceToTarget, losMask, out RaycastHit losHit))
                actualTarget = losHit.point;
            else
                actualTarget = cameraTarget;

            hasValidTarget = true;
            return;
        }

        actualTarget = cameraTarget;
        hasValidTarget = true;
    }

    bool TryRaycastIgnoringSelf(Vector3 origin, Vector3 direction, float distance, int mask, out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, mask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            hit = default;
            return false;
        }

        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        Transform selfRoot = transform.root;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider candidateCollider = hits[i].collider;
            if (candidateCollider == null)
                continue;

            Transform candidateTransform = candidateCollider.transform;
            if (candidateTransform != null && candidateTransform.IsChildOf(selfRoot))
                continue;

            hit = hits[i];
            return true;
        }

        hit = default;
        return false;
    }

    private static Camera ResolveGameplayCamera()
    {
        Camera taggedMain = Camera.main;
        if (IsGameplayCamera(taggedMain))
            return taggedMain;

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate == null)
                continue;

            if (candidate.name.Contains("PlayerCamera"))
                return candidate;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (IsGameplayCamera(candidate))
                return candidate;
        }

        return taggedMain;
    }

    private static bool IsGameplayCamera(Camera camera)
    {
        if (camera == null || !camera.isActiveAndEnabled)
            return false;

        string cameraName = camera.name.ToLowerInvariant();
        if (cameraName.Contains("minimap"))
            return false;

        return true;
    }

}
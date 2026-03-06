using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //Sets up look action and transform object to grab
    [SerializeField] InputActionReference lookAction;     
    [SerializeField] Transform followTarget;             
    [SerializeField] string preferredTargetName = "minitank-v10-green 1";
    [SerializeField] bool forcePreferredTarget = true;

    //Variables to change how the camera feels when moving
    [SerializeField] Vector3 offset = new Vector3(2.2f, 2.2f, -8.2f);
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] float minPitch = -8f;
    [SerializeField] float maxPitch = 35f;
    [SerializeField] float lookAtHeight = 1.25f;

    //Private variable to hold rotation
    private float yaw = 0;
    private float pitch = 0;
    private float targetYaw;

    void Start()
    {
        EnsurePreferredFollowTarget();

        if (followTarget == null)
            followTarget = ResolveDefaultFollowTarget();

        if (followTarget == null)
            return;

        targetYaw = followTarget.eulerAngles.y;
        yaw = 0f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }
    void OnEnable()
    {
        if (lookAction?.action != null)
            lookAction.action.Enable();
    }

    void OnDisable()
    {
        if (lookAction?.action != null)
            lookAction.action.Disable();
    }

    void LateUpdate()
    {
        EnsurePreferredFollowTarget();

        if (followTarget == null)
        {
            followTarget = ResolveDefaultFollowTarget();
            if (followTarget == null)
                return;
        }

        //Gets look action
        Vector2 look = lookAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;


        //Horizontal rotation 
        targetYaw = followTarget.eulerAngles.y;

        // Accumulate RELATIVE rotations (mouse/stick)
        yaw += look.x * rotationSpeed * Time.deltaTime;
        float pitchInput = look.y * rotationSpeed * Time.deltaTime;

        pitch += pitchInput;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // ABSOLUTE rotation: tank yaw + relative camera offset
        Quaternion rot = Quaternion.Euler(pitch, targetYaw + yaw, 0);

        // Position: rotate offset around target
        transform.position = followTarget.position + rot * offset;

        // Look at target (slight up bias for better view)
        transform.LookAt(followTarget.position + followTarget.up * lookAtHeight);
    }

    void EnsurePreferredFollowTarget()
    {
        if (!forcePreferredTarget)
            return;

        if (string.IsNullOrWhiteSpace(preferredTargetName))
            return;

        GameObject preferred = GameObject.Find(preferredTargetName);
        if (preferred == null)
            return;

        if (followTarget != preferred.transform)
            followTarget = preferred.transform;
    }

    static Transform ResolveDefaultFollowTarget()
    {
        GameObject preferredGreenTank = GameObject.Find("minitank-v10-green 1");
        if (preferredGreenTank != null)
            return preferredGreenTank.transform;

        GameObject greenTank = GameObject.Find("minitank-v10-green");
        if (greenTank != null)
            return greenTank.transform;

        GameObject taggedPlayer = null;
        try
        {
            taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        }
        catch (UnityException)
        {
        }

        if (taggedPlayer != null)
            return taggedPlayer.transform;

        TankController tankController = FindFirstObjectByType<TankController>();
        if (tankController != null)
            return tankController.transform;

        return null;
    }
}
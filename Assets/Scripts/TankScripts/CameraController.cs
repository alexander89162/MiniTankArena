using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    //Sets up look action and transform object to grab
    [SerializeField] InputActionReference lookAction;     
    [SerializeField] Transform followTarget;             

    //Variables to change how the camera feels when moving
    [SerializeField] Vector3 offset = new Vector3(0, 3f, -8f);
    [SerializeField] float rotationSpeed = 120f;
    [SerializeField] float minPitch = -20f;
    [SerializeField] float maxPitch = 70f;

    //Private variable to hold rotation
    private float yaw = 0;
    private float pitch = 0;
    private float targetYaw;

    void Start()
    {
        targetYaw = followTarget.eulerAngles.y;
        yaw = 180;
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
        transform.LookAt(followTarget.position + followTarget.up * 1.5f);
    }
}
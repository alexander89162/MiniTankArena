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
    private Vector3 currentRotation = Vector3.zero;

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
        currentRotation.y += look.x * rotationSpeed * Time.deltaTime;

        //Vertical rotation
        currentRotation.x += look.y * rotationSpeed * Time.deltaTime;
        currentRotation.x = Mathf.Clamp(currentRotation.x, minPitch, maxPitch);

        //Apply rotation
        Quaternion rot = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        transform.position = followTarget.position + rot * offset;
        transform.LookAt(followTarget.position + Vector3.up * 1.5f); 
    }

    public void ResetCamera()
    {
        currentRotation = Vector3.zero;
    }
}
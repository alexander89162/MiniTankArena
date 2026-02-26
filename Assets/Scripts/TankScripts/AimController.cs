using UnityEngine;
using UnityEngine.InputSystem;

public class AimController: MonoBehaviour
{
    //Gets the aim input
    [SerializeField] InputActionReference aimDeltaAction; 

    //Variables to set users aim
    [SerializeField] float aimSensitivity = 4f;
    [SerializeField] float turretTurnSpeed = 120f;
    [SerializeField] float cannonElevationSpeed = 60f;
    [SerializeField] float maxElevation = 20f;
    [SerializeField] float minElevation = -8f;

    //Sets the space to world to avoid weird transformation bug
    [SerializeField] Space rotationSpace = Space.World; 

    //grabs the obejects to transform
    [SerializeField] Transform turretTransform;   
    [SerializeField] Transform cannonTransform;   

    //private variables to use later
    private Vector2 aimInput;
    private float turretYawOffset;              

    void Update()
    {
        //Gets the aim input
        aimInput = aimDeltaAction?.action?.ReadValue<Vector2>() ?? Vector2.zero;

        //Rotates the turret weapon
        float yaw = aimInput.x * aimSensitivity * Time.deltaTime;
        turretYawOffset += yaw * turretTurnSpeed;

        turretTransform.Rotate(0, yaw * turretTurnSpeed, 0, rotationSpace);

        //Cannon muzzle to show where its aiming
        float pitch = -aimInput.y * aimSensitivity * Time.deltaTime;
        Vector3 localRot = cannonTransform.localEulerAngles;
        float currentPitch = (localRot.x > 180) ? localRot.x - 360 : localRot.x;
        float newPitch = currentPitch - pitch * cannonElevationSpeed;
        newPitch = Mathf.Clamp(newPitch, minElevation, maxElevation);
        
        
        if (newPitch < 0) newPitch += 360f;
        cannonTransform.localEulerAngles = new Vector3(newPitch, localRot.y, localRot.z);
    }

    public Vector3 GetAimDirection()
    {
        if (cannonTransform != null) return cannonTransform.forward;
        if (turretTransform != null) return turretTransform.forward;
        return transform.forward;
    }
}
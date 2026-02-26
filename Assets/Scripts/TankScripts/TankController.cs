using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    //Private variable to grab character controller
    private CharacterController controller;

    //Slope Align Tank Func
    public TankSlope tankSlope;

    //To place tank turrents to array for cycling
    public Transform[] turrets = new Transform[3];

    //Private variable to set current turrent
    private int turretIndex = 0;
    private Transform activeTurret;

    //Variables that move tank or apply gravity, helps simulate physics
    public float maxSpeed;                 
    public float accelerationTime;          
    public float decelerationTime;          
    public float backwardMultiplier;       
    public float rotationSpeed;            
    public float gravity;   


    //Private variables that help with velocity of tank, and direction
    private Vector2 moveInput;                     
    private Vector3 currentVelocity = Vector3.zero;
    private float verticalVelocity = 0f;
    private Vector3 currentVelocityV = Vector3.zero; 

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        SwitchToTurret(turretIndex);

    if (tankSlope != null)
    {
        tankSlope.tankRoot = transform;  // Pass root reference
    }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnCycleTurret(InputAction.CallbackContext context)
    {
        if (context.performed)  // On press only
        {
            turretIndex = (turretIndex + 1) % 3;  // Cycle 0→1→2→0
            SwitchToTurret(turretIndex);
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        //Rotates tank
        float turnInput = moveInput.x;
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float rotationThisFrame = turnInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0, rotationThisFrame, 0);
        }

        //Dircetion and speed
        Vector3 targetDirection = transform.forward * moveInput.y;
        float targetSpeed = Mathf.Abs(moveInput.y) * maxSpeed;

        //Reverses
        if (moveInput.y < 0)
            targetSpeed *= backwardMultiplier;

        //Acceleration using smoothdamp to simulate physics 
        float accelSmoothTime = moveInput.y != 0 ? accelerationTime : decelerationTime;
        currentVelocity = Vector3.SmoothDamp(
            currentVelocity,
            targetDirection * targetSpeed,
            ref currentVelocityV,   
            accelSmoothTime
        );

        //Grativity applied to tank
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; 
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        //Final move
        Vector3 move = currentVelocity;
        move.y = verticalVelocity;

        //Applies the movement
        controller.Move(move * Time.deltaTime);

        //Reset vertical velocity when grounded
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (tankSlope != null)
        {
            tankSlope.UpdateAlignment(currentVelocity);
        }
    }

    void SwitchToTurret(int index)
    {
        for (int i = 0; i < turrets.Length; i++)
            if (turrets[i] != null) turrets[i].gameObject.SetActive(i == index);

        if (turrets[index] != null)
        {
            activeTurret = turrets[index];
        }
    }
}
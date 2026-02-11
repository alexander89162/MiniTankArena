using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    public InputActionAsset InputActions;
    
    private InputAction moveAction;
    private InputAction aimAction;
    
    private Vector2 moveAmount;
    private Vector2 aimAmount;
    private Rigidbody rb;

    public float speed = 100;
    public float rotateSpeed = 10;

/*
    private void OnEnable()
    {
        InputActions.FindAction("Tanks").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindAction("Tanks").Disable();
    }
*/
    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        aimAction = InputSystem.actions.FindAction("Aim");

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        moveAmount = moveAction.ReadValue<Vector2>();
        aimAmount = aimAction.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Move();
        //Aim();
    }

    private void Move()
    {
       rb.MovePosition(rb.position+transform.forward*moveAmount.y*speed*Time.deltaTime); 
    
        float rotAmount = moveAmount.x * rotateSpeed*Time.deltaTime;
        Quaternion delataRot = Quaternion.Euler(0,rotAmount,0);
        rb.MoveRotation(rb.rotation * delataRot);
    }   
}
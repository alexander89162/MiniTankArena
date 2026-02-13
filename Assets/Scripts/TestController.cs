using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TestController : MonoBehaviour
{
    public Vector2 moveInput;  // for debugging

    [Header("Movement")]
    public float motorForce = 4500f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // MUST be public, exact name "OnMove", exact parameter type
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log("OnMove called! Value = " + moveInput);  // ← must see this in console
    }

    // Optional: test Aim too
    public void OnAim(InputAction.CallbackContext context)
    {
        Debug.Log("OnAim called!");
    }

    void FixedUpdate()
    {
        // Simple test movement — no fancy stuff yet
        rb.AddForce(transform.forward * moveInput.y * motorForce);
        rb.AddTorque(Vector3.up * moveInput.x * 200f);
    }
}
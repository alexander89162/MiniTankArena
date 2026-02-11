using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public InputActionAsset InputActions;
    public float speed = 10f;
    public float turnSpeed = 100f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical") * speed;
        float turn = Input.GetAxis("Horizontal") * turnSpeed;

        Vector3 forwardMove = transform.forward * move * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        Quaternion turnRot = Quaternion.Euler(0f, turn * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turnRot);
    }
}
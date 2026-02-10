using UnityEngine;
using UnityEngine.InputSystem;

/* This module uses player input to control the character */

public class PlayerBrain : IBrainModule
{
    private PlayerInput playerInput;
    private Transform weapon;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction attackAction;

    public Vector3 desiredMove { get; private set; }
    public Vector3 desiredLook { get; private set; }
    public bool wantsToAttack { get; private set; }

    public PlayerBrain(PlayerInput input, Transform weaponTarget)
    {
        playerInput = input;
        weapon = weaponTarget;

        // Grab actions from PlayerInput asset
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        attackAction = input.actions["Attack"];

        // Enable actions
        moveAction.Enable();
        lookAction.Enable();
        attackAction.Enable();  
    }
    
    public void Tick()
    {
        // Read values from the input system
        Vector2 moveInput = moveAction.ReadValue<Vector2>(); // X=horizontal, Y=vertical
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // 1. Map input to world-space movement vector
        desiredMove = new Vector3(moveInput.x, 0f, moveInput.y);

        // Map mouse/gamepad delta to a look vector (assuming planar rotation)
        desiredLook = new Vector3(lookInput.x, 0f, lookInput.y);

        // 3. Attack intent
        // future change: make isPressed() or wasPressedThisFrame() varying per 
        // IAttackModule implementation (send this decision to attack module to decide)
        wantsToAttack = attackAction.IsPressed();
    }

}
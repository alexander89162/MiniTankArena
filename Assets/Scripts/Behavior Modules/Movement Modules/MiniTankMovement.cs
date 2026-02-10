using UnityEngine;

public class MiniTankMovement : IMovementModule
{
    BaseController controller;
    Transform root;
    Vector3 planarMove;
    float moveSpeed;
    
    public MiniTankMovement(BaseController controller, Transform root, float moveSpeed)
    {
        this.controller = controller;
        this.root = root;
        this.moveSpeed = moveSpeed;
    }
    public void Tick()
    {
        moveSpeed = controller.movementSpeed; // DEBUG: update moveSpeed to keep synced

        Vector3 attemptedMove = controller.brainModule.desiredMove;

        // Move the body in world XZ plane
        planarMove = new Vector3(attemptedMove.x, 0f, attemptedMove.z);

        // DEBUG
        controller.planarMove = planarMove;
        
        root.position += planarMove * moveSpeed * Time.deltaTime;
    }
}
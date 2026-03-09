using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Given a series of positions along a path, this component automatically 
makes the drone move along the path and adjust speed as needed. It also
animates the drone automatically based on the specified movesQueue in a JSON 
(or hard-coded test case)*/
public class DroneController : MonoBehaviour
{
    [Tooltip("The positions that will be crossed by the drone (assign nodes first to last)")]
    public Vector3[] pathNodes;
    private Queue<Move> movesQueue;

    private Vector3 nextPos; // next "checkpoint" to reach
    private int currentNodeIndex = 0;
    private Vector3 move;

    private enum AnimationState
    {
        Forward, // leans the drone forward and tilts based on next path node
        StabilizingFromStop, // rock back and forth to simulate realistic stopping
        Idle, // Rock up and down, small sideways noise which should always return to original position and tilt gently while doing so
    }

    AnimationState currentState = AnimationState.Forward;
    private Quaternion defaultRotation; // we use this to remember the forward-facing direction

    private struct Move
    {
        readonly float startVelocity; // velocity when at this node
        readonly float endVelocity; // velocity by the time of reaching the next node
        readonly string accelerationType; // "linear" or "quadratic"
        readonly Vector3 startPos;
        readonly Vector3 endPos;
        Quaternion targetRotation; // the drone should be mostly facing in this direction
        readonly string rotationType; // "linear" or "Slerp"
    }

    void Awake()
    {
        defaultRotation = transform.localRotation;
    }

    void Update()
    {
        switch (currentState)
        {
            case AnimationState.Forward:
                break;
            case AnimationState.StabilizingFromStop:
                break;
            case AnimationState.Idle:
                break;
        }
    }

    public void BuildMovesQueue()
    /*Use the given pathNodes to compute each move the drone should undergo, then
    put the moves into the movesQueue*/
    {
        return;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

/*Given a series of Move instructions, this component makes the drone move along 
the path and interpolate values as needed to animate the drone automatically*/
public class DroneController : MonoBehaviour
{
    public string droneActions; // the file containing the specific actions this drone will follow
    public string droneActionsPath; // directory that contains droneActions file

    private List<Move> moves;
    private List<BrakingManeuver> brakingManeuvers;
    private List<DeploymentAction> deploymentActions;
    private int currentNodeIndex = 0;
    AnimationState currentState = AnimationState.Forward;

    private enum AnimationState
    {
        InitializingController, // not ready to move or animate yet
        Forward, // leans the drone forward and tilts based on next path node
        StabilizingFromStop, // rock back and forth to simulate realistic stopping
        Idle, // Rock up and down, small sideways noise which should always return to original position and tilt gently while doing so
    }

    public class DroneActions
    {
        public readonly Move[] moves;
        public readonly BrakingManeuver[] brakingManeuvers;
        public readonly DeploymentAction[] deploymentActions;
    }

    public struct Move
    {
        public readonly int moveId;
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float endVelocity;
        public readonly string accelerationType; // "linear" or "quadratic"
        public readonly string rotationType; // "linear" or "Slerp"
    }

    public readonly struct BrakingManeuver
    {
        public readonly Quaternion rotation;
        public readonly float duration;
        public readonly float outwardMove;
    }

    public readonly struct DeploymentAction
    {
        public readonly string action;
        public readonly int activationNode;
        public readonly float startDelay;
        public readonly float duration;
    }

    // Allowed interpolation methods
    private static readonly HashSet<string> allowedAccelerationTypes = new HashSet<string>{ "linear", "quadratic" };
    private static readonly HashSet<string> allowedRotationTypes = new HashSet<string>{ "linear", "slerp" };

    void Awake()
    {
        SetState(AnimationState.InitializingController);
        StartCoroutine(InitializeDroneActions());
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

    private IEnumerator InitializeDroneActions()
    /*Extract drone path data from JSON*/
    {
        // 1) Extract data from json
        string path = System.IO.Path.Combine(droneActionsPath, System.String.Concat(droneActions, ".json"));

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load drone actions: " + request.error);
            yield break;
        }

        DroneActions actions =
            JsonUtility.FromJson<DroneActions>(
                request.downloadHandler.text
            );

        // 2) Validation - fail = delete this drone to avoid crashes
        if (!ValidateDroneActions(actions))
        {
            Debug.LogError($"DroneController on {name} received invalid action data. Destroying drone.");
            Destroy(gameObject);
            yield break;
        }

        // 3) Cache the data we need
        moves = new List<Move>(actions.moves);
        brakingManeuvers = new List<BrakingManeuver>(actions.brakingManeuvers);
        deploymentActions = new List<DeploymentAction>(actions.deploymentActions);

        // 4) Done initializing
        SetState(AnimationState.Forward);
    }

    private bool ValidateDroneActions(DroneActions actions)
    {
        // 1) Interpolation methods must be valid
        for (int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            if (string.IsNullOrEmpty(move.accelerationType) || !allowedAccelerationTypes.Contains(move.accelerationType.ToLower()))
            {
                Debug.LogError($"Acceleration type is invalid or missing");
                return false;   
            }
            if (string.IsNullOrEmpty(move.rotationType) || !allowedRotationTypes.Contains(move.rotationType.ToLower()))
            {
                Debug.LogError($"Rotation type is invalid or missing");
                return false;   
            }
        }

        return true;
    }

    private void SetState(AnimationState newState){ currentState = newState; }
}
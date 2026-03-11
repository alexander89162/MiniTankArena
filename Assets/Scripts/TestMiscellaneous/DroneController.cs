using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*Given a series of Move instructions, this component automatically moves 
and animates the drone*/
public class DroneController : MonoBehaviour
{
    public string droneActions; // the file containing the specific actions this drone will follow
    public string droneActionsPath; // directory that contains droneActions file
    private List<Move> moves;
    private List<BrakingManeuver> brakingManeuvers;
    private List<DeploymentAction> deploymentActions;
    private int currentNodeIndex = 0;
    ControllerState currentState = ControllerState.Forward;

    private enum ControllerState
    {
        InitializingController, // not ready to move or animate yet
        Forward, // leans the drone forward and tilts based on next path node
        StabilizingFromStop, // rock back and forth to simulate realistic stopping
        Idle, // Rock up and down, small sideways noise which should always return to original position and tilt gently while doing so
    }

    public class DroneActions
    {
        public readonly MoveJson[] moves;
        public readonly BrakingManeuver[] brakingManeuvers;
        public readonly DeploymentAction[] deploymentActions;
    }

    public struct Move
    {
        public readonly int moveId;
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly float endVelocity;
        public readonly AccelerationType accelerationType; // "linear" or "quadratic"
        public readonly RotationType rotationType; // "linear" or "Slerp"
        public Move(MoveJson json)
        {
            moveId = json.moveId;
            position = json.position;
            rotation = Quaternion.Euler(json.rotation);
            endVelocity = json.endVelocity;

            accelerationType =
                System.Enum.TryParse(json.accelerationType, true, out AccelerationType a)
                ? a : AccelerationType.Unknown;

            rotationType =
                System.Enum.TryParse(json.rotationType, true, out RotationType r)
                ? r : RotationType.Unknown;
        }
    }

    [System.Serializable]
    public struct MoveJson
    {
        public int moveId;
        public Vector3 position;
        public Vector3 rotation;
        public float endVelocity;
        public string accelerationType;
        public string rotationType;
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

    public enum AccelerationType
    {
        Unknown,
        Linear,
        Quadratic
    }

    public enum RotationType
    {
        Unknown,
        Linear,
        Slerp
    }

    // Allowed interpolation methods
    private static readonly HashSet<string> allowedRotationTypes = new HashSet<string>{ "linear", "slerp" };

    void Awake()
    {
        SetState(ControllerState.InitializingController);
        StartCoroutine(InitializeDroneActions());
    }

    void Update()
    {
        switch (currentState)
        {
            case ControllerState.InitializingController: break;
            case ControllerState.Forward:
                //
                break;
            case ControllerState.StabilizingFromStop:
                break;
            case ControllerState.Idle:
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
            Destroy(gameObject);
            yield break;
        }

        DroneActions actions =
            JsonUtility.FromJson<DroneActions>(
                request.downloadHandler.text
            );

        // 2) Validation - fail = delete this drone to avoid crashes
        if (!ValidateDroneActions())
        {
            Debug.LogError($"DroneController on {name} received invalid action data. Destroying drone.");
            Destroy(gameObject);
            yield break;
        }

        // 3) Cache the data we need
        moves = new List<Move>(actions.moves.Length);
        foreach (var m in actions.moves)
            moves.Add(new Move(m));
        brakingManeuvers = new List<BrakingManeuver>(actions.brakingManeuvers);
        deploymentActions = new List<DeploymentAction>(actions.deploymentActions);

        // 4) Done initializing
        SetState(ControllerState.Forward);
    }

    private bool ValidateDroneActions()
    {
        // 1) Interpolation methods must be valid
        for (int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            if (!System.Enum.IsDefined(typeof(AccelerationType), move.accelerationType) || move.accelerationType == AccelerationType.Unknown)
            {
                Debug.LogError($"Acceleration type is invalid.");
                return false;   
            }
            if (!System.Enum.IsDefined(typeof(RotationType), move.rotationType) || move.rotationType == RotationType.Unknown)
            {
                Debug.LogError($"Rotation type is invalid.");
                return false;   
            }
        }

        return true;
    }

    private float ApplyAcceleration(float t, AccelerationType type)
    {
        switch (type)
        {
            case AccelerationType.Linear: return t;
            case AccelerationType.Quadratic: return t * t;
            default: return t;
        }
    }

    private void SetState(ControllerState newState){ currentState = newState; }
}
using UnityEngine;
using UnityEngine.InputSystem;

/*  BaseController defines only fundamental characteristics, 
    IBrainModule implementation decides if we take player input (use PlayerBrain)
    and if not, decide which AI to use. The IBrainModule decides the 
    high-level strategy by deciding intent and representing it as a series
    of primitive values (boolean, vectors for direction, etc) ready for
    use by the other modules. All modules are invoked by BaseController.
*/

public class BaseController : MonoBehaviour
{
    [Header("Root")]
    public Transform weaponTarget;

    [Header("Rig dependencies")] // for animation only
    public Transform[] legTargets;
    public Transform[] legHints;
    public Vector3[] hintOffsets;


    [Header("Core Stats")] // configure to test for now; will be changed later to load upgrades from JSON
    public int level = 1;
    public float health = 100f;
    public float movementSpeed = 2f;
    public int team; // allow tanks to fight by your side, against each other, etc

    // Modules -- these cannot be assigned in inspector because they're implementations of an interface, so they can only be hard-coded for now and later on assigned by the spawner
    public IBrainModule brainModule;
    public IMovementModule movementModule;
    public IAnimationModule animationModule;
    public IAttackModule attackModule;

    public Vector3 planarMove = Vector3.zero;

    void Awake()
    {
        // TODO: Instantiate rig (?)

        // Set level, health, movement speed, and team.
        // If it's the player, then load the account's values from JSON
        if (gameObject.CompareTag("Player"))
        {
            team = 0;
            
            // Load from JSON
        }
        else
        {
            // Set based on configurations
        }

        // Assign modules
        // NOTE: this is temporarily hard-coded but it should be handled via a unit spawner
        brainModule = new PlayerBrain(GetComponent<PlayerInput>(), weaponTarget);
        movementModule = new MiniTankMovement(this, this.transform, movementSpeed);
        attackModule = new CannonAttack();
        animationModule = new MiniTankAnimation(this);
    }

    void FixedUpdate()
    {
        brainModule?.Tick(); // update intent, which serves as input for the other modules
        movementModule?.Tick(); // perform logical movement; translations but not animation, which belongs to the animation module
        attackModule?.Tick(); // attack if desired and allowed to
    }

    void LateUpdate()
    {
        animationModule?.Tick(); // purely visual
    }

    // Setters
    public void SetBrainModule(IBrainModule brainModule) { this.brainModule = brainModule; }
    public void SetMovementModule(IMovementModule movementModule) { this.movementModule = movementModule; }
    public void SetAttackModule(IAttackModule attackModule) { this.attackModule = attackModule; }
    public void SetAnimationModule(IAnimationModule animationModule) { this.animationModule = animationModule; }
}
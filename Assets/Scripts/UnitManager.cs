using UnityEngine;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    private readonly int poolSize = 64;    
    private UnitData[] unitHandles = new UnitData[64]; // list of all units
    
    // module parallel arrays
    private IBrainModule[] brains;
    private IMovementModule[] movements;
    private IAttackModule[] attacks;
    private IAnimationModule[] animations;

    void Awake()
    {
        brains = new IBrainModule[64];
        movements = new IMovementModule[64];
        attacks = new IAttackModule[64];
        animations = new IAnimationModule[64];
    }

    // Add a unit by making sure the configuration is correct for the specified index, then setting start state
    public void RegisterUnit(UnitData unit, int index)
    {
        //
    }

    // Remove a unit by wiping its state and disabling its components
    public void UnregisterUnit(int index)
    {
        //
    }

    void FixedUpdate()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (unitHandles[i] == null) continue;
            
            brains[i]?.Tick();        // update AI / intent
            movements[i]?.Tick();     // move logically
            attacks[i]?.Tick();       // handle shooting / attack
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < poolSize; i++)
        {
            animations[i]?.Tick();    // visual updates only
        }
    }
}

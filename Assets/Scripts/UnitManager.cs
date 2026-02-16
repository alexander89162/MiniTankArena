using System.Collections.Generic;
using UnityEngine;
public class UnitManager : MonoBehaviour
{
    // list of all units
    private List<UnitData> units = new List<UnitData>();


    // module parallel arrays
    private List<IBrainModule> brains = new List<IBrainModule>();
    private List<IMovementModule> movements = new List<IMovementModule>();
    private List<IAttackModule> attacks = new List<IAttackModule>();
    private List<IAnimationModule> animations = new List<IAnimationModule>();

    // Add a unit
    public void RegisterUnit(UnitData unit)
    {
        units.Add(unit);
        brains.Add(unit.brainModule);
        movements.Add(unit.movementModule);
        attacks.Add(unit.attackModule);
        animations.Add(unit.animationModule);
    }

    // Remove a unit (pool instead of destroy)
    public void UnregisterUnit(UnitData unit)
    {
        int index = units.IndexOf(unit);
        if (index < 0) return;

        units.RemoveAt(index);
        brains.RemoveAt(index);
        movements.RemoveAt(index);
        attacks.RemoveAt(index);
        animations.RemoveAt(index);
    }

    void FixedUpdate()
    {
        int count = units.Count;
        for (int i = 0; i < count; i++)
        {
            brains[i]?.Tick();        // update AI / intent
            movements[i]?.Tick();     // move logically
            attacks[i]?.Tick();       // handle shooting / attack
        }
    }

    void LateUpdate()
    {
        int count = units.Count;
        for (int i = 0; i < count; i++)
        {
            animations[i]?.Tick();    // visual updates only
        }
    }
}

using System;
using UnityEngine;

public interface IBrainModule
{
    Vector3 desiredMove { get; }
    Vector3 desiredLook { get; }
    bool wantsToAttack { get; }

    void Tick();
}
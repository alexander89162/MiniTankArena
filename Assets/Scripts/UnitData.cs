using UnityEngine;

public class UnitData
{
    public int id;
    public int team;
    public float health;
    public float movementSpeed;

    // Rig / animation dependencies
    public Transform[] legTargets;
    public Transform[] legHints;
    public Vector3[] hintOffsets;

    // Runtime state
    public Vector3 planarMove;
}

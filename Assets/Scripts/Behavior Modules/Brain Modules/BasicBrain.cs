using UnityEngine;

/*-----TODO-----
The most basic enemy brain module. It will use NavMesh to run at the 
player and shoot whenever the player is in range and not behind 
cover (use a raycast)*/
public class BasicBrain : IBrainModule
{
    public Vector3 desiredMove => throw new System.NotImplementedException();

    public Vector3 desiredLook => throw new System.NotImplementedException();

    public bool wantsToAttack => throw new System.NotImplementedException();
    
    public BasicBrain(){}

    public void Tick(){}
}
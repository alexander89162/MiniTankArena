using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class WeaponSwap
{
    [SerializeField] private Transform convergencePoint; // an Empty that describes the point where the weapon will shrink to/grow from
    private float swapSpeed = 1.4f;
    public void Animate(GameObject weapon1, GameObject weapon2, Transform pos1, Transform pos2, float delta)
    /*Animate the weapon swapping. This method lerps for SwapSpeed seconds, 
    moving the weapon towards the convergencePoint while scaling from 
    original scale to 0. After, it scales up the newly active weapon2.*/
    {
        // if delta is less than halfway, continue lerping weapon1, else lerp weapon2
        if (delta < swapSpeed / 2)
        {
            weapon1.transform.localPosition = Vector3.Lerp(pos1.position, convergencePoint.position, delta / 2);
            weapon1.transform.localScale = Vector3.Lerp(pos1.localScale, Vector3.zero, delta / 2);
        }
        else
        {
            weapon2.transform.localPosition = Vector3.Lerp(convergencePoint.position, pos2.position, delta / 2);
            weapon2.transform.localScale = Vector3.Lerp(Vector3.zero, pos2.localScale, delta / 2);
        }
    }
}
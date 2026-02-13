using UnityEngine;

public class TestCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smooth = 0.125f;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 pos = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, pos, smooth);
        transform.LookAt(target.position + Vector3.up * 2);
    }
}
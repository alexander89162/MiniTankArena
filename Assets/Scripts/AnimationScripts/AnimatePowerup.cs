using UnityEngine;

public class AnimatePowerup : MonoBehaviour
{
    public float floatHeight;
    public float floatSpeed;
    public float rotationSpeed;

    private float phaseOffset;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Move up and down with sine wave
        float y = Mathf.Sin((Time.time + phaseOffset) * floatSpeed) * floatHeight;
        transform.localPosition = startPos + new Vector3(0, y, 0);

        // Rotate at constant speed
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
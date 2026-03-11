using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 60f;
    public float maxLifetime = 8f;
    public Vector3 initialVelocity = Vector3.zero;


    [SerializeField] private float damage = 35f;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Hit Detection")]
    public LayerMask hitLayers;             // only enemies / world — **exclude** your own tank layer!
    public float raycastLengthMultiplier = 1.2f;   // safety margin
    public float initialIgnoreDistance = 1.5f;     // NEW: ignore hits within X meters of spawn

    //private Vector3 velocityDirection;
    private Vector3 velocity;
    private Vector3 spawnPosition;                 // NEW
    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
        spawnPosition = transform.position;        // remember exact spawn point
        //velocityDirection = transform.forward.normalized;
        //velocity = velocity.normalized * (speed + initialVelocity.magnitude * 0.8f);
        velocity = initialVelocity + transform.forward * speed;
    }

    void LateUpdate()
    {
        if (Time.time > spawnTime + maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        float moveThisFrame = velocity.magnitude * Time.deltaTime;
        Vector3 moveDirection = velocity.normalized;

        // Raycast ahead this frame's distance + safety
        Ray ray = new Ray(transform.position, moveDirection);
        float rayDistanceThisFrame = moveThisFrame * raycastLengthMultiplier;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistanceThisFrame, hitLayers))
        {
            // NEW: ignore very close hits (self / muzzle / gun barrel)
            float distanceFromSpawn = Vector3.Distance(hit.point, spawnPosition);
            if (distanceFromSpawn >= initialIgnoreDistance)
            {
                OnHit(hit.point, hit.normal, hit.collider);
                Destroy(gameObject);
                return;
            }
        }

        // Safe to move
        transform.position += moveDirection * moveThisFrame;

        if (velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(velocity);
    }

    private void OnHit(Vector3 point, Vector3 normal, Collider target)
    {
        Debug.Log($"[Projectile] Hit {target.name} for {damage} damage!");

        // **DAMAGE INTEGRATION** — Apply to DamageReceiver
        if (target.TryGetComponent<DamageController>(out DamageController receiver))
        {
            receiver.TakeDamage(damage);
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, point, Quaternion.LookRotation(normal));
        }

        Destroy(gameObject);
    }

    // Optional: keep OnTriggerEnter as fallback / for trigger colliders
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitLayers.value) != 0)
        {
            float distFromSpawn = Vector3.Distance(transform.position, spawnPosition);
            if (distFromSpawn >= initialIgnoreDistance)
            {
                OnHit(transform.position, -transform.forward, other);
                //Destroy(gameObject);
            }
        }
    }
}
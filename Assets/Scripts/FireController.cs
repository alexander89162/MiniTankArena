using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FireController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionReference fireAction;

    [Header("Components")]
    [SerializeField] Transform firePoint;           // muzzle / barrel tip
    [SerializeField] GameObject projectilePrefab;   // bullet / shell prefab
    [SerializeField] float projectileSpeed = 120f;

    [Header("Timing & Feedback")]
    [SerializeField] float reloadTime = 3.5f;
    [SerializeField] float recoilStrength = 800f;
    [SerializeField] AudioSource fireSound;

    [Header("State")]
    public bool CanFire { get; private set; } = true;
    private float reloadTimer;

    private Rigidbody bodyRb;           // optional – for recoil
    private CharacterController bodyCC; // optional – for recoil

    void Awake()
    {
        if (firePoint == null) firePoint = transform;
        bodyRb  = GetComponentInParent<Rigidbody>();
        bodyCC = GetComponentInParent<CharacterController>();
    }

    void OnEnable()
    {
        if (fireAction?.action != null)
            fireAction.action.Enable();
    }

    void OnDisable()
    {
        if (fireAction?.action != null)
            fireAction.action.Disable();
    }

    void Update()
    {
        if (!CanFire) return;

        if (fireAction?.action?.WasPressedThisFrame() ?? false)
        {
            Fire();
        }
    }

    void Fire()
    {
        if (projectilePrefab == null) return;

        // Spawn projectile
        var proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        var rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        // Sound
        if (fireSound != null)
            fireSound.Play();

        // Recoil
        ApplyRecoil(-firePoint.forward * recoilStrength);

        // Start reload
        CanFire = false;
        reloadTimer = reloadTime;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        while (reloadTimer > 0)
        {
            reloadTimer -= Time.deltaTime;
            yield return null;
        }
        CanFire = true;
    }

    private void ApplyRecoil(Vector3 impulse)
    {
        if (bodyRb != null)
        {
            bodyRb.AddForceAtPosition(impulse, firePoint.position, ForceMode.Impulse);
        }
        else if (bodyCC != null)
        {
            // CharacterController – fake recoil with velocity
            // You can expose a public method in your movement script instead
            // For simplicity we apply directly here (not ideal, but works)
            var vel = bodyCC.velocity + impulse / 10f; // scale down
            // Ideally → call method on movement component
        }
    }

    // Optional: UI / progress bar can read this
    public float ReloadNormalized => CanFire ? 1f : 1f - (reloadTimer / reloadTime);
}
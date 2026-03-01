using UnityEngine;
using UnityEngine.UI;

public class CrosshairScript : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [Tooltip("The TargetingSystem for 3rd person aiming")]
    [SerializeField] private TargetingSystem targetingSystem;

    [Tooltip("Fallback aim source when no TargetingSystem is assigned")]
    [SerializeField] private AimController aimController;
    
    [Tooltip("The main camera (usually assigned automatically)")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("If true, crosshair stays at screen center. If false, follows target")]
    [SerializeField] private bool lockToCenter = false;

    [Tooltip("Max distance for aim ray when using AimController fallback")]
    [SerializeField] private float maxAimDistance = 500f;

    [Tooltip("Offset forward from aim source to avoid self-hit")]
    [SerializeField] private float muzzleForwardOffset = 1f;

    [Tooltip("Layer mask for AimController fallback raycast")]
    [SerializeField] private LayerMask aimLayerMask = -1;

    [Tooltip("Draw debug ray for AimController fallback")]
    [SerializeField] private bool showDebugRay = false;
    
    [Header("Visual Settings")]
    [Tooltip("Crosshair color")]
    [SerializeField] private Color crosshairColor = Color.white;
    
    [Tooltip("Size of the crosshair")]
    [SerializeField] private float crosshairSize = 1f;
    
    private RectTransform rectTransform;
    private Image crosshairImage;
    private Canvas canvas;
    
    void Start()
    {
        // Get components
        rectTransform = GetComponent<RectTransform>();
        crosshairImage = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
        
        // Verify Canvas setup
        if (canvas != null)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.LogWarning("CrosshairScript: Canvas should be in Screen Space mode (Overlay or Camera)!");
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }
        else
        {
            Debug.LogError("CrosshairScript: No Canvas found! Crosshair must be a child of a Canvas.");
        }
        
        // Get main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Auto-find TargetingSystem if not assigned
        if (targetingSystem == null)
        {
            targetingSystem = FindFirstObjectByType<TargetingSystem>();
            if (targetingSystem != null)
            {
                Debug.Log("CrosshairScript: Auto-found TargetingSystem");
            }
            else
            {
                Debug.LogWarning("CrosshairScript: No TargetingSystem found in scene. Add one to your player tank.");
            }
        }

        // Auto-find AimController fallback if not assigned
        if (aimController == null)
        {
            aimController = FindFirstObjectByType<AimController>();
        }
        
        // Apply visual settings
        if (crosshairImage != null)
        {
            crosshairImage.color = crosshairColor;
        }
        
        // Set initial scale
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * crosshairSize;
        }
        
        // Hide system cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (rectTransform == null || mainCamera == null)
            return;
        
        if (lockToCenter)
        {
            // Simple mode: keep crosshair at screen center
            CenterCrosshair();
        }
        else
        {
            // Advanced mode: project target to screen
            ProjectTargetToScreen();
        }
    }
    
    void CenterCrosshair()
    {
        // Position at center of the screen
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    void ProjectTargetToScreen()
    {
        Vector3 targetPosition;

        if (targetingSystem != null && targetingSystem.HasValidTarget())
        {
            targetPosition = targetingSystem.GetTargetPosition();
        }
        else if (aimController != null)
        {
            Vector3 aimDirection = aimController.GetAimDirection().normalized;
            Vector3 rayOrigin = aimController.transform.position + aimDirection * muzzleForwardOffset;

            if (Physics.Raycast(rayOrigin, aimDirection, out RaycastHit hit, maxAimDistance, aimLayerMask))
            {
                targetPosition = hit.point;
            }
            else
            {
                targetPosition = rayOrigin + aimDirection * maxAimDistance;
            }

            if (showDebugRay)
                Debug.DrawRay(rayOrigin, aimDirection * maxAimDistance, Color.cyan);
        }
        else
        {
            CenterCrosshair();
            return;
        }
        
        // Convert world position to screen position
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(targetPosition);
        
        // Check if point is in front of camera
        if (screenPoint.z > 0)
        {
            // Convert screen point to canvas position
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out canvasPosition
            );
            
            rectTransform.anchoredPosition = canvasPosition;
        }
        else
        {
            // Point is behind camera, keep at center
            CenterCrosshair();
        }
    }
    
    void OnDestroy()
    {
        // Restore cursor when destroyed
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    // Public methods to change settings at runtime
    public void SetLockToCenter(bool value)
    {
        lockToCenter = value;
    }
    
    public void SetCrosshairColor(Color color)
    {
        crosshairColor = color;
        if (crosshairImage != null)
        {
            crosshairImage.color = color;
        }
    }
    
    public void SetCrosshairSize(float size)
    {
        crosshairSize = size;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * size;
        }
    }
}

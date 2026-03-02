using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseController playerController;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private RectTransform ammoContainer;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private MinimapRadarOverlay minimapRadarOverlay;
    [SerializeField] private bool use2DMinimap = true;
    [SerializeField] private Color minimap2DBackgroundColor = new Color(0.18f, 0.28f, 0.2f, 1f);

    private bool minimapMarkerCleaned;
    private MinimapPlayerMarker minimapPlayerMarker;

    [Header("Layout")]
    [SerializeField] private bool bulletCountOnRight = true;
    [SerializeField] private Vector2 ammoScreenMargin = new Vector2(32f, 28f);

    [Header("Minimap Follow")]
    [SerializeField] private Vector3 minimapFollowOffset = new Vector3(0f, 35f, 0f);
    [SerializeField] private bool minimapRotateWithPlayer = true;
    [SerializeField] private Vector2 minimapSize = new Vector2(500f, 500f);

    [Header("Performance")]
    [SerializeField, Range(0.03f, 0.5f)] private float uiRefreshInterval = 0.1f;
    [SerializeField, Range(0.1f, 1f)] private float targetResolveInterval = 0.25f;

    private float nextUiRefreshTime;
    private float nextTargetResolveTime;
    private Transform cachedMinimapTarget;

    void Awake()
    {
        ResolveUIReferences();
        TryResolvePlayerController();

        ApplyAmmoAnchor();
        EnsureMinimapRadarOverlay();
        RefreshUI();

        nextUiRefreshTime = Time.unscaledTime + uiRefreshInterval;
    }

    void Update()
    {
        if (Time.unscaledTime < nextUiRefreshTime)
            return;

        RefreshUI();
        nextUiRefreshTime = Time.unscaledTime + uiRefreshInterval;
    }

    void LateUpdate()
    {
        ForceMinimapCameraPose();
    }

    public void RefreshUI()
    {
        ResolveUIReferences();

        if (playerController == null)
            TryResolvePlayerController();

        if (playerController != null)
            UpdateHealthAndAmmo(playerController);
        else
            SetMissingPlayerState();

        UpdateMinimapState();
    }

    public void SetBulletCountSide(bool showOnRight)
    {
        bulletCountOnRight = showOnRight;
        ApplyAmmoAnchor();
    }

    void UpdateHealthAndAmmo(BaseController controller)
    {
        float maxHealth = Mathf.Max(1f, controller.MaxHealth);
        float currentHealth = Mathf.Clamp(controller.CurrentHealth, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"HP {Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";

        if (ammoText != null)
            ammoText.text = $"Ammo {controller.CurrentAmmo}/{controller.MaxAmmo}";
    }

    void SetMissingPlayerState()
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.value = 0f;
        }

        if (healthText != null)
            healthText.text = "HP --/--";

        if (ammoText != null)
            ammoText.text = "Ammo --/--";
    }

    void UpdateMinimapState()
    {
        bool minimapReady = use2DMinimap ? minimapImage != null : minimapCamera != null;

        if (minimapImage != null)
        {
            minimapImage.enabled = minimapReady;

            if (use2DMinimap)
            {
                minimapImage.color = minimapImage.texture == null ? minimap2DBackgroundColor : Color.white;

                if (minimapCamera != null && minimapCamera.enabled)
                    minimapCamera.enabled = false;
            }
            else
            {
                minimapImage.color = Color.white;

                if (minimapCamera != null && !minimapCamera.enabled)
                    minimapCamera.enabled = true;
            }
        }

        if (minimapReady)
        {
            if (!use2DMinimap)
                EnsureMinimapCameraBehavior();

            EnsureMinimapRadarOverlay();

            if (!minimapMarkerCleaned)
            {
                CleanupDuplicateMinimapMarkers();
                minimapMarkerCleaned = true;
            }

            EnsureMinimapPlayerMarkerTracking();
        }
    }

    void EnsureMinimapRadarOverlay()
    {
        if (minimapImage == null)
            return;

        RectTransform minimapRoot = minimapImage.transform.parent as RectTransform;
        if (minimapRoot == null)
            return;

        if (minimapSize.x <= 0f || minimapSize.y <= 0f)
            minimapSize = new Vector2(500f, 500f);

        minimapRoot.sizeDelta = minimapSize;

        if (minimapRadarOverlay == null)
            minimapRadarOverlay = minimapRoot.GetComponent<MinimapRadarOverlay>();

        if (minimapRadarOverlay == null)
            minimapRadarOverlay = minimapRoot.gameObject.AddComponent<MinimapRadarOverlay>();

        minimapRadarOverlay.SetMinimapView(minimapImage);

        minimapRadarOverlay.SetRotateCardinalsWithPlayer(true);

        Transform minimapTarget = ResolveMinimapTargetTransform(false);
        if (minimapTarget != null)
            minimapRadarOverlay.SetTrackingTarget(minimapTarget);
    }

    void EnsureMinimapCameraBehavior()
    {
        if (minimapCamera == null)
            return;

        MinimapCameraFollow minimapFollow = minimapCamera.GetComponent<MinimapCameraFollow>();
        if (minimapFollow == null)
            minimapFollow = minimapCamera.gameObject.AddComponent<MinimapCameraFollow>();

        minimapFollow.SetRotateWithTarget(true);

        Transform minimapTarget = ResolveMinimapTargetTransform(false);
        if (minimapTarget != null)
            minimapFollow.SetTarget(minimapTarget);
    }

    void ForceMinimapCameraPose()
    {
        if (use2DMinimap)
            return;

        if (minimapCamera == null)
            return;

        Transform followTarget = ResolveMinimapTargetTransform(false);
        if (followTarget == null)
            return;

        minimapCamera.transform.position = followTarget.position + minimapFollowOffset;

        if (minimapRotateWithPlayer)
            minimapCamera.transform.rotation = Quaternion.Euler(90f, followTarget.eulerAngles.y, 0f);
        else
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    Transform ResolveMinimapTargetTransform(bool forceRefresh)
    {
        if (!forceRefresh)
        {
            if (cachedMinimapTarget != null && cachedMinimapTarget.gameObject.activeInHierarchy)
                return cachedMinimapTarget;

            if (Time.unscaledTime < nextTargetResolveTime)
                return null;
        }

        nextTargetResolveTime = Time.unscaledTime + targetResolveInterval;

        GameObject taggedPlayer = TryFindTaggedPlayer();
        if (taggedPlayer != null)
        {
            cachedMinimapTarget = taggedPlayer.transform;
            return cachedMinimapTarget;
        }

        TankController tankController = FindFirstObjectByType<TankController>();
        if (tankController != null)
        {
            cachedMinimapTarget = tankController.transform;
            return cachedMinimapTarget;
        }

        if (playerController != null)
        {
            TankController nestedTankController = playerController.GetComponent<TankController>();
            if (nestedTankController == null)
                nestedTankController = playerController.GetComponentInChildren<TankController>();

            if (nestedTankController != null)
            {
                cachedMinimapTarget = nestedTankController.transform;
                return cachedMinimapTarget;
            }

            cachedMinimapTarget = playerController.transform;
            return cachedMinimapTarget;
        }

        cachedMinimapTarget = null;
        return null;
    }

    void CleanupDuplicateMinimapMarkers()
    {
        if (minimapImage == null)
            return;

        RectTransform minimapRoot = minimapImage.transform.parent as RectTransform;
        if (minimapRoot == null)
            return;

        TMP_Text[] markerTexts = minimapRoot.GetComponentsInChildren<TMP_Text>(true);
        MinimapPlayerMarker keptMarker = null;

        for (int i = 0; i < markerTexts.Length; i++)
        {
            TMP_Text markerText = markerTexts[i];
            if (markerText == null)
                continue;

            bool isMarkerName = markerText.name.ToLowerInvariant().Contains("playermarker");
            bool isMarkerGlyph = markerText.text != null && markerText.text.Contains("▲");
            if (!isMarkerName && !isMarkerGlyph)
                continue;

            MinimapPlayerMarker marker = markerText.GetComponent<MinimapPlayerMarker>();

            if (keptMarker == null)
            {
                if (marker == null)
                    marker = markerText.gameObject.AddComponent<MinimapPlayerMarker>();

                keptMarker = marker;
                continue;
            }

            Destroy(markerText.gameObject);
        }

        if (keptMarker == null)
            return;

        Transform markerTarget = null;
        if (playerController != null)
            markerTarget = playerController.transform;
        else
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
                markerTarget = taggedPlayer.transform;
        }

        if (markerTarget != null)
            keptMarker.SetTarget(markerTarget);

        minimapPlayerMarker = keptMarker;
        ApplyPlayerMarkerStyle(minimapPlayerMarker);
        keptMarker.transform.SetAsLastSibling();
    }

    void EnsureMinimapPlayerMarkerTracking()
    {
        if (minimapImage == null)
            return;

        RectTransform minimapRoot = minimapImage.transform.parent as RectTransform;
        if (minimapRoot == null)
            return;

        if (minimapPlayerMarker == null)
        {
            MinimapPlayerMarker[] markers = minimapRoot.GetComponentsInChildren<MinimapPlayerMarker>(true);
            if (markers.Length > 0)
                minimapPlayerMarker = markers[0];
        }

        if (minimapPlayerMarker == null)
            return;

        Transform minimapTarget = ResolveMinimapTargetTransform(false);
        if (minimapTarget != null)
            minimapPlayerMarker.SetTarget(minimapTarget);

        ApplyPlayerMarkerStyle(minimapPlayerMarker);

        minimapPlayerMarker.transform.SetAsLastSibling();
    }

    void ApplyPlayerMarkerStyle(MinimapPlayerMarker marker)
    {
        if (marker == null)
            return;

        TMP_Text markerText = marker.GetComponent<TMP_Text>();
        if (markerText == null)
            return;

        markerText.text = "▲";
        markerText.fontSize = 44;
        markerText.color = new Color(0.22f, 1f, 0.3f, 1f);

        Outline outline = markerText.GetComponent<Outline>();
        if (outline == null)
            outline = markerText.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);
        outline.useGraphicAlpha = true;
    }

    void ApplyAmmoAnchor()
    {
        if (ammoContainer == null)
            return;

        if (bulletCountOnRight)
        {
            ammoContainer.anchorMin = new Vector2(1f, 0f);
            ammoContainer.anchorMax = new Vector2(1f, 0f);
            ammoContainer.pivot = new Vector2(1f, 0f);
            ammoContainer.anchoredPosition = new Vector2(-ammoScreenMargin.x, ammoScreenMargin.y);
        }
        else
        {
            ammoContainer.anchorMin = new Vector2(0f, 0f);
            ammoContainer.anchorMax = new Vector2(0f, 0f);
            ammoContainer.pivot = new Vector2(0f, 0f);
            ammoContainer.anchoredPosition = ammoScreenMargin;
        }
    }

    void ResolveUIReferences()
    {
        if (healthBar == null)
        {
            Transform healthRoot = transform.Find("HealthTopRight");
            if (healthRoot != null)
                healthBar = healthRoot.GetComponentInChildren<Slider>(true);
        }

        if (healthText == null)
            healthText = FindTextByName("HealthText");

        if (ammoText == null)
            ammoText = FindTextByName("AmmoText");

        if (ammoContainer == null)
        {
            Transform ammoRoot = transform.Find("AmmoBottom");
            if (ammoRoot != null)
                ammoContainer = ammoRoot as RectTransform;
        }

        if (minimapImage == null)
        {
            Transform minimapRoot = transform.Find("MiniMapTopLeft");
            if (minimapRoot != null)
                minimapImage = minimapRoot.GetComponentInChildren<RawImage>(true);
        }

        if (minimapCamera == null)
        {
            GameObject minimapCameraObject = GameObject.Find("MinimapCamera");
            if (minimapCameraObject != null)
                minimapCamera = minimapCameraObject.GetComponent<Camera>();
        }
    }

    TMP_Text FindTextByName(string objectName)
    {
        Transform target = transform.Find(objectName);
        if (target != null)
            return target.GetComponent<TMP_Text>();

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text != null && text.name == objectName)
                return text;
        }

        return null;
    }

    void TryResolvePlayerController()
    {
        GameObject taggedPlayer = TryFindTaggedPlayer();
        if (taggedPlayer != null)
            playerController = taggedPlayer.GetComponent<BaseController>();

        if (playerController == null)
            playerController = FindFirstObjectByType<BaseController>();

        cachedMinimapTarget = ResolveMinimapTargetTransform(true);
    }

    static GameObject TryFindTaggedPlayer()
    {
        GameObject taggedPlayer = null;

        try
        {
            taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        }
        catch (UnityException)
        {
        }

        if (taggedPlayer == null)
        {
            try
            {
                taggedPlayer = GameObject.FindGameObjectWithTag("player");
            }
            catch (UnityException)
            {
            }
        }

        return taggedPlayer;
    }
}

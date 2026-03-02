using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class BattleUISceneSetup
{
    [MenuItem("Tools/Battle UI/Setup In Active Scene")]
    public static void SetupInActiveScene()
    {
        Canvas canvas = GetOrCreateCanvas();
        EnsureEventSystem();

        GameObject battleUiRoot = GetOrCreateChild(canvas.gameObject, "BattleUI");
        RectTransform battleUiRect = battleUiRoot.GetComponent<RectTransform>();
        if (battleUiRect == null)
            battleUiRect = battleUiRoot.AddComponent<RectTransform>();
        StretchToParent(battleUiRect);

        HUDController hudController = battleUiRoot.GetComponent<HUDController>();
        if (hudController == null)
            hudController = battleUiRoot.AddComponent<HUDController>();

        GameObject healthRoot = GetOrCreateChild(battleUiRoot, "HealthTopRight");
        RectTransform healthRootRect = EnsureRectTransform(healthRoot);
        healthRootRect.anchorMin = new Vector2(1f, 1f);
        healthRootRect.anchorMax = new Vector2(1f, 1f);
        healthRootRect.pivot = new Vector2(1f, 1f);
        healthRootRect.sizeDelta = new Vector2(280f, 70f);
        healthRootRect.anchoredPosition = new Vector2(-24f, -24f);

        Slider healthSlider = GetOrCreateSlider(healthRoot.transform, "HealthBar");
        RectTransform healthSliderRect = healthSlider.GetComponent<RectTransform>();
        healthSliderRect.anchorMin = new Vector2(0f, 0f);
        healthSliderRect.anchorMax = new Vector2(1f, 0f);
        healthSliderRect.pivot = new Vector2(0.5f, 0f);
        healthSliderRect.sizeDelta = new Vector2(0f, 22f);
        healthSliderRect.anchoredPosition = new Vector2(0f, 8f);

        TextMeshProUGUI healthText = GetOrCreateTMPText(healthRoot.transform, "HealthText", "HP 100/100", 24, TextAlignmentOptions.TopRight);
        RectTransform healthTextRect = healthText.GetComponent<RectTransform>();
        healthTextRect.anchorMin = new Vector2(0f, 1f);
        healthTextRect.anchorMax = new Vector2(1f, 1f);
        healthTextRect.pivot = new Vector2(1f, 1f);
        healthTextRect.sizeDelta = new Vector2(0f, 30f);
        healthTextRect.anchoredPosition = new Vector2(0f, 0f);

        GameObject minimapRoot = GetOrCreateChild(battleUiRoot, "MiniMapTopLeft");
        RectTransform minimapRect = EnsureRectTransform(minimapRoot);
        minimapRect.anchorMin = new Vector2(0f, 1f);
        minimapRect.anchorMax = new Vector2(0f, 1f);
        minimapRect.pivot = new Vector2(0f, 1f);
        minimapRect.sizeDelta = new Vector2(500f, 500f);
        minimapRect.anchoredPosition = new Vector2(24f, -24f);

        RemoveRadarVisuals(minimapRoot.transform);

        RawImage minimapImage = EnsureCircularMinimapRootAndGetView(minimapRoot);

        MinimapRadarOverlay radarOverlay = minimapRoot.GetComponent<MinimapRadarOverlay>();
        if (radarOverlay == null)
            radarOverlay = minimapRoot.AddComponent<MinimapRadarOverlay>();
        radarOverlay.SetMinimapView(minimapImage);

        Image minimapMaskImage = minimapRoot.GetComponent<Image>();
        if (minimapMaskImage == null)
            minimapMaskImage = minimapRoot.AddComponent<Image>();

        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (circleSprite != null)
            minimapMaskImage.sprite = circleSprite;

        minimapMaskImage.type = Image.Type.Simple;
        minimapMaskImage.preserveAspect = true;
        minimapMaskImage.color = Color.white;

        Mask minimapMask = minimapRoot.GetComponent<Mask>();
        if (minimapMask == null)
            minimapMask = minimapRoot.AddComponent<Mask>();
        minimapMask.showMaskGraphic = false;

        RectTransform minimapImageRect = minimapImage.GetComponent<RectTransform>();
        StretchToParent(minimapImageRect);

        TextMeshProUGUI playerMarker = GetOrCreateTMPText(minimapRoot.transform, "PlayerMarker", "▲", 34, TextAlignmentOptions.Center);
        RemoveDuplicatePlayerMarkers(minimapRoot.transform, playerMarker.transform);
        RectTransform playerMarkerRect = playerMarker.GetComponent<RectTransform>();
        playerMarkerRect.anchorMin = new Vector2(0.5f, 0.5f);
        playerMarkerRect.anchorMax = new Vector2(0.5f, 0.5f);
        playerMarkerRect.pivot = new Vector2(0.5f, 0.5f);
        playerMarkerRect.sizeDelta = new Vector2(36f, 36f);
        playerMarkerRect.anchoredPosition = Vector2.zero;
        playerMarker.color = Color.green;
        playerMarker.raycastTarget = false;
        playerMarker.transform.SetAsLastSibling();

        GameObject ammoRoot = GetOrCreateChild(battleUiRoot, "AmmoBottom");
        RectTransform ammoRootRect = EnsureRectTransform(ammoRoot);
        ammoRootRect.anchorMin = new Vector2(1f, 0f);
        ammoRootRect.anchorMax = new Vector2(1f, 0f);
        ammoRootRect.pivot = new Vector2(1f, 0f);
        ammoRootRect.sizeDelta = new Vector2(280f, 44f);
        ammoRootRect.anchoredPosition = new Vector2(-24f, 24f);

        TextMeshProUGUI ammoText = GetOrCreateTMPText(ammoRoot.transform, "AmmoText", "Ammo 30/30", 28, TextAlignmentOptions.BottomRight);
        RectTransform ammoTextRect = ammoText.GetComponent<RectTransform>();
        ammoTextRect.anchorMin = new Vector2(0f, 0f);
        ammoTextRect.anchorMax = new Vector2(1f, 1f);
        ammoTextRect.pivot = new Vector2(1f, 0f);
        ammoTextRect.sizeDelta = Vector2.zero;
        ammoTextRect.anchoredPosition = Vector2.zero;

        Camera minimapCamera = GetOrCreateMinimapCamera();
        Transform playerTransform = FindPlayerTransform();
        MinimapCameraFollow minimapFollow = minimapCamera.GetComponent<MinimapCameraFollow>();
        if (minimapFollow == null)
            minimapFollow = minimapCamera.gameObject.AddComponent<MinimapCameraFollow>();
        minimapFollow.SetRotateWithTarget(true);
        if (playerTransform != null)
            minimapFollow.SetTarget(playerTransform);

        MinimapPlayerMarker minimapPlayerMarker = playerMarker.GetComponent<MinimapPlayerMarker>();
        if (minimapPlayerMarker == null)
            minimapPlayerMarker = playerMarker.gameObject.AddComponent<MinimapPlayerMarker>();
        if (playerTransform != null)
            minimapPlayerMarker.SetTarget(playerTransform);

        RenderTexture minimapTexture = GetOrCreateMinimapRenderTexture();
        minimapCamera.targetTexture = minimapTexture;
        minimapImage.texture = minimapTexture;

        BaseController playerController = null;
        if (playerTransform != null)
            playerController = playerTransform.GetComponent<BaseController>();

        SerializedObject serializedHud = new SerializedObject(hudController);
        serializedHud.FindProperty("playerController").objectReferenceValue = playerController;
        serializedHud.FindProperty("healthBar").objectReferenceValue = healthSlider;
        serializedHud.FindProperty("healthText").objectReferenceValue = healthText;
        serializedHud.FindProperty("ammoText").objectReferenceValue = ammoText;
        serializedHud.FindProperty("ammoContainer").objectReferenceValue = ammoRootRect;
        serializedHud.FindProperty("minimapCamera").objectReferenceValue = minimapCamera;
        serializedHud.FindProperty("minimapImage").objectReferenceValue = minimapImage;
        serializedHud.FindProperty("use2DMinimap").boolValue = true;
        serializedHud.FindProperty("bulletCountOnRight").boolValue = true;
        serializedHud.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(hudController);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = battleUiRoot;

        Debug.Log("Battle UI setup complete in active scene.");
    }

    static Canvas GetOrCreateCanvas()
    {
        Canvas existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null)
            return existing;

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    static void EnsureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static GameObject GetOrCreateChild(GameObject parent, string name)
    {
        Transform child = parent.transform.Find(name);
        if (child != null)
            return child.gameObject;

        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent.transform, false);
        return gameObject;
    }

    static RectTransform EnsureRectTransform(GameObject gameObject)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();
        return rectTransform;
    }

    static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    static Slider GetOrCreateSlider(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null && existing.TryGetComponent(out Slider existingSlider))
            return existingSlider;

        DefaultControls.Resources resources = new DefaultControls.Resources();
        GameObject sliderObject = DefaultControls.CreateSlider(resources);
        sliderObject.name = name;
        sliderObject.transform.SetParent(parent, false);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        slider.wholeNumbers = false;
        return slider;
    }

    static TextMeshProUGUI GetOrCreateTMPText(Transform parent, string name, string initialText, int fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        TextMeshProUGUI text = null;
        if (existing != null)
            text = existing.GetComponent<TextMeshProUGUI>();

        if (text == null)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            text = textObject.GetComponent<TextMeshProUGUI>();
        }

        text.text = initialText;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        return text;
    }

    static RawImage GetOrCreateRawImage(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        RawImage rawImage = null;
        if (existing != null)
            rawImage = existing.GetComponent<RawImage>();

        if (rawImage == null)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            rawImage = imageObject.GetComponent<RawImage>();
            rawImage.color = Color.white;
        }

        return rawImage;
    }

    static RawImage EnsureCircularMinimapRootAndGetView(GameObject minimapRoot)
    {
        RawImage legacyRawImage = minimapRoot.GetComponent<RawImage>();
        Texture legacyTexture = null;
        Color legacyColor = Color.white;

        if (legacyRawImage != null)
        {
            legacyTexture = legacyRawImage.texture;
            legacyColor = legacyRawImage.color;
            Object.DestroyImmediate(legacyRawImage);
        }

        Graphic rootGraphic = minimapRoot.GetComponent<Graphic>();
        if (rootGraphic != null && rootGraphic is not Image)
            Object.DestroyImmediate(rootGraphic);

        RawImage minimapView = GetOrCreateRawImage(minimapRoot.transform, "MinimapView");
        if (legacyTexture != null)
            minimapView.texture = legacyTexture;
        minimapView.color = legacyColor;

        return minimapView;
    }

    static Camera GetOrCreateMinimapCamera()
    {
        GameObject existing = GameObject.Find("MinimapCamera");
        if (existing == null)
            existing = new GameObject("MinimapCamera", typeof(Camera));

        Camera cameraComponent = existing.GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 24f;
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.04f, 0.08f, 0.12f, 1f);
        cameraComponent.cullingMask = ~0;
        cameraComponent.nearClipPlane = 0.1f;
        cameraComponent.farClipPlane = 400f;
        cameraComponent.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        return cameraComponent;
    }

    static RenderTexture GetOrCreateMinimapRenderTexture()
    {
        const string assetPath = "Assets/Settings/MinimapRT.renderTexture";
        RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(assetPath);

        if (renderTexture != null)
            return renderTexture;

        renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32)
        {
            name = "MinimapRT"
        };

        AssetDatabase.CreateAsset(renderTexture, assetPath);
        AssetDatabase.SaveAssets();
        return renderTexture;
    }

    static Transform FindPlayerTransform()
    {
        GameObject namedTank = GameObject.Find("minitank-processed-v3 (1)");
        if (namedTank != null)
            return namedTank.transform;

        GameObject namedVariantTank = GameObject.Find("minitank-processed-v3 (1) Variant Variant 1");
        if (namedVariantTank != null)
            return namedVariantTank.transform;

        TankController[] tanks = Object.FindObjectsByType<TankController>(FindObjectsSortMode.None);
        for (int i = 0; i < tanks.Length; i++)
        {
            if (tanks[i] == null)
                continue;

            string tankName = tanks[i].gameObject.name;
            if (!string.IsNullOrEmpty(tankName) && tankName.ToLowerInvariant().Contains("minitank-processed-v3"))
                return tanks[i].transform;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
            return taggedPlayer.transform;

        BaseController playerController = Object.FindFirstObjectByType<BaseController>();
        if (playerController != null)
            return playerController.transform;

        return null;
    }

    static void RemoveRadarVisuals(Transform minimapRoot)
    {
        if (minimapRoot == null)
            return;

        for (int i = minimapRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = minimapRoot.GetChild(i);
            string childName = child.name.ToLowerInvariant();

            bool isRadarVisual = childName.Contains("radar") || childName.Contains("sweep") || childName.Contains("ring");
            if (!isRadarVisual)
                continue;

            if (child.name == "PlayerMarker" || child.name == "MinimapView")
                continue;

            Object.DestroyImmediate(child.gameObject);
        }
    }

    static void RemoveDuplicatePlayerMarkers(Transform minimapRoot, Transform keepMarker)
    {
        if (minimapRoot == null)
            return;

        TextMeshProUGUI[] texts = minimapRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
                continue;

            if (text.transform == keepMarker)
                continue;

            bool isMarkerName = text.name.ToLowerInvariant().Contains("playermarker");
            bool isMarkerGlyph = !string.IsNullOrEmpty(text.text) && text.text.Contains("▲");
            if (!isMarkerName && !isMarkerGlyph)
                continue;

            Object.DestroyImmediate(text.gameObject);
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MinimapTextureGenerator
{
    const int DefaultResolution = 1024;
    const float HeightShadeMin = 0.9f;
    const float HeightShadeMax = 1.22f;
    const float SlopeTintStrength = 0.2f;
    const float PostContrast = 1.16f;
    const float PostBrightness = 0.07f;
    const float PostSaturation = 1.08f;
    static readonly Color SlopeTint = new Color(0.42f, 0.36f, 0.29f, 1f);

    [MenuItem("Tools/Battle UI/Generate Top-Down Minimap Texture")]
    public static void GenerateTopDownMinimapTexture()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No active Terrain found. Please open a scene with an active Terrain before generating minimap texture.");
            return;
        }

        string targetPath = EditorUtility.SaveFilePanelInProject(
            "Save Minimap Texture",
            "TopDownMinimap",
            "png",
            "Choose where to save the generated top-down minimap texture.",
            "Assets"
        );

        if (string.IsNullOrEmpty(targetPath))
            return;

        Texture2D bakedTexture = BakeTerrainTexture(terrain, DefaultResolution);
        if (bakedTexture == null)
        {
            Debug.LogError("Failed to bake minimap texture.");
            return;
        }

        byte[] pngBytes = bakedTexture.EncodeToPNG();
        File.WriteAllBytes(targetPath, pngBytes);
        Object.DestroyImmediate(bakedTexture);

        AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceSynchronousImport);
        ConfigureImportedTexture(targetPath);

        Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
        if (importedTexture == null)
        {
            Debug.LogError("Generated texture could not be loaded after import.");
            return;
        }

        ApplyToSceneMinimaps(importedTexture);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"Generated and applied minimap texture: {targetPath}");
    }

    static Texture2D BakeTerrainTexture(Terrain terrain, int resolution)
    {
        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null)
            return null;

        int width = Mathf.Max(64, resolution);
        int height = Mathf.Max(64, resolution);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        TerrainLayer[] terrainLayers = terrainData.terrainLayers;
        int layerCount = terrainLayers != null ? terrainLayers.Length : 0;
        float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        Color[] layerColors = new Color[layerCount];
        for (int i = 0; i < layerCount; i++)
            layerColors[i] = ResolveLayerColor(terrainLayers[i]);

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float normalizedY = y / (float)(height - 1);
            int alphaY = Mathf.Clamp(Mathf.RoundToInt(normalizedY * (terrainData.alphamapHeight - 1)), 0, terrainData.alphamapHeight - 1);

            for (int x = 0; x < width; x++)
            {
                float normalizedX = x / (float)(width - 1);
                int alphaX = Mathf.Clamp(Mathf.RoundToInt(normalizedX * (terrainData.alphamapWidth - 1)), 0, terrainData.alphamapWidth - 1);

                Color mixedColor = Color.black;

                if (layerCount > 0)
                {
                    for (int layerIndex = 0; layerIndex < layerCount; layerIndex++)
                    {
                        float weight = alphaMaps[alphaY, alphaX, layerIndex];
                        if (weight <= 0.0001f)
                            continue;

                        mixedColor += layerColors[layerIndex] * weight;
                    }
                }
                else
                {
                    float height01 = terrainData.GetInterpolatedHeight(normalizedX, normalizedY) / Mathf.Max(1f, terrainData.size.y);
                    mixedColor = Color.Lerp(new Color(0.15f, 0.35f, 0.2f), new Color(0.7f, 0.7f, 0.6f), height01);
                }

                float sampledHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedY);
                float normalizedHeight = Mathf.Clamp01(sampledHeight / Mathf.Max(1f, terrainData.size.y));
                float heightShade = Mathf.Lerp(HeightShadeMin, HeightShadeMax, normalizedHeight);
                mixedColor *= heightShade;

                Vector3 normal = terrainData.GetInterpolatedNormal(normalizedX, normalizedY);
                float slope01 = Mathf.Clamp01(1f - normal.y);
                float slopeInfluence = Mathf.Pow(slope01, 0.85f) * SlopeTintStrength;
                mixedColor = Color.Lerp(mixedColor, SlopeTint, slopeInfluence);

                mixedColor = ApplyColorGrading(mixedColor);

                mixedColor.r = Mathf.Clamp01(mixedColor.r);
                mixedColor.g = Mathf.Clamp01(mixedColor.g);
                mixedColor.b = Mathf.Clamp01(mixedColor.b);

                mixedColor.a = 1f;
                pixels[y * width + x] = mixedColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        return texture;
    }

    static Color ApplyColorGrading(Color color)
    {
        float luminance = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
        Color saturated = Color.Lerp(new Color(luminance, luminance, luminance, color.a), color, PostSaturation);

        saturated.r = ((saturated.r - 0.5f) * PostContrast) + 0.5f + PostBrightness;
        saturated.g = ((saturated.g - 0.5f) * PostContrast) + 0.5f + PostBrightness;
        saturated.b = ((saturated.b - 0.5f) * PostContrast) + 0.5f + PostBrightness;

        return saturated;
    }

    static Color ResolveLayerColor(TerrainLayer layer)
    {
        if (layer == null)
            return new Color(0.3f, 0.45f, 0.3f, 1f);

        if (layer.diffuseTexture == null)
            return layer.diffuseRemapMax;

        return AverageTextureColor(layer.diffuseTexture);
    }

    static Color AverageTextureColor(Texture2D texture)
    {
        if (texture == null)
            return Color.gray;

        try
        {
            Color32[] colors = texture.GetPixels32();
            if (colors == null || colors.Length == 0)
                return Color.gray;

            long r = 0;
            long g = 0;
            long b = 0;

            for (int i = 0; i < colors.Length; i++)
            {
                r += colors[i].r;
                g += colors[i].g;
                b += colors[i].b;
            }

            float inv = 1f / (255f * colors.Length);
            return new Color(r * inv, g * inv, b * inv, 1f);
        }
        catch
        {
            return Color.gray;
        }
    }

    static void ConfigureImportedTexture(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Default;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    static void ApplyToSceneMinimaps(Texture2D mapTexture)
    {
        MinimapRadarOverlay[] overlays = Object.FindObjectsByType<MinimapRadarOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < overlays.Length; i++)
        {
            SerializedObject overlaySO = new SerializedObject(overlays[i]);
            overlaySO.FindProperty("topDownMapTexture").objectReferenceValue = mapTexture;
            overlaySO.FindProperty("useTopDownTextureMap").boolValue = true;
            overlaySO.FindProperty("autoWorldBoundsFromActiveTerrain").boolValue = true;
            overlaySO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(overlays[i]);
        }

        HUDController[] hudControllers = Object.FindObjectsByType<HUDController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < hudControllers.Length; i++)
        {
            SerializedObject hudSO = new SerializedObject(hudControllers[i]);
            SerializedProperty use2DProp = hudSO.FindProperty("use2DMinimap");
            if (use2DProp != null)
                use2DProp.boolValue = true;
            hudSO.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hudControllers[i]);
        }
    }
}

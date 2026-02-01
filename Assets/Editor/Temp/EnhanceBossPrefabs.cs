using UnityEngine;
using UnityEditor;
using Boss;

public class EnhanceBossPrefabs
{
    [MenuItem("Tools/Enhance Boss Prefabs")]
    public static void Execute()
    {
        // Update Wave Projectile - make it much more visible
        UpdateWaveProjectile();
        
        // Update Falling Projectile
        UpdateFallingProjectile();
        
        // Update Telegraph
        UpdateTelegraph();
        
        // Update Strike
        UpdateStrike();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[EnhanceBossPrefabs] All boss prefabs enhanced with larger, more visible effects!");
    }

    private static void UpdateWaveProjectile()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossWaveProjectile.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Use a larger built-in sprite
            sr.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            sr.color = new Color(0.8f, 0.2f, 1f, 0.9f); // Bright purple with slight transparency
            sr.sortingOrder = 100;
        }

        // Much larger scale for visibility
        prefabRoot.transform.localScale = new Vector3(3f, 10f, 1f);

        // Update collider
        BoxCollider2D boxCol = prefabRoot.GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            boxCol.size = new Vector2(0.8f, 0.8f);
        }

        // Configure visual effects
        BossVisualEffects effects = prefabRoot.GetComponent<BossVisualEffects>();
        if (effects != null)
        {
            SerializedObject so = new SerializedObject(effects);
            so.FindProperty("_effectType").enumValueIndex = 0; // Wave
            so.FindProperty("_enablePulse").boolValue = true;
            so.FindProperty("_pulseSpeed").floatValue = 6f;
            so.FindProperty("_pulseMinScale").floatValue = 0.95f;
            so.FindProperty("_pulseMaxScale").floatValue = 1.05f;
            so.FindProperty("_enableGlow").boolValue = true;
            so.FindProperty("_glowSpeed").floatValue = 4f;
            so.FindProperty("_glowMinAlpha").floatValue = 0.7f;
            so.FindProperty("_glowMaxAlpha").floatValue = 1f;
            so.FindProperty("_glowColor").colorValue = new Color(0.8f, 0.2f, 1f, 1f);
            so.FindProperty("_createTrail").boolValue = true;
            so.FindProperty("_trailTime").floatValue = 0.5f;
            so.FindProperty("_trailStartWidth").floatValue = 3f;
            so.FindProperty("_createParticles").boolValue = true;
            so.FindProperty("_particleCount").intValue = 30;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[EnhanceBossPrefabs] BossWaveProjectile - Purple 3x10 wave with trail and particles");
    }

    private static void UpdateFallingProjectile()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossFallingProjectile.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(1f, 0.2f, 0.2f, 1f); // Bright red
            sr.sortingOrder = 100;
        }

        // Larger scale
        prefabRoot.transform.localScale = new Vector3(2f, 2f, 1f);

        // Update collider
        CircleCollider2D circleCol = prefabRoot.GetComponent<CircleCollider2D>();
        if (circleCol != null)
        {
            circleCol.radius = 0.5f;
        }

        // Configure visual effects
        BossVisualEffects effects = prefabRoot.GetComponent<BossVisualEffects>();
        if (effects != null)
        {
            SerializedObject so = new SerializedObject(effects);
            so.FindProperty("_effectType").enumValueIndex = 1; // Falling
            so.FindProperty("_enablePulse").boolValue = true;
            so.FindProperty("_pulseSpeed").floatValue = 8f;
            so.FindProperty("_pulseMinScale").floatValue = 0.85f;
            so.FindProperty("_pulseMaxScale").floatValue = 1.15f;
            so.FindProperty("_enableGlow").boolValue = true;
            so.FindProperty("_glowSpeed").floatValue = 5f;
            so.FindProperty("_glowMinAlpha").floatValue = 0.8f;
            so.FindProperty("_glowMaxAlpha").floatValue = 1f;
            so.FindProperty("_glowColor").colorValue = new Color(1f, 0.3f, 0.3f, 1f);
            so.FindProperty("_enableRotation").boolValue = true;
            so.FindProperty("_rotationSpeed").floatValue = 360f;
            so.FindProperty("_createTrail").boolValue = true;
            so.FindProperty("_trailTime").floatValue = 0.6f;
            so.FindProperty("_trailStartWidth").floatValue = 2f;
            so.FindProperty("_createParticles").boolValue = true;
            so.FindProperty("_particleCount").intValue = 25;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[EnhanceBossPrefabs] BossFallingProjectile - Red 2x2 orb with rotation and trail");
    }

    private static void UpdateTelegraph()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossTelegraph.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(1f, 0f, 0f, 0.7f); // Semi-transparent red
            sr.sortingOrder = 99;
        }

        // Larger base scale
        prefabRoot.transform.localScale = new Vector3(6f, 6f, 1f);

        // Configure telegraph effect
        TelegraphEffect telegraph = prefabRoot.GetComponent<TelegraphEffect>();
        if (telegraph != null)
        {
            SerializedObject so = new SerializedObject(telegraph);
            so.FindProperty("_warningColor").colorValue = new Color(1f, 0f, 0f, 0.7f);
            so.FindProperty("_dangerColor").colorValue = new Color(1f, 0.5f, 0f, 0.9f);
            so.FindProperty("_pulseSpeed").floatValue = 5f;
            so.FindProperty("_rotationSpeed").floatValue = 120f;
            so.FindProperty("_expandDuration").floatValue = 0.2f;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[EnhanceBossPrefabs] BossTelegraph - Red 6x6 warning circle with pulsing rings");
    }

    private static void UpdateStrike()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossStrike.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = new Color(1f, 1f, 0f, 1f); // Bright yellow
            sr.sortingOrder = 101;
        }

        // Larger base scale
        prefabRoot.transform.localScale = new Vector3(6f, 6f, 1f);

        // Configure explosion effect
        StrikeExplosionEffect explosion = prefabRoot.GetComponent<StrikeExplosionEffect>();
        if (explosion != null)
        {
            SerializedObject so = new SerializedObject(explosion);
            so.FindProperty("_coreColor").colorValue = new Color(1f, 1f, 0.5f, 1f);
            so.FindProperty("_outerColor").colorValue = new Color(1f, 0.6f, 0f, 0.9f);
            so.FindProperty("_explosionDuration").floatValue = 0.6f;
            so.FindProperty("_maxScale").floatValue = 3f;
            so.FindProperty("_sparkCount").intValue = 40;
            so.FindProperty("_sparkSpeed").floatValue = 18f;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[EnhanceBossPrefabs] BossStrike - Yellow 6x6 explosion with sparks");
    }
}

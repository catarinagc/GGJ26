using UnityEngine;
using UnityEditor;
using Boss;

public class SetupBossPrefabs
{
    [MenuItem("Tools/Setup Boss Prefabs")]
    public static void Execute()
    {
        SetupWaveProjectile();
        SetupFallingProjectile();
        SetupTelegraph();
        SetupStrike();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[SetupBossPrefabs] All boss prefabs updated with enhanced visuals!");
    }

    private static void SetupWaveProjectile()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossWaveProjectile.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError("BossWaveProjectile prefab not found!");
            return;
        }

        // Open prefab for editing
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        // Get or add SpriteRenderer
        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = prefabRoot.AddComponent<SpriteRenderer>();
        }

        // Set to a larger, more visible sprite - use Knob which is circular/visible
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0.8f, 0.2f, 1f, 1f); // Bright purple
        sr.sortingOrder = 100;

        // Set larger scale for visibility (2x8 as requested)
        prefabRoot.transform.localScale = new Vector3(2f, 8f, 1f);

        // Update BoxCollider2D size
        BoxCollider2D boxCol = prefabRoot.GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            boxCol.size = new Vector2(1f, 1f);
        }

        // Add visual effects component
        BossVisualEffects effects = prefabRoot.GetComponent<BossVisualEffects>();
        if (effects == null)
        {
            effects = prefabRoot.AddComponent<BossVisualEffects>();
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[SetupBossPrefabs] BossWaveProjectile updated - Purple 2x8 wave with effects");
    }

    private static void SetupFallingProjectile()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossFallingProjectile.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError("BossFallingProjectile prefab not found!");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        // Get or add SpriteRenderer
        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = prefabRoot.AddComponent<SpriteRenderer>();
        }

        // Set to Knob sprite for circular appearance
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0.2f, 0.2f, 1f); // Bright red
        sr.sortingOrder = 100;

        // Set larger scale (1.5x1.5 as requested)
        prefabRoot.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        // Update CircleCollider2D
        CircleCollider2D circleCol = prefabRoot.GetComponent<CircleCollider2D>();
        if (circleCol != null)
        {
            circleCol.radius = 0.5f;
        }

        // Add visual effects component
        BossVisualEffects effects = prefabRoot.GetComponent<BossVisualEffects>();
        if (effects == null)
        {
            effects = prefabRoot.AddComponent<BossVisualEffects>();
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[SetupBossPrefabs] BossFallingProjectile updated - Red 1.5x1.5 orb with effects");
    }

    private static void SetupTelegraph()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossTelegraph.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError("BossTelegraph prefab not found!");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        // Get or add SpriteRenderer
        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = prefabRoot.AddComponent<SpriteRenderer>();
        }

        // Set to Knob sprite for circular appearance
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0f, 0f, 0.6f); // Semi-transparent red
        sr.sortingOrder = 99;

        // Set scale (will be multiplied by strikeRadius * 2 in code)
        prefabRoot.transform.localScale = new Vector3(4f, 4f, 1f);

        // Add telegraph effect component
        TelegraphEffect telegraph = prefabRoot.GetComponent<TelegraphEffect>();
        if (telegraph == null)
        {
            telegraph = prefabRoot.AddComponent<TelegraphEffect>();
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[SetupBossPrefabs] BossTelegraph updated - Red warning circle with pulsing effect");
    }

    private static void SetupStrike()
    {
        string prefabPath = "Assets/Prefabs/Boss/BossStrike.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError("BossStrike prefab not found!");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        // Get or add SpriteRenderer
        SpriteRenderer sr = prefabRoot.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = prefabRoot.AddComponent<SpriteRenderer>();
        }

        // Set to Knob sprite
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 1f, 0f, 1f); // Bright yellow
        sr.sortingOrder = 101;

        // Set scale
        prefabRoot.transform.localScale = new Vector3(4f, 4f, 1f);

        // Add explosion effect component
        StrikeExplosionEffect explosion = prefabRoot.GetComponent<StrikeExplosionEffect>();
        if (explosion == null)
        {
            explosion = prefabRoot.AddComponent<StrikeExplosionEffect>();
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("[SetupBossPrefabs] BossStrike updated - Yellow explosion with particles");
    }
}

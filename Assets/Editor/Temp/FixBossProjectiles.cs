using UnityEngine;
using UnityEditor;

public class FixBossProjectiles
{
    [MenuItem("Tools/Fix Boss Projectiles Visibility")]
    public static void Execute()
    {
        string prefabFolder = "Assets/Prefabs/Boss";
        
        // Fix Wave Projectile - make it larger and more visible
        FixWaveProjectile(prefabFolder);
        
        // Fix Falling Projectile - make it larger
        FixFallingProjectile(prefabFolder);
        
        // Fix Telegraph - make it more visible
        FixTelegraph(prefabFolder);
        
        // Fix Strike effect
        FixStrike(prefabFolder);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[FixBossProjectiles] All projectiles updated for better visibility!");
    }
    
    private static void FixWaveProjectile(string folder)
    {
        string path = $"{folder}/BossWaveProjectile.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;
        
        // Modify the prefab
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        // Make it larger
        instance.transform.localScale = new Vector3(2f, 8f, 1f);
        
        // Make sprite more visible
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.8f, 0.2f, 1f, 1f); // Bright purple
            sr.sortingOrder = 10;
        }
        
        // Make collider match
        BoxCollider2D col = instance.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(1f, 1f);
        }
        
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        
        Debug.Log("[FixBossProjectiles] Wave projectile fixed - Scale: 2x8, Bright purple");
    }
    
    private static void FixFallingProjectile(string folder)
    {
        string path = $"{folder}/BossFallingProjectile.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;
        
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        // Make it larger
        instance.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        
        // Make sprite more visible
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 0.2f, 0.2f, 1f); // Bright red
            sr.sortingOrder = 10;
        }
        
        // Make collider match
        CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.radius = 0.4f;
        }
        
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        
        Debug.Log("[FixBossProjectiles] Falling projectile fixed - Scale: 1.5x1.5, Bright red");
    }
    
    private static void FixTelegraph(string folder)
    {
        string path = $"{folder}/BossTelegraph.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;
        
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        // Telegraph will be scaled by the boss script, but set base scale
        instance.transform.localScale = new Vector3(1f, 1f, 1f);
        
        // Make sprite more visible - pulsing red circle
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 0f, 0f, 0.6f); // Semi-transparent red
            sr.sortingOrder = 8;
        }
        
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        
        Debug.Log("[FixBossProjectiles] Telegraph fixed - Red warning circle");
    }
    
    private static void FixStrike(string folder)
    {
        string path = $"{folder}/BossStrike.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;
        
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        // Strike will be scaled by boss script
        instance.transform.localScale = new Vector3(1f, 1f, 1f);
        
        // Make sprite bright yellow/white flash
        SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 1f, 0.5f, 1f); // Bright yellow
            sr.sortingOrder = 12;
        }
        
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        
        Debug.Log("[FixBossProjectiles] Strike effect fixed - Bright yellow flash");
    }
}

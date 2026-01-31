using UnityEngine;
using UnityEditor;
using Enemy;

public class SetupEnemyCombat
{
    [MenuItem("Tools/Setup Enemy Combat on BasicGuard")]
    public static void Execute()
    {
        // Load the BasicGuard prefab
        string prefabPath = "Assets/Prefabs/Enemies/BasicGuard.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError($"Could not find prefab at {prefabPath}");
            return;
        }

        // Open prefab for editing
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

        try
        {
            // Add EnemyCombat component if not present
            EnemyCombat enemyCombat = prefabRoot.GetComponent<EnemyCombat>();
            if (enemyCombat == null)
            {
                enemyCombat = prefabRoot.AddComponent<EnemyCombat>();
                Debug.Log("Added EnemyCombat component to BasicGuard");
            }

            // Create Enemy_Weapon_Rapier child object if not present
            Transform weaponTransform = prefabRoot.transform.Find("Enemy_Weapon_Rapier");
            if (weaponTransform == null)
            {
                GameObject weapon = new GameObject("Enemy_Weapon_Rapier");
                weapon.transform.SetParent(prefabRoot.transform);
                weapon.transform.localPosition = new Vector3(0.4f, 0f, 0f);
                weapon.transform.localRotation = Quaternion.Euler(0, 0, 45f);
                weapon.transform.localScale = new Vector3(0.15f, 0.8f, 1f);

                // Add SpriteRenderer for the sword visual
                SpriteRenderer sr = weapon.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Capsule.png");
                sr.color = new Color(0.8f, 0.15f, 0.15f, 1f); // Crimson red
                sr.sortingOrder = 1;

                weaponTransform = weapon.transform;
                Debug.Log("Created Enemy_Weapon_Rapier child object");
            }

            // Create ProjectileSpawnPoint child object if not present
            Transform spawnPoint = prefabRoot.transform.Find("ProjectileSpawnPoint");
            if (spawnPoint == null)
            {
                GameObject spawnPointObj = new GameObject("ProjectileSpawnPoint");
                spawnPointObj.transform.SetParent(prefabRoot.transform);
                spawnPointObj.transform.localPosition = new Vector3(0.6f, 0f, 0f);
                spawnPoint = spawnPointObj.transform;
                Debug.Log("Created ProjectileSpawnPoint child object");
            }

            // Configure EnemyCombat component via SerializedObject
            SerializedObject so = new SerializedObject(enemyCombat);

            // Set projectile prefab
            GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            if (projectilePrefab != null)
            {
                so.FindProperty("_projectilePrefab").objectReferenceValue = projectilePrefab;
            }

            // Set slash effect prefab
            GameObject slashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SlashEffect.prefab");
            if (slashPrefab != null)
            {
                so.FindProperty("_slashEffectPrefab").objectReferenceValue = slashPrefab;
            }

            // Set projectile spawn point
            so.FindProperty("_projectileSpawnPoint").objectReferenceValue = spawnPoint;

            // Set weapon visual
            so.FindProperty("_weaponVisual").objectReferenceValue = weaponTransform.gameObject;

            // Set player layer (layer 8 is Player)
            so.FindProperty("_playerLayer").intValue = 1 << 8;

            // Set crimson colors
            SerializedProperty slashColorProp = so.FindProperty("_slashColor");
            slashColorProp.colorValue = new Color(0.8f, 0.1f, 0.1f, 1f);

            SerializedProperty waveColorProp = so.FindProperty("_swordWaveColor");
            waveColorProp.colorValue = new Color(0.8f, 0.15f, 0.15f, 0.9f);

            so.ApplyModifiedProperties();

            // Also update EnemyAI to use Magic Sword combat
            EnemyAI enemyAI = prefabRoot.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                SerializedObject aiSo = new SerializedObject(enemyAI);
                
                // Enable Magic Sword combat
                SerializedProperty useMagicSword = aiSo.FindProperty("_useMagicSwordCombat");
                if (useMagicSword != null)
                {
                    useMagicSword.boolValue = true;
                }

                // Disable contact damage (enemies now use sword attacks)
                SerializedProperty dealContactDamage = aiSo.FindProperty("_dealContactDamage");
                if (dealContactDamage != null)
                {
                    dealContactDamage.boolValue = false;
                }

                aiSo.ApplyModifiedProperties();
                Debug.Log("Configured EnemyAI for Magic Sword combat");
            }

            // Save the prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            Debug.Log($"Successfully set up EnemyCombat on BasicGuard prefab at {prefabPath}");
        }
        finally
        {
            // Unload the prefab contents
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}

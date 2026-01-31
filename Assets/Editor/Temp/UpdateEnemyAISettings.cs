using UnityEngine;
using UnityEditor;
using Enemy;

public class UpdateEnemyAISettings
{
    [MenuItem("Tools/Update Enemy AI Settings")]
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
            // Update EnemyAI settings
            EnemyAI enemyAI = prefabRoot.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                SerializedObject aiSo = new SerializedObject(enemyAI);
                
                // Set dual attack ranges
                SerializedProperty meleeRange = aiSo.FindProperty("_meleeAttackRange");
                if (meleeRange != null) meleeRange.floatValue = 1.5f;

                SerializedProperty waveRange = aiSo.FindProperty("_waveAttackRange");
                if (waveRange != null) waveRange.floatValue = 6f;

                SerializedProperty waveChance = aiSo.FindProperty("_waveAttackChance");
                if (waveChance != null) waveChance.floatValue = 0.7f;

                SerializedProperty waveInterval = aiSo.FindProperty("_waveAttackInterval");
                if (waveInterval != null) waveInterval.floatValue = 2f;

                // Set telegraph times
                SerializedProperty meleeTelegraph = aiSo.FindProperty("_meleeTelegraphTime");
                if (meleeTelegraph != null) meleeTelegraph.floatValue = 0.2f;

                SerializedProperty waveTelegraph = aiSo.FindProperty("_waveTelegraphTime");
                if (waveTelegraph != null) waveTelegraph.floatValue = 0.35f;

                // Set telegraph visual settings
                SerializedProperty useTelegraphVisual = aiSo.FindProperty("_useTelegraphVisual");
                if (useTelegraphVisual != null) useTelegraphVisual.boolValue = true;

                SerializedProperty telegraphColor = aiSo.FindProperty("_telegraphColor");
                if (telegraphColor != null) telegraphColor.colorValue = new Color(1f, 0.9f, 0.3f, 1f);

                aiSo.ApplyModifiedProperties();
                Debug.Log("Updated EnemyAI with dual attack ranges and telegraph settings");
            }

            // Update EnemyCombat settings
            EnemyCombat enemyCombat = prefabRoot.GetComponent<EnemyCombat>();
            if (enemyCombat != null)
            {
                SerializedObject combatSo = new SerializedObject(enemyCombat);

                // Set projectile ignore layers to Enemy layer (layer 7)
                SerializedProperty ignoreLayers = combatSo.FindProperty("_projectileIgnoreLayers");
                if (ignoreLayers != null) ignoreLayers.intValue = 1 << 7; // Enemy layer

                combatSo.ApplyModifiedProperties();
                Debug.Log("Updated EnemyCombat with projectile ignore layers");
            }

            // Save the prefab
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            Debug.Log($"Successfully updated BasicGuard prefab at {prefabPath}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}

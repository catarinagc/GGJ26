using UnityEngine;
using UnityEditor;
using Enemy;

public class FixDuplicateComponents : MonoBehaviour
{
    [MenuItem("Tools/Fix Duplicate Components")]
    public static void Execute()
    {
        // Find BasicGuard in scene
        GameObject basicGuard = GameObject.Find("BasicGuard");
        if (basicGuard == null)
        {
            Debug.LogWarning("[FixDuplicateComponents] BasicGuard not found in scene");
            return;
        }

        // Get all EnemyHealthBarSpawner components
        EnemyHealthBarSpawner[] spawners = basicGuard.GetComponents<EnemyHealthBarSpawner>();
        
        if (spawners.Length > 1)
        {
            // Keep the first one, remove the rest
            for (int i = 1; i < spawners.Length; i++)
            {
                Object.DestroyImmediate(spawners[i]);
            }
            Debug.Log($"[FixDuplicateComponents] Removed {spawners.Length - 1} duplicate EnemyHealthBarSpawner components");
        }
        else
        {
            Debug.Log("[FixDuplicateComponents] No duplicate components found");
        }

        EditorUtility.SetDirty(basicGuard);
        
        // Also fix the prefab
        string prefabPath = "Assets/Prefabs/Enemies/BasicGuard.prefab";
        using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            GameObject prefabRoot = editScope.prefabContentsRoot;
            EnemyHealthBarSpawner[] prefabSpawners = prefabRoot.GetComponents<EnemyHealthBarSpawner>();
            
            if (prefabSpawners.Length > 1)
            {
                for (int i = 1; i < prefabSpawners.Length; i++)
                {
                    Object.DestroyImmediate(prefabSpawners[i]);
                }
                Debug.Log($"[FixDuplicateComponents] Removed {prefabSpawners.Length - 1} duplicate components from prefab");
            }
        }

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}

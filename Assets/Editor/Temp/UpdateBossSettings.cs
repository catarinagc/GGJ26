using UnityEngine;
using UnityEditor;
using Boss;

public class UpdateBossSettings
{
    [MenuItem("Tools/Update Boss Settings")]
    public static void Execute()
    {
        // Find the boss in the scene
        GameObject bossObj = GameObject.Find("Boss_CountArmand");
        if (bossObj == null)
        {
            Debug.LogError("Boss_CountArmand not found in scene!");
            return;
        }

        BossCountArmand boss = bossObj.GetComponent<BossCountArmand>();
        if (boss == null)
        {
            Debug.LogError("BossCountArmand component not found!");
            return;
        }

        // Use SerializedObject to modify private fields
        SerializedObject serializedBoss = new SerializedObject(boss);

        // Update wave settings for better visibility
        SerializedProperty waveSize = serializedBoss.FindProperty("_waveSize");
        if (waveSize != null)
        {
            waveSize.vector2Value = new Vector2(2f, 8f);
        }

        SerializedProperty waveSpeed = serializedBoss.FindProperty("_waveSpeed");
        if (waveSpeed != null)
        {
            waveSpeed.floatValue = 10f; // Slightly slower for visibility
        }

        // Update strike radius for better visibility
        SerializedProperty strikeRadius = serializedBoss.FindProperty("_strikeRadius");
        if (strikeRadius != null)
        {
            strikeRadius.floatValue = 3f; // Larger strike area
        }

        // Update telegraph duration
        SerializedProperty telegraphDuration = serializedBoss.FindProperty("_telegraphDuration");
        if (telegraphDuration != null)
        {
            telegraphDuration.floatValue = 1.5f; // More warning time
        }

        // Update falling projectile settings
        SerializedProperty fallingSpeed = serializedBoss.FindProperty("_fallingSpeed");
        if (fallingSpeed != null)
        {
            fallingSpeed.floatValue = 10f; // Slightly slower
        }

        SerializedProperty fallingCount = serializedBoss.FindProperty("_fallingProjectileCount");
        if (fallingCount != null)
        {
            fallingCount.intValue = 8; // More projectiles
        }

        SerializedProperty fallingInterval = serializedBoss.FindProperty("_fallingSpawnInterval");
        if (fallingInterval != null)
        {
            fallingInterval.floatValue = 0.3f; // Slightly more spread out
        }

        serializedBoss.ApplyModifiedProperties();
        EditorUtility.SetDirty(boss);

        Debug.Log("[UpdateBossSettings] Boss settings updated for better visibility!");
        Debug.Log($"  - Wave Size: 2x8");
        Debug.Log($"  - Wave Speed: 10");
        Debug.Log($"  - Strike Radius: 3");
        Debug.Log($"  - Telegraph Duration: 1.5s");
        Debug.Log($"  - Falling Speed: 10");
        Debug.Log($"  - Falling Count: 8");
    }
}

using UnityEngine;
using UnityEditor;
using Boss;

public class FixBossSpawnPoints
{
    [MenuItem("Tools/Fix Boss Spawn Points")]
    public static void Execute()
    {
        // Fix TopSpawnPoint - should be at top of camera view (y=12)
        GameObject topSpawn = GameObject.Find("Boss_CountArmand/TopSpawnPoint");
        if (topSpawn != null)
        {
            // Set world position directly
            topSpawn.transform.position = new Vector3(0f, 11f, 0f);
            Debug.Log($"[FixBossSpawnPoints] TopSpawnPoint moved to world position: {topSpawn.transform.position}");
        }
        
        // Fix PlatformCenter - should be at platform level (y=-4, just above ground)
        GameObject platformCenter = GameObject.Find("Boss_CountArmand/PlatformCenter");
        if (platformCenter != null)
        {
            platformCenter.transform.position = new Vector3(0f, -3f, 0f);
            Debug.Log($"[FixBossSpawnPoints] PlatformCenter moved to world position: {platformCenter.transform.position}");
        }
        
        // Update BossCountArmand script settings for better spawn positions
        GameObject boss = GameObject.Find("Boss_CountArmand");
        if (boss != null)
        {
            BossCountArmand bossScript = boss.GetComponent<BossCountArmand>();
            if (bossScript != null)
            {
                SerializedObject so = new SerializedObject(bossScript);
                
                // Noble Slashes - spawn closer to screen edges
                // Camera at x=0, ortho size 10, aspect ~1.78 = width ~17.8
                // So spawn at x=-18 and x=18 should work, but let's use -20/20 to be safe
                // Actually the issue is the spawn is happening but waves move too slow or wrong direction
                
                // Let's also reduce attack cooldown for testing
                so.FindProperty("_attackCooldown").floatValue = 2f;
                
                // Make waves faster
                so.FindProperty("_waveSpeed").floatValue = 12f;
                
                // Make falling projectiles spawn lower
                so.FindProperty("_fallingSpreadRange").floatValue = 10f;
                
                so.ApplyModifiedProperties();
                
                Debug.Log("[FixBossSpawnPoints] Boss settings updated!");
            }
        }
        
        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("[FixBossSpawnPoints] Complete! Spawn points fixed.");
    }
}

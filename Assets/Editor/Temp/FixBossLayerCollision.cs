using UnityEngine;
using UnityEditor;

public class FixBossLayerCollision
{
    [MenuItem("Tools/Fix Boss Layer Collision")]
    public static void Execute()
    {
        int bossProjectileLayer = LayerMask.NameToLayer("BossProjectile");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        int groundLayer = LayerMask.NameToLayer("Ground");

        if (bossProjectileLayer >= 0)
        {
            // BossProjectile should NOT hit Enemy layer (which now includes the boss)
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, enemyLayer, true);
            
            // BossProjectile SHOULD hit Player
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, playerLayer, false);
            
            // BossProjectile SHOULD hit Ground (to destroy on impact)
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, groundLayer, false);
            
            // BossProjectile should NOT hit other boss projectiles
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, bossProjectileLayer, true);

            Debug.Log("[FixBossLayerCollision] Layer collision updated!");
            Debug.Log($"  - BossProjectile ignores Enemy: true");
            Debug.Log($"  - BossProjectile ignores Player: false");
            Debug.Log($"  - BossProjectile ignores Ground: false");
        }
        
        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
        Debug.Log("[FixBossLayerCollision] Scene saved!");
    }
}

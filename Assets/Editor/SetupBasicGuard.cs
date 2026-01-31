using UnityEngine;
using UnityEditor;
using Combat;
using Enemy;
using Effects;

public class SetupBasicGuard : MonoBehaviour
{
    [MenuItem("Tools/Setup Basic Guard Enemy")]
    public static void Execute()
    {
        // First, set up the Player with Health component and proper layer/tag
        SetupPlayer();
        
        // Then create the BasicGuard
        CreateBasicGuard();
        
        Debug.Log("[SetupBasicGuard] Setup complete!");
    }

    private static void SetupPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("[SetupBasicGuard] Could not find Player object!");
            return;
        }

        // Set Player tag and layer
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");

        // Add Health component if not present
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<Health>();
        }

        // Configure player health via SerializedObject
        SerializedObject healthSO = new SerializedObject(playerHealth);
        healthSO.FindProperty("_maxHealth").floatValue = 100f;
        healthSO.FindProperty("_applyKnockback").boolValue = true;
        healthSO.FindProperty("_knockbackResistance").floatValue = 0f;
        healthSO.FindProperty("_invincibilityDuration").floatValue = 0.5f;
        healthSO.FindProperty("_useHitFlash").boolValue = true;
        healthSO.FindProperty("_triggerCameraShake").boolValue = true;
        healthSO.FindProperty("_cameraShakeDuration").floatValue = 0.1f;
        healthSO.FindProperty("_cameraShakeMagnitude").floatValue = 0.15f;
        healthSO.ApplyModifiedProperties();

        // Add HitEffect component if not present
        HitEffect hitEffect = player.GetComponent<HitEffect>();
        if (hitEffect == null)
        {
            hitEffect = player.AddComponent<HitEffect>();
        }

        EditorUtility.SetDirty(player);
        Debug.Log("[SetupBasicGuard] Player configured with Health and HitEffect components");
    }

    private static void CreateBasicGuard()
    {
        // Create the BasicGuard GameObject
        GameObject guard = new GameObject("BasicGuard");
        
        // Set layer to Enemy
        guard.layer = LayerMask.NameToLayer("Enemy");

        // Position it in the scene (to the right of the player)
        guard.transform.position = new Vector3(8f, 1f, 0f);

        // Add SpriteRenderer with dark blue square
        SpriteRenderer sr = guard.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/Square.png");
        sr.color = new Color(0.1f, 0.15f, 0.4f, 1f); // Dark blue color

        // Add Rigidbody2D
        Rigidbody2D rb = guard.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Add BoxCollider2D
        BoxCollider2D col = guard.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);

        // Add Health component
        Health health = guard.AddComponent<Health>();
        SerializedObject healthSO = new SerializedObject(health);
        healthSO.FindProperty("_maxHealth").floatValue = 30f; // 3 hits from player's base sword (10 damage each)
        healthSO.FindProperty("_applyKnockback").boolValue = true;
        healthSO.FindProperty("_knockbackResistance").floatValue = 2f;
        healthSO.FindProperty("_invincibilityDuration").floatValue = 0.1f;
        healthSO.FindProperty("_useHitFlash").boolValue = true;
        healthSO.FindProperty("_triggerCameraShake").boolValue = true;
        healthSO.FindProperty("_cameraShakeDuration").floatValue = 0.05f;
        healthSO.FindProperty("_cameraShakeMagnitude").floatValue = 0.1f;
        healthSO.ApplyModifiedProperties();

        // Add HitEffect component
        HitEffect hitEffect = guard.AddComponent<HitEffect>();

        // Add EnemyAI component
        EnemyAI enemyAI = guard.AddComponent<EnemyAI>();
        SerializedObject aiSO = new SerializedObject(enemyAI);
        
        // AI Settings
        aiSO.FindProperty("_detectionRange").floatValue = 8f;
        aiSO.FindProperty("_attackRange").floatValue = 1.5f;
        aiSO.FindProperty("_loseTargetRange").floatValue = 12f;
        aiSO.FindProperty("_playerLayer").intValue = 1 << LayerMask.NameToLayer("Player");
        
        // Patrol Settings
        aiSO.FindProperty("_patrolSpeed").floatValue = 3f;
        aiSO.FindProperty("_patrolWaitTime").floatValue = 1f;
        aiSO.FindProperty("_useLocalPatrolPoints").boolValue = true;
        aiSO.FindProperty("_localPatrolDistance").floatValue = 5f;
        
        // Chase Settings
        aiSO.FindProperty("_chaseSpeed").floatValue = 5f;
        
        // Attack Settings
        aiSO.FindProperty("_attackDamage").floatValue = 10f;
        aiSO.FindProperty("_attackCooldown").floatValue = 1f;
        aiSO.FindProperty("_attackKnockbackForce").floatValue = 8f;
        aiSO.FindProperty("_attackHitboxSize").vector2Value = new Vector2(1.5f, 1f);
        aiSO.FindProperty("_attackHitboxOffset").vector2Value = new Vector2(0.8f, 0f);
        aiSO.FindProperty("_attackDuration").floatValue = 0.3f;
        
        // Contact Damage
        aiSO.FindProperty("_dealContactDamage").boolValue = true;
        aiSO.FindProperty("_contactDamage").floatValue = 5f;
        aiSO.FindProperty("_contactKnockbackForce").floatValue = 6f;
        aiSO.FindProperty("_contactDamageCooldown").floatValue = 0.5f;
        
        // Enemy Base Settings
        aiSO.FindProperty("_enemyName").stringValue = "Basic Guard";
        aiSO.FindProperty("_destroyOnDeath").boolValue = true;
        aiSO.FindProperty("_destroyDelay").floatValue = 1f;
        aiSO.FindProperty("_flashOnHit").boolValue = true;
        aiSO.FindProperty("_hitFlashColor").colorValue = Color.red;
        aiSO.FindProperty("_hitFlashDuration").floatValue = 0.1f;
        
        aiSO.ApplyModifiedProperties();

        // Select the new guard
        Selection.activeGameObject = guard;
        EditorUtility.SetDirty(guard);

        Debug.Log("[SetupBasicGuard] BasicGuard created at position (8, 1, 0)");
    }
}

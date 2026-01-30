using UnityEngine;
using UnityEditor;

public class FixPlayerPhysicsRetry
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            // 1. Fix Collider Size
            CapsuleCollider2D collider = player.GetComponent<CapsuleCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(1f, 2f);
                collider.offset = Vector2.zero;
                Debug.Log("Updated Player CapsuleCollider2D size to (1, 2).");
            }
            else
            {
                Debug.LogError("CapsuleCollider2D not found on Player.");
            }

            // 2. Fix GroundCheck Position
            Transform groundCheck = player.transform.Find("GroundCheck");
            if (groundCheck != null)
            {
                groundCheck.localPosition = new Vector3(0, -1f, 0);
                Debug.Log("Updated GroundCheck localPosition to (0, -1, 0).");
            }
            else
            {
                Debug.LogError("GroundCheck child not found on Player.");
            }

            // 3. Verify Rigidbody2D
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            EditorUtility.SetDirty(player);
        }
        else
        {
            Debug.LogError("Player GameObject not found.");
        }
    }
}

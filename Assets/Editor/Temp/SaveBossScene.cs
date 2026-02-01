using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveBossScene
{
    [MenuItem("Tools/Save Boss Scene")]
    public static void Execute()
    {
        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        
        Debug.Log("[SaveBossScene] Scene saved successfully!");
        
        // Log scene summary
        var boss = GameObject.Find("Boss_CountArmand");
        var player = GameObject.Find("Player");
        var canvas = GameObject.Find("Canvas");
        
        if (boss != null)
        {
            var health = boss.GetComponent<Combat.Health>();
            Debug.Log($"[SaveBossScene] Boss HP: {health?.MaxHealth ?? 0}");
        }
        
        if (player != null)
        {
            Debug.Log($"[SaveBossScene] Player position: {player.transform.position}");
        }
        
        if (canvas != null)
        {
            var bossUI = canvas.GetComponentInChildren<UI.BossHealthUI>();
            Debug.Log($"[SaveBossScene] Boss UI found: {bossUI != null}");
        }
    }
}

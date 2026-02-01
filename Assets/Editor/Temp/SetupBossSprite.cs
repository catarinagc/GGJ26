using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SetupBossSprite
{
    [MenuItem("Tools/Setup Boss Sprite and Player Health")]
    public static void Execute()
    {
        // Import the boss sprite
        AssetDatabase.Refresh();
        
        // Configure the texture import settings
        string spritePath = "Assets/Sprites/Boss_CountArmand.png";
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            Debug.Log("[SetupBossSprite] Boss sprite imported!");
        }
        
        // Find boss and set sprite
        GameObject boss = GameObject.Find("Boss_CountArmand");
        if (boss != null)
        {
            SpriteRenderer sr = boss.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite bossSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (bossSprite != null)
                {
                    sr.sprite = bossSprite;
                    sr.color = Color.white;
                    sr.drawMode = SpriteDrawMode.Simple;
                    
                    // Adjust scale for the boss to be large in background
                    boss.transform.localScale = new Vector3(12f, 12f, 1f);
                    boss.transform.position = new Vector3(0f, 6f, 0f);
                    
                    Debug.Log("[SetupBossSprite] Boss sprite assigned!");
                }
            }
        }
        
        // Add Player Health UI
        AddPlayerHealthUI();
        
        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
        Debug.Log("[SetupBossSprite] Setup complete!");
    }
    
    private static void AddPlayerHealthUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        
        // Check if PlayerHealthUI already exists
        GameObject existingUI = GameObject.Find("PlayerHealthUI");
        if (existingUI != null)
        {
            Object.DestroyImmediate(existingUI);
        }
        
        // Create Player Health UI Container (top-left corner)
        GameObject playerHealthUI = new GameObject("PlayerHealthUI");
        playerHealthUI.transform.SetParent(canvas.transform, false);
        
        RectTransform uiRect = playerHealthUI.AddComponent<RectTransform>();
        uiRect.anchorMin = new Vector2(0f, 1f);
        uiRect.anchorMax = new Vector2(0f, 1f);
        uiRect.pivot = new Vector2(0f, 1f);
        uiRect.anchoredPosition = new Vector2(20f, -20f);
        uiRect.sizeDelta = new Vector2(300f, 50f);
        
        // Add Image component for background
        Image bgImage = playerHealthUI.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Add PlayerHealthUI script
        UI.PlayerHealthUI healthUIScript = playerHealthUI.AddComponent<UI.PlayerHealthUI>();
        
        // Create health bar background
        GameObject healthBarBG = new GameObject("HealthBarBG");
        healthBarBG.transform.SetParent(playerHealthUI.transform, false);
        RectTransform bgRect = healthBarBG.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0f);
        bgRect.anchorMax = new Vector2(1f, 1f);
        bgRect.offsetMin = new Vector2(10f, 10f);
        bgRect.offsetMax = new Vector2(-10f, -10f);
        Image healthBgImage = healthBarBG.AddComponent<Image>();
        healthBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Create health bar fill
        GameObject healthBarFill = new GameObject("HealthBarFill");
        healthBarFill.transform.SetParent(healthBarBG.transform, false);
        RectTransform fillRect = healthBarFill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = healthBarFill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green health
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;
        
        // Create health text
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(healthBarBG.transform, false);
        RectTransform textRect = healthTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text healthText = healthTextObj.AddComponent<Text>();
        healthText.text = "100/100";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 18;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;
        
        // Configure PlayerHealthUI script
        SerializedObject uiSO = new SerializedObject(healthUIScript);
        uiSO.FindProperty("_healthBarFill").objectReferenceValue = fillImage;
        uiSO.FindProperty("_healthText").objectReferenceValue = healthText;
        uiSO.ApplyModifiedProperties();
        
        Debug.Log("[SetupBossSprite] Player Health UI added!");
    }
}

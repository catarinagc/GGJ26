using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UI;
using Enemy;
using Combat;

public class SetupPlayerHUD : MonoBehaviour
{
    [MenuItem("Tools/Setup Player HUD and Game Over")]
    public static void Execute()
    {
        // Find the main canvas
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SetupPlayerHUD] No Canvas found in scene!");
            return;
        }

        // Create Player Health Bar UI
        CreatePlayerHealthBar(canvas.transform);

        // Create Low Health Vignette
        CreateLowHealthVignette(canvas.transform);

        // Create Game Over Panel
        CreateGameOverPanel(canvas.transform);

        // Create Enemy Health Bar Prefab
        CreateEnemyHealthBarPrefab();

        // Add EnemyHealthBarSpawner to BasicGuard
        SetupBasicGuardHealthBar();

        // Save the scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupPlayerHUD] HUD setup complete!");
    }

    private static void CreatePlayerHealthBar(Transform canvasTransform)
    {
        // Check if already exists
        Transform existing = canvasTransform.Find("PlayerHealthUI");
        if (existing != null)
        {
            Debug.Log("[SetupPlayerHUD] PlayerHealthUI already exists, skipping...");
            return;
        }

        // Create main panel
        GameObject healthPanel = new GameObject("PlayerHealthUI");
        healthPanel.transform.SetParent(canvasTransform, false);
        healthPanel.layer = LayerMask.NameToLayer("UI");

        RectTransform panelRect = healthPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -150); // Below MaskSystemUI
        panelRect.sizeDelta = new Vector2(200, 40);

        // Add background image
        Image panelBg = healthPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

        // Create health bar background
        GameObject healthBarBg = new GameObject("HealthBarBG");
        healthBarBg.transform.SetParent(healthPanel.transform, false);
        healthBarBg.layer = LayerMask.NameToLayer("UI");

        RectTransform bgRect = healthBarBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(1, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = new Vector2(0, 0);
        bgRect.sizeDelta = new Vector2(-20, 20);

        Image bgImage = healthBarBg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Create health bar fill
        GameObject healthBarFill = new GameObject("HealthBarFill");
        healthBarFill.transform.SetParent(healthBarBg.transform, false);
        healthBarFill.layer = LayerMask.NameToLayer("UI");

        RectTransform fillRect = healthBarFill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = healthBarFill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;

        // Create health text
        GameObject healthText = new GameObject("HealthText");
        healthText.transform.SetParent(healthPanel.transform, false);
        healthText.layer = LayerMask.NameToLayer("UI");

        RectTransform textRect = healthText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        Text text = healthText.AddComponent<Text>();
        text.text = "100/100";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        // Add PlayerHealthUI component
        PlayerHealthUI healthUI = healthPanel.AddComponent<PlayerHealthUI>();
        SerializedObject healthUISO = new SerializedObject(healthUI);
        healthUISO.FindProperty("_healthBarFill").objectReferenceValue = fillImage;
        healthUISO.FindProperty("_healthText").objectReferenceValue = text;
        healthUISO.ApplyModifiedProperties();

        Debug.Log("[SetupPlayerHUD] Created PlayerHealthUI");
    }

    private static void CreateLowHealthVignette(Transform canvasTransform)
    {
        // Check if already exists
        Transform existing = canvasTransform.Find("LowHealthVignette");
        if (existing != null)
        {
            Debug.Log("[SetupPlayerHUD] LowHealthVignette already exists, skipping...");
            return;
        }

        // Create vignette panel (full screen)
        GameObject vignettePanel = new GameObject("LowHealthVignette");
        vignettePanel.transform.SetParent(canvasTransform, false);
        vignettePanel.layer = LayerMask.NameToLayer("UI");

        RectTransform vignetteRect = vignettePanel.AddComponent<RectTransform>();
        vignetteRect.anchorMin = Vector2.zero;
        vignetteRect.anchorMax = Vector2.one;
        vignetteRect.pivot = new Vector2(0.5f, 0.5f);
        vignetteRect.anchoredPosition = Vector2.zero;
        vignetteRect.sizeDelta = Vector2.zero;

        // Add image with vignette-like appearance
        Image vignetteImage = vignettePanel.AddComponent<Image>();
        vignetteImage.color = new Color(0.8f, 0f, 0f, 0f); // Start invisible
        vignetteImage.raycastTarget = false; // Don't block input

        // Create a gradient texture for vignette effect
        // For now, use a simple solid color that will be controlled by alpha
        // A proper vignette would use a radial gradient texture

        // Add LowHealthVignette component
        LowHealthVignette vignette = vignettePanel.AddComponent<LowHealthVignette>();
        SerializedObject vignetteSO = new SerializedObject(vignette);
        vignetteSO.FindProperty("_vignetteImage").objectReferenceValue = vignetteImage;
        vignetteSO.FindProperty("_healthThreshold").floatValue = 0.3f;
        vignetteSO.FindProperty("_maxAlpha").floatValue = 0.4f;
        vignetteSO.FindProperty("_pulseEffect").boolValue = true;
        vignetteSO.ApplyModifiedProperties();

        // Move to back (render behind other UI)
        vignettePanel.transform.SetAsFirstSibling();

        Debug.Log("[SetupPlayerHUD] Created LowHealthVignette");
    }

    private static void CreateGameOverPanel(Transform canvasTransform)
    {
        // Check if already exists
        Transform existing = canvasTransform.Find("GameOverPanel");
        if (existing != null)
        {
            Debug.Log("[SetupPlayerHUD] GameOverPanel already exists, skipping...");
            return;
        }

        // Create game over panel (full screen overlay)
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasTransform, false);
        gameOverPanel.layer = LayerMask.NameToLayer("UI");

        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;

        // Add dark semi-transparent background
        Image panelBg = gameOverPanel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

        // Add canvas group for fade effect
        CanvasGroup canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Create "GAME OVER" text
        GameObject gameOverText = new GameObject("GameOverText");
        gameOverText.transform.SetParent(gameOverPanel.transform, false);
        gameOverText.layer = LayerMask.NameToLayer("UI");

        RectTransform textRect = gameOverText.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(400, 80);

        Text text = gameOverText.AddComponent<Text>();
        text.text = "GAME OVER";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.9f, 0.2f, 0.2f, 1f);
        text.fontStyle = FontStyle.Bold;

        // Create Retry button
        GameObject retryButton = new GameObject("RetryButton");
        retryButton.transform.SetParent(gameOverPanel.transform, false);
        retryButton.layer = LayerMask.NameToLayer("UI");

        RectTransform buttonRect = retryButton.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(200, 50);

        Image buttonBg = retryButton.AddComponent<Image>();
        buttonBg.color = new Color(0.2f, 0.6f, 0.8f, 1f);

        Button button = retryButton.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.7f, 1f);
        button.colors = colors;

        // Create button text
        GameObject buttonText = new GameObject("Text");
        buttonText.transform.SetParent(retryButton.transform, false);
        buttonText.layer = LayerMask.NameToLayer("UI");

        RectTransform btnTextRect = buttonText.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.pivot = new Vector2(0.5f, 0.5f);
        btnTextRect.anchoredPosition = Vector2.zero;
        btnTextRect.sizeDelta = Vector2.zero;

        Text btnText = buttonText.AddComponent<Text>();
        btnText.text = "RETRY";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 24;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        btnText.fontStyle = FontStyle.Bold;

        // Add GameOverUI component
        GameOverUI gameOverUI = gameOverPanel.AddComponent<GameOverUI>();
        SerializedObject gameOverSO = new SerializedObject(gameOverUI);
        gameOverSO.FindProperty("_gameOverPanel").objectReferenceValue = gameOverPanel;
        gameOverSO.FindProperty("_retryButton").objectReferenceValue = button;
        gameOverSO.FindProperty("_gameOverText").objectReferenceValue = text;
        gameOverSO.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
        gameOverSO.FindProperty("_showDelay").floatValue = 0.5f;
        gameOverSO.FindProperty("_pauseGameOnDeath").boolValue = true;
        gameOverSO.FindProperty("_fadeInSpeed").floatValue = 2f;
        gameOverSO.ApplyModifiedProperties();

        // Start hidden
        gameOverPanel.SetActive(false);

        Debug.Log("[SetupPlayerHUD] Created GameOverPanel");
    }

    private static void CreateEnemyHealthBarPrefab()
    {
        string prefabPath = "Assets/Prefabs/UI/EnemyHealthBar.prefab";
        
        // Check if prefab already exists
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log("[SetupPlayerHUD] EnemyHealthBar prefab already exists, skipping...");
            return;
        }

        // Create folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // Create world space canvas for health bar
        GameObject healthBarRoot = new GameObject("EnemyHealthBar");

        // Add Canvas (World Space)
        Canvas canvas = healthBarRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = healthBarRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.15f);
        canvasRect.localScale = new Vector3(1f, 1f, 1f);

        // Add CanvasGroup for fading
        CanvasGroup canvasGroup = healthBarRoot.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarRoot.transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBarRoot.transform, false);

        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = new Vector2(0.02f, 0);
        fillRect.sizeDelta = new Vector2(-0.04f, -0.04f);

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;

        // Add EnemyHealthBar component
        EnemyHealthBar healthBar = healthBarRoot.AddComponent<EnemyHealthBar>();
        SerializedObject healthBarSO = new SerializedObject(healthBar);
        healthBarSO.FindProperty("_healthBarFill").objectReferenceValue = fillImage;
        healthBarSO.FindProperty("_healthBarBackground").objectReferenceValue = bgImage;
        healthBarSO.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
        healthBarSO.FindProperty("_offset").vector3Value = new Vector3(0f, 1.2f, 0f);
        healthBarSO.FindProperty("_hideWhenFull").boolValue = true;
        healthBarSO.FindProperty("_showDuration").floatValue = 3f;
        healthBarSO.FindProperty("_fadeSpeed").floatValue = 2f;
        healthBarSO.ApplyModifiedProperties();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(healthBarRoot, prefabPath);
        Object.DestroyImmediate(healthBarRoot);

        Debug.Log("[SetupPlayerHUD] Created EnemyHealthBar prefab at " + prefabPath);
    }

    private static void SetupBasicGuardHealthBar()
    {
        // Find BasicGuard in scene
        GameObject basicGuard = GameObject.Find("BasicGuard");
        if (basicGuard == null)
        {
            Debug.LogWarning("[SetupPlayerHUD] BasicGuard not found in scene");
            return;
        }

        // Add EnemyHealthBarSpawner if not present
        EnemyHealthBarSpawner spawner = basicGuard.GetComponent<EnemyHealthBarSpawner>();
        if (spawner == null)
        {
            spawner = basicGuard.AddComponent<EnemyHealthBarSpawner>();
        }

        // Load the prefab
        GameObject healthBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/EnemyHealthBar.prefab");
        if (healthBarPrefab != null)
        {
            SerializedObject spawnerSO = new SerializedObject(spawner);
            spawnerSO.FindProperty("_healthBarPrefab").objectReferenceValue = healthBarPrefab;
            spawnerSO.FindProperty("_offset").vector3Value = new Vector3(0f, 1.2f, 0f);
            spawnerSO.FindProperty("_spawnOnFirstDamage").boolValue = true;
            spawnerSO.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(basicGuard);

        // Also update the prefab
        string prefabPath = "Assets/Prefabs/Enemies/BasicGuard.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            // Add component to prefab
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject prefabRoot = editScope.prefabContentsRoot;
                EnemyHealthBarSpawner prefabSpawner = prefabRoot.GetComponent<EnemyHealthBarSpawner>();
                if (prefabSpawner == null)
                {
                    prefabSpawner = prefabRoot.AddComponent<EnemyHealthBarSpawner>();
                }

                SerializedObject prefabSpawnerSO = new SerializedObject(prefabSpawner);
                prefabSpawnerSO.FindProperty("_healthBarPrefab").objectReferenceValue = healthBarPrefab;
                prefabSpawnerSO.FindProperty("_offset").vector3Value = new Vector3(0f, 1.2f, 0f);
                prefabSpawnerSO.FindProperty("_spawnOnFirstDamage").boolValue = true;
                prefabSpawnerSO.ApplyModifiedProperties();
            }
        }

        Debug.Log("[SetupPlayerHUD] Added EnemyHealthBarSpawner to BasicGuard");
    }
}

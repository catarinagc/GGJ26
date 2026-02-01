using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Combat;
using Boss;
using UI;
using CameraSystem;

public class SetupBossArena
{
    [MenuItem("Tools/Setup Boss Arena")]
    public static void Execute()
    {
        // Get references to existing objects
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                mainCamera = camObj.GetComponent<Camera>();
            }
        }

        // ============================================
        // 1. SETUP ARENA PLATFORM (Ground)
        // ============================================
        GameObject platform = CreatePlatform();
        
        // ============================================
        // 2. SETUP ARENA BOUNDARIES
        // ============================================
        GameObject boundaries = CreateArenaBoundaries();

        // ============================================
        // 3. SETUP FIXED CAMERA
        // ============================================
        SetupFixedCamera(mainCamera);

        // ============================================
        // 4. CREATE BOSS
        // ============================================
        GameObject boss = CreateBoss();

        // ============================================
        // 5. CREATE BOSS PROJECTILE PREFABS
        // ============================================
        CreateBossProjectilePrefabs();

        // ============================================
        // 6. CREATE PLAYER SPAWN POINT
        // ============================================
        CreatePlayerSpawnPoint();

        // ============================================
        // 7. CREATE CANVAS AND BOSS UI
        // ============================================
        CreateBossUI();

        // ============================================
        // 8. SETUP LAYER COLLISION
        // ============================================
        SetupLayerCollision();

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupBossArena] Boss arena setup complete!");
    }

    private static GameObject CreatePlatform()
    {
        // Check if platform already exists
        GameObject existingPlatform = GameObject.Find("BossArenaPlatform");
        if (existingPlatform != null)
        {
            Object.DestroyImmediate(existingPlatform);
        }

        // Create platform parent
        GameObject platform = new GameObject("BossArenaPlatform");
        platform.transform.position = Vector3.zero;

        // Create ground collider
        GameObject ground = new GameObject("Ground");
        ground.transform.SetParent(platform.transform);
        ground.transform.localPosition = new Vector3(0f, -5f, 0f);
        ground.layer = LayerMask.NameToLayer("Ground");

        BoxCollider2D groundCollider = ground.AddComponent<BoxCollider2D>();
        groundCollider.size = new Vector2(30f, 2f);

        // Add visual for ground
        SpriteRenderer groundRenderer = ground.AddComponent<SpriteRenderer>();
        groundRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        groundRenderer.color = new Color(0.3f, 0.25f, 0.2f, 1f);
        groundRenderer.drawMode = SpriteDrawMode.Tiled;
        groundRenderer.size = new Vector2(30f, 2f);
        groundRenderer.sortingOrder = -5;

        // Add Rigidbody2D for collision
        Rigidbody2D groundRb = ground.AddComponent<Rigidbody2D>();
        groundRb.bodyType = RigidbodyType2D.Static;

        // Create left wall
        GameObject leftWall = new GameObject("LeftWall");
        leftWall.transform.SetParent(platform.transform);
        leftWall.transform.localPosition = new Vector3(-16f, 2f, 0f);
        leftWall.layer = LayerMask.NameToLayer("Ground");

        BoxCollider2D leftCollider = leftWall.AddComponent<BoxCollider2D>();
        leftCollider.size = new Vector2(2f, 16f);

        Rigidbody2D leftRb = leftWall.AddComponent<Rigidbody2D>();
        leftRb.bodyType = RigidbodyType2D.Static;

        // Create right wall
        GameObject rightWall = new GameObject("RightWall");
        rightWall.transform.SetParent(platform.transform);
        rightWall.transform.localPosition = new Vector3(16f, 2f, 0f);
        rightWall.layer = LayerMask.NameToLayer("Ground");

        BoxCollider2D rightCollider = rightWall.AddComponent<BoxCollider2D>();
        rightCollider.size = new Vector2(2f, 16f);

        Rigidbody2D rightRb = rightWall.AddComponent<Rigidbody2D>();
        rightRb.bodyType = RigidbodyType2D.Static;

        Debug.Log("[SetupBossArena] Platform created");
        return platform;
    }

    private static GameObject CreateArenaBoundaries()
    {
        // Check if boundaries already exist
        GameObject existingBoundaries = GameObject.Find("ArenaBoundaries");
        if (existingBoundaries != null)
        {
            Object.DestroyImmediate(existingBoundaries);
        }

        GameObject boundaries = new GameObject("ArenaBoundaries");
        boundaries.transform.position = Vector3.zero;

        // Create TOP COLLIDER to prevent jumping out
        GameObject topCollider = new GameObject("TopCollider");
        topCollider.transform.SetParent(boundaries.transform);
        topCollider.transform.localPosition = new Vector3(0f, 12f, 0f);
        topCollider.layer = LayerMask.NameToLayer("Ground");

        BoxCollider2D topBox = topCollider.AddComponent<BoxCollider2D>();
        topBox.size = new Vector2(40f, 2f);

        Rigidbody2D topRb = topCollider.AddComponent<Rigidbody2D>();
        topRb.bodyType = RigidbodyType2D.Static;

        Debug.Log("[SetupBossArena] Arena boundaries created with top collider");
        return boundaries;
    }

    private static void SetupFixedCamera(Camera mainCamera)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[SetupBossArena] Main camera not found!");
            return;
        }

        // Position camera to view entire arena
        mainCamera.transform.position = new Vector3(0f, 3f, -10f);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 10f;

        // Disable CameraFollow for fixed view
        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
            Debug.Log("[SetupBossArena] CameraFollow disabled for fixed boss fight view");
        }

        Debug.Log("[SetupBossArena] Camera configured for fixed arena view");
    }

    private static GameObject CreateBoss()
    {
        // Check if boss already exists
        GameObject existingBoss = GameObject.Find("Boss_CountArmand");
        if (existingBoss != null)
        {
            Object.DestroyImmediate(existingBoss);
        }

        // Create boss GameObject
        GameObject boss = new GameObject("Boss_CountArmand");
        boss.transform.position = new Vector3(0f, 5f, 5f); // Background position
        boss.tag = "Boss";
        boss.layer = LayerMask.NameToLayer("Background");

        // Add SpriteRenderer for visual
        SpriteRenderer spriteRenderer = boss.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        spriteRenderer.color = new Color(0.4f, 0.2f, 0.3f, 1f); // Dark purple/maroon
        spriteRenderer.sortingOrder = -10;
        boss.transform.localScale = new Vector3(8f, 12f, 1f); // Large background boss

        // Add BoxCollider2D (Trigger) for being hit by sword waves
        BoxCollider2D collider = boss.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f); // Will be scaled with transform

        // Add Health component
        Health health = boss.AddComponent<Health>();
        // Set health via SerializedObject
        SerializedObject healthSO = new SerializedObject(health);
        healthSO.FindProperty("_maxHealth").floatValue = 500f;
        healthSO.FindProperty("_currentHealth").floatValue = 500f;
        healthSO.FindProperty("_applyKnockback").boolValue = false;
        healthSO.FindProperty("_invincibilityDuration").floatValue = 0.1f;
        healthSO.FindProperty("_triggerCameraShake").boolValue = true;
        healthSO.FindProperty("_cameraShakeDuration").floatValue = 0.1f;
        healthSO.FindProperty("_cameraShakeMagnitude").floatValue = 0.2f;
        healthSO.ApplyModifiedProperties();

        // Add BossCountArmand component
        BossCountArmand bossScript = boss.AddComponent<BossCountArmand>();
        
        // Create spawn points
        GameObject topSpawnPoint = new GameObject("TopSpawnPoint");
        topSpawnPoint.transform.SetParent(boss.transform);
        topSpawnPoint.transform.localPosition = new Vector3(0f, 1.5f, 0f); // Above boss, will be at y=11 world

        GameObject platformCenter = new GameObject("PlatformCenter");
        platformCenter.transform.SetParent(boss.transform);
        platformCenter.transform.localPosition = new Vector3(0f, -0.8f, 0f); // At platform level

        // Configure boss script
        SerializedObject bossSO = new SerializedObject(bossScript);
        bossSO.FindProperty("_attackCooldown").floatValue = 3f;
        bossSO.FindProperty("_patternTransitionTime").floatValue = 1.5f;
        bossSO.FindProperty("_waveSpeed").floatValue = 8f;
        bossSO.FindProperty("_waveDamage").floatValue = 15f;
        bossSO.FindProperty("_waveKnockback").floatValue = 10f;
        bossSO.FindProperty("_waveCount").intValue = 3;
        bossSO.FindProperty("_fallingSpeed").floatValue = 12f;
        bossSO.FindProperty("_fallingDamage").floatValue = 10f;
        bossSO.FindProperty("_fallingProjectileCount").intValue = 5;
        bossSO.FindProperty("_fallingSpreadRange").floatValue = 8f;
        bossSO.FindProperty("_telegraphDuration").floatValue = 1.2f;
        bossSO.FindProperty("_strikeDamage").floatValue = 25f;
        bossSO.FindProperty("_strikeRadius").floatValue = 2f;
        bossSO.FindProperty("_topSpawnPoint").objectReferenceValue = topSpawnPoint.transform;
        bossSO.FindProperty("_platformCenter").objectReferenceValue = platformCenter.transform;
        bossSO.FindProperty("_spriteRenderer").objectReferenceValue = spriteRenderer;
        bossSO.FindProperty("_playerLayer").intValue = 1 << LayerMask.NameToLayer("Player");
        bossSO.ApplyModifiedProperties();

        Debug.Log("[SetupBossArena] Boss_CountArmand created with 500 HP");
        return boss;
    }

    private static void CreateBossProjectilePrefabs()
    {
        string prefabFolder = "Assets/Prefabs/Boss";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Boss");
        }

        // Create Wave Projectile Prefab
        CreateWaveProjectilePrefab(prefabFolder);

        // Create Falling Projectile Prefab
        CreateFallingProjectilePrefab(prefabFolder);

        // Create Telegraph Prefab
        CreateTelegraphPrefab(prefabFolder);

        // Create Strike Prefab
        CreateStrikePrefab(prefabFolder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assign prefabs to boss
        AssignPrefabsToBoss(prefabFolder);
    }

    private static void CreateWaveProjectilePrefab(string folder)
    {
        string path = $"{folder}/BossWaveProjectile.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        GameObject wave = new GameObject("BossWaveProjectile");
        wave.layer = LayerMask.NameToLayer("BossProjectile");

        SpriteRenderer sr = wave.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        sr.color = new Color(0.8f, 0.2f, 0.8f, 0.8f); // Purple wave
        sr.sortingOrder = 5;

        BoxCollider2D col = wave.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = wave.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        BossProjectile proj = wave.AddComponent<BossProjectile>();
        SerializedObject projSO = new SerializedObject(proj);
        projSO.FindProperty("_obstacleLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
        projSO.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(wave, path);
        Object.DestroyImmediate(wave);

        Debug.Log("[SetupBossArena] Wave projectile prefab created");
    }

    private static void CreateFallingProjectilePrefab(string folder)
    {
        string path = $"{folder}/BossFallingProjectile.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        GameObject falling = new GameObject("BossFallingProjectile");
        falling.layer = LayerMask.NameToLayer("BossProjectile");

        SpriteRenderer sr = falling.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0.3f, 0.3f, 1f); // Red projectile
        sr.sortingOrder = 5;
        falling.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        CircleCollider2D col = falling.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody2D rb = falling.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        BossProjectile proj = falling.AddComponent<BossProjectile>();
        SerializedObject projSO = new SerializedObject(proj);
        projSO.FindProperty("_obstacleLayer").intValue = 1 << LayerMask.NameToLayer("Ground");
        projSO.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(falling, path);
        Object.DestroyImmediate(falling);

        Debug.Log("[SetupBossArena] Falling projectile prefab created");
    }

    private static void CreateTelegraphPrefab(string folder)
    {
        string path = $"{folder}/BossTelegraph.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        GameObject telegraph = new GameObject("BossTelegraph");

        SpriteRenderer sr = telegraph.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 0f, 0f, 0.5f); // Semi-transparent red
        sr.sortingOrder = 4;

        PrefabUtility.SaveAsPrefabAsset(telegraph, path);
        Object.DestroyImmediate(telegraph);

        Debug.Log("[SetupBossArena] Telegraph prefab created");
    }

    private static void CreateStrikePrefab(string folder)
    {
        string path = $"{folder}/BossStrike.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        GameObject strike = new GameObject("BossStrike");

        SpriteRenderer sr = strike.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(1f, 1f, 0f, 1f); // Bright yellow
        sr.sortingOrder = 6;

        PrefabUtility.SaveAsPrefabAsset(strike, path);
        Object.DestroyImmediate(strike);

        Debug.Log("[SetupBossArena] Strike prefab created");
    }

    private static void AssignPrefabsToBoss(string folder)
    {
        GameObject boss = GameObject.Find("Boss_CountArmand");
        if (boss == null) return;

        BossCountArmand bossScript = boss.GetComponent<BossCountArmand>();
        if (bossScript == null) return;

        SerializedObject bossSO = new SerializedObject(bossScript);

        GameObject wavePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/BossWaveProjectile.prefab");
        GameObject fallingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/BossFallingProjectile.prefab");
        GameObject telegraphPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/BossTelegraph.prefab");
        GameObject strikePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/BossStrike.prefab");

        bossSO.FindProperty("_waveProjectilePrefab").objectReferenceValue = wavePrefab;
        bossSO.FindProperty("_fallingProjectilePrefab").objectReferenceValue = fallingPrefab;
        bossSO.FindProperty("_telegraphPrefab").objectReferenceValue = telegraphPrefab;
        bossSO.FindProperty("_strikePrefab").objectReferenceValue = strikePrefab;
        bossSO.ApplyModifiedProperties();

        Debug.Log("[SetupBossArena] Prefabs assigned to boss");
    }

    private static void CreatePlayerSpawnPoint()
    {
        GameObject existingSpawn = GameObject.Find("PlayerSpawnPoint");
        if (existingSpawn != null)
        {
            Object.DestroyImmediate(existingSpawn);
        }

        GameObject spawnPoint = new GameObject("PlayerSpawnPoint");
        spawnPoint.transform.position = new Vector3(-10f, -3f, 0f);

        // Add a visual indicator in editor
        SpriteRenderer sr = spawnPoint.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        sr.color = new Color(0f, 1f, 0f, 0.5f);
        spawnPoint.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        Debug.Log("[SetupBossArena] Player spawn point created");
    }

    private static void CreateBossUI()
    {
        // Find or create Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Configure CanvasScaler
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Check if BossHealthUI already exists
        GameObject existingUI = GameObject.Find("BossHealthUI");
        if (existingUI != null)
        {
            Object.DestroyImmediate(existingUI);
        }

        // Create Boss Health UI Container
        GameObject bossHealthUI = new GameObject("BossHealthUI");
        bossHealthUI.transform.SetParent(canvas.transform, false);

        RectTransform uiRect = bossHealthUI.AddComponent<RectTransform>();
        uiRect.anchorMin = new Vector2(0.5f, 0f);
        uiRect.anchorMax = new Vector2(0.5f, 0f);
        uiRect.pivot = new Vector2(0.5f, 0f);
        uiRect.anchoredPosition = new Vector2(0f, 50f);
        uiRect.sizeDelta = new Vector2(800f, 80f);

        CanvasGroup canvasGroup = bossHealthUI.AddComponent<CanvasGroup>();

        // Create background frame (ornate border)
        GameObject frame = new GameObject("Frame");
        frame.transform.SetParent(bossHealthUI.transform, false);
        RectTransform frameRect = frame.AddComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = new Vector2(-10f, -10f);
        frameRect.offsetMax = new Vector2(10f, 10f);
        Image frameImage = frame.AddComponent<Image>();
        frameImage.color = new Color(0.3f, 0.2f, 0.1f, 1f); // Dark gold/brown

        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(bossHealthUI.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Create health slider
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(bossHealthUI.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0f);
        sliderRect.anchorMax = new Vector2(1f, 0.6f);
        sliderRect.offsetMin = new Vector2(10f, 10f);
        sliderRect.offsetMax = new Vector2(-10f, 0f);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.interactable = false;

        // Slider background
        GameObject sliderBg = new GameObject("SliderBackground");
        sliderBg.transform.SetParent(sliderObj.transform, false);
        RectTransform sliderBgRect = sliderBg.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.offsetMin = Vector2.zero;
        sliderBgRect.offsetMax = Vector2.zero;
        Image sliderBgImage = sliderBg.AddComponent<Image>();
        sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Slider fill area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Slider fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.8f, 0.1f, 0.1f, 1f); // Red health

        slider.fillRect = fillRect;

        // Create boss name text
        GameObject nameTextObj = new GameObject("BossNameText");
        nameTextObj.transform.SetParent(bossHealthUI.transform, false);
        RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.6f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(10f, 0f);
        nameRect.offsetMax = new Vector2(-10f, -5f);

        Text nameText = nameTextObj.AddComponent<Text>();
        nameText.text = "COUNT ARMAND";
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 24;
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = new Color(0.9f, 0.8f, 0.6f, 1f); // Gold text

        // Create health text
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(sliderObj.transform, false);
        RectTransform healthTextRect = healthTextObj.AddComponent<RectTransform>();
        healthTextRect.anchorMin = Vector2.zero;
        healthTextRect.anchorMax = Vector2.one;
        healthTextRect.offsetMin = Vector2.zero;
        healthTextRect.offsetMax = Vector2.zero;

        Text healthText = healthTextObj.AddComponent<Text>();
        healthText.text = "500/500";
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 18;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.color = Color.white;

        // Add BossHealthUI component
        BossHealthUI bossHealthUIScript = bossHealthUI.AddComponent<BossHealthUI>();
        SerializedObject uiSO = new SerializedObject(bossHealthUIScript);
        uiSO.FindProperty("_healthSlider").objectReferenceValue = slider;
        uiSO.FindProperty("_healthFill").objectReferenceValue = fillImage;
        uiSO.FindProperty("_bossNameText").objectReferenceValue = nameText;
        uiSO.FindProperty("_healthText").objectReferenceValue = healthText;
        uiSO.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
        uiSO.ApplyModifiedProperties();

        Debug.Log("[SetupBossArena] Boss Health UI created");
    }

    private static void SetupLayerCollision()
    {
        // BossProjectile layer (10) should:
        // - Collide with Player layer (8)
        // - Collide with Ground layer (6)
        // - NOT collide with Background layer (9) - the boss itself
        // - NOT collide with Enemy layer (7)

        int bossProjectileLayer = LayerMask.NameToLayer("BossProjectile");
        int playerLayer = LayerMask.NameToLayer("Player");
        int groundLayer = LayerMask.NameToLayer("Ground");
        int backgroundLayer = LayerMask.NameToLayer("Background");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (bossProjectileLayer >= 0)
        {
            // Ignore collision with Background (boss)
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, backgroundLayer, true);
            
            // Ignore collision with Enemy
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, enemyLayer, true);
            
            // Ignore collision with other boss projectiles
            Physics2D.IgnoreLayerCollision(bossProjectileLayer, bossProjectileLayer, true);

            Debug.Log("[SetupBossArena] Layer collision configured for BossProjectile");
        }
        else
        {
            Debug.LogWarning("[SetupBossArena] BossProjectile layer not found!");
        }
    }
}

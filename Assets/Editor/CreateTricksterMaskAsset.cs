using UnityEngine;
using UnityEditor;
using Masks;

public static class CreateTricksterMaskAsset
{
    [MenuItem("Tools/Create Trickster Mask Asset")]
    public static void Execute()
    {
        // Create the MaskData asset
        MaskData tricksterMask = ScriptableObject.CreateInstance<MaskData>();

        // Use SerializedObject to set private serialized fields
        SerializedObject serializedMask = new SerializedObject(tricksterMask);
        
        serializedMask.FindProperty("_maskName").stringValue = "Trickster Mask";
        serializedMask.FindProperty("_description").stringValue = "A mischievous mask that grants increased agility. The wearer moves with supernatural speed, leaving opponents struggling to keep up.";
        serializedMask.FindProperty("_speedMultiplier").floatValue = 1.2f;
        serializedMask.FindProperty("_damageMultiplier").floatValue = 1f;
        serializedMask.FindProperty("_jumpForceBonus").floatValue = 0f;

        // Load and assign the ability prefab
        MonoBehaviour abilityPrefab = AssetDatabase.LoadAssetAtPath<MonoBehaviour>("Assets/Prefabs/Masks/TricksterAbility.prefab");
        if (abilityPrefab != null)
        {
            serializedMask.FindProperty("_abilityPrefab").objectReferenceValue = abilityPrefab;
        }
        else
        {
            // Try loading the GameObject and getting the component
            GameObject prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Masks/TricksterAbility.prefab");
            if (prefabGO != null)
            {
                var ability = prefabGO.GetComponent<Masks.Abilities.TricksterAbility>();
                if (ability != null)
                {
                    serializedMask.FindProperty("_abilityPrefab").objectReferenceValue = ability;
                }
            }
        }

        serializedMask.ApplyModifiedPropertiesWithoutUndo();

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Data/Masks"))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Masks");
        }

        // Save the asset
        string assetPath = "Assets/Data/Masks/TricksterMask.asset";
        AssetDatabase.CreateAsset(tricksterMask, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateTricksterMaskAsset] Created Trickster Mask asset at: {assetPath}");
        
        // Select the created asset
        Selection.activeObject = tricksterMask;
        EditorGUIUtility.PingObject(tricksterMask);
    }
}

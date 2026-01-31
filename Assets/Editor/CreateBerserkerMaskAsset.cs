using UnityEngine;
using UnityEditor;
using Masks;

public static class CreateBerserkerMaskAsset
{
    [MenuItem("Tools/Create Berserker Mask Asset")]
    public static void Execute()
    {
        // Create the MaskData asset
        MaskData berserkerMask = ScriptableObject.CreateInstance<MaskData>();

        // Use SerializedObject to set private serialized fields
        SerializedObject serializedMask = new SerializedObject(berserkerMask);
        
        serializedMask.FindProperty("_maskName").stringValue = "Berserker Mask";
        serializedMask.FindProperty("_description").stringValue = "A fearsome mask that channels raw fury. The wearer deals devastating damage but sacrifices some agility.";
        serializedMask.FindProperty("_speedMultiplier").floatValue = 0.9f; // Slightly slower
        serializedMask.FindProperty("_damageMultiplier").floatValue = 1.5f; // 50% more damage
        serializedMask.FindProperty("_jumpForceBonus").floatValue = 0f;

        // Load and assign the ability prefab
        GameObject prefabGO = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Masks/BerserkerAbility.prefab");
        if (prefabGO != null)
        {
            var ability = prefabGO.GetComponent<Masks.Abilities.BerserkerAbility>();
            if (ability != null)
            {
                serializedMask.FindProperty("_abilityPrefab").objectReferenceValue = ability;
            }
        }

        serializedMask.ApplyModifiedPropertiesWithoutUndo();

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Data/Masks"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
            {
                AssetDatabase.CreateFolder("Assets", "Data");
            }
            AssetDatabase.CreateFolder("Assets/Data", "Masks");
        }

        // Save the asset
        string assetPath = "Assets/Data/Masks/BerserkerMask.asset";
        AssetDatabase.CreateAsset(berserkerMask, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateBerserkerMaskAsset] Created Berserker Mask asset at: {assetPath}");
        
        // Select the created asset
        Selection.activeObject = berserkerMask;
        EditorGUIUtility.PingObject(berserkerMask);
    }
}

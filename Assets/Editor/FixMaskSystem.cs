using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Masks;

public static class FixMaskSystem
{
    [MenuItem("Tools/Fix Mask System")]
    public static void Execute()
    {
        // 1. Fix InteractionPrompt RectTransform
        var promptGO = GameObject.Find("Canvas/InteractionPrompt");
        if (promptGO != null)
        {
            var rect = promptGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.2f);
                rect.anchorMax = new Vector2(0.5f, 0.2f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(300f, 50f);
                Debug.Log("[FixMaskSystem] Fixed InteractionPrompt RectTransform.");
            }

            var image = promptGO.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0f, 0f, 0f, 0.85f);
                Debug.Log("[FixMaskSystem] Fixed InteractionPrompt Color.");
            }
        }
        else
        {
            Debug.LogError("[FixMaskSystem] InteractionPrompt not found!");
        }

        // 2. Fix MaskPickup Data
        var pickupGO = GameObject.Find("MaskPickup");
        if (pickupGO != null)
        {
            var interactable = pickupGO.GetComponent<MaskInteractable>();
            if (interactable != null)
            {
                // Load Berserker Mask
                var berserkerMask = AssetDatabase.LoadAssetAtPath<MaskData>("Assets/Data/Masks/BerserkerMask.asset");
                if (berserkerMask != null)
                {
                    // Use SerializedObject to set private field
                    SerializedObject so = new SerializedObject(interactable);
                    so.FindProperty("_maskData").objectReferenceValue = berserkerMask;
                    so.ApplyModifiedProperties();
                    Debug.Log("[FixMaskSystem] Assigned BerserkerMask to MaskPickup.");
                    
                    // Force visual update
                    interactable.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    Debug.LogError("[FixMaskSystem] BerserkerMask asset not found!");
                }
            }
        }
        else
        {
            Debug.LogError("[FixMaskSystem] MaskPickup not found!");
        }
    }
}

using UnityEngine;
using UnityEditor;

public static class DisableInteractionPrompt
{
    [MenuItem("Tools/Disable Interaction Prompt")]
    public static void Execute()
    {
        var promptGO = GameObject.Find("Canvas/InteractionPrompt");
        if (promptGO != null)
        {
            promptGO.SetActive(false);
            Debug.Log("[DisableInteractionPrompt] Disabled InteractionPrompt.");
        }
        else
        {
            Debug.LogError("[DisableInteractionPrompt] InteractionPrompt not found!");
        }
    }
}

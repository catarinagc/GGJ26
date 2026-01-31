using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public static class CleanupMaskUI
{
    [MenuItem("Tools/Cleanup Mask UI")]
    public static void Execute()
    {
        // Find the MaskSystemUI panel
        var maskSystemUI = GameObject.Find("Canvas/MaskSystemUI");
        if (maskSystemUI == null)
        {
            Debug.LogError("MaskSystemUI not found!");
            return;
        }

        // Get all children and remove duplicates
        var children = new List<Transform>();
        for (int i = 0; i < maskSystemUI.transform.childCount; i++)
        {
            children.Add(maskSystemUI.transform.GetChild(i));
        }

        // Track names we've seen
        var seenNames = new HashSet<string>();
        var toDelete = new List<GameObject>();

        foreach (var child in children)
        {
            if (seenNames.Contains(child.name))
            {
                toDelete.Add(child.gameObject);
            }
            else
            {
                seenNames.Add(child.name);
            }
        }

        // Delete duplicates
        foreach (var obj in toDelete)
        {
            Debug.Log($"Deleting duplicate: {obj.name}");
            Object.DestroyImmediate(obj);
        }

        // Also clean up CooldownBarBG children
        var cooldownBG = maskSystemUI.transform.Find("CooldownBarBG");
        if (cooldownBG != null)
        {
            var bgChildren = new List<Transform>();
            for (int i = 0; i < cooldownBG.childCount; i++)
            {
                bgChildren.Add(cooldownBG.GetChild(i));
            }

            seenNames.Clear();
            toDelete.Clear();

            foreach (var child in bgChildren)
            {
                if (seenNames.Contains(child.name))
                {
                    toDelete.Add(child.gameObject);
                }
                else
                {
                    seenNames.Add(child.name);
                }
            }

            foreach (var obj in toDelete)
            {
                Debug.Log($"Deleting duplicate in CooldownBarBG: {obj.name}");
                Object.DestroyImmediate(obj);
            }
        }

        EditorUtility.SetDirty(maskSystemUI);
        Debug.Log("[CleanupMaskUI] Cleanup complete!");
    }
}

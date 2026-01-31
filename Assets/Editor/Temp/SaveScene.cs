using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveScene
{
    public static void Execute()
    {
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved!");
    }
}

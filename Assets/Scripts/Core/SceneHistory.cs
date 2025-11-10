using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHistory
{
    static Stack<string> history = new Stack<string>();
    static bool initialized = false;

    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // Jangan push jika oldScene kosong atau sama dengan newScene
        if (!string.IsNullOrEmpty(oldScene.name) && oldScene.name != newScene.name)
        {
            history.Push(oldScene.name);
        }
    }

    public static bool CanGoBack()
    {
        return history.Count > 0;
    }

    public static void GoBack()
    {
        if (!CanGoBack()) return;
        var prevSceneName = history.Pop();
        // load by name, build index urutan tidak masalah
        SceneManager.LoadScene(prevSceneName);
    }

    // Opsional: clear history (mis. saat mulai game baru)
    public static void Clear()
    {
        history.Clear();
    }
}
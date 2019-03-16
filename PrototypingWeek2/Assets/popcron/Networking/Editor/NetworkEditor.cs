using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Popcron.Networking;
using Popcron.Networking.Shared;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class NetworkEditor : Editor
{
    private static bool IsCompiling
    {
        get
        {
            return EditorPrefs.GetBool("IsCompiling", false);
        }
        set
        {
            EditorPrefs.SetBool("IsCompiling", value);
        }
    }

    static NetworkEditor()
    {
        Initialize();

        EditorSceneManager.activeSceneChangedInEditMode += OnChanged;

        EditorApplication.quitting += OnQuit;
        EditorApplication.update += OnUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnChanged(Scene arg0, Scene arg1)
    {
        if (Settings.CurrentlyUniqueID == 0)
        {
            Settings.CurrentlyUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        NetworkManager.Start();
    }

    private static async void Initialize()
    {
        int tries = 0;
        int maxTries = 100;
        Scene scene;
        do
        {
            scene = SceneManager.GetActiveScene();
            await Task.Delay(10);
            tries++;
        }
        while (scene.name == "" && tries < maxTries);

        if (Settings.CurrentlyUniqueID == 0)
        {
            Settings.CurrentlyUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        NetworkManager.Start();
    }

    [MenuItem("Popcron/Networking/Open application")]
    public static void OpenApplication()
    {
        if (Settings.CurrentlyUniqueID == 0)
        {
            Settings.CurrentlyUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        NetworkManager.Start();
    }

    [MenuItem("Popcron/Networking/Close application")]
    public static void CloseApplication()
    {
        NetworkManager.Close();
    }

    [MenuItem("Popcron/Networking/Restart application")]
    public static async void RestartApplication()
    {
        NetworkManager.Close();
        await Task.Delay(1000);

        Settings.CurrentlyUniqueID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        NetworkManager.Start();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        Message message = new Message(PipeMessageType.EditorPlayModeStateChangedRequest);
        message.Write((byte)state);
        NetworkManager.SendToOtherApp(message);
    }

    private static void OnUpdate()
    {
        if (!Application.isPlaying)
        {
            NetworkManager.Poll();
        }

        if (IsCompiling != EditorApplication.isCompiling)
        {
            IsCompiling = EditorApplication.isCompiling;
            if (IsCompiling)
            {
                Message message = new Message(PipeMessageType.EditorCompileStartedRequest);
                NetworkManager.SendToOtherApp(message);
            }
            else
            {
                Message message = new Message(PipeMessageType.EditorFinishedCompilingRequest);
                NetworkManager.SendToOtherApp(message);
            }
        }
    }

    private static void OnQuit()
    {
        NetworkManager.Close(true);
    }
}
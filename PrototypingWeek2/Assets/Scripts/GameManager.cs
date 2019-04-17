using System.Threading.Tasks;
using Popcron.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public UnityEngine.UI.Slider loadingBar;
    public TMPro.TMP_Text loadingText;

    public async void Host(int level)
    {
        try
        {
            await LoadGame();

            LevelManager.Instance.SetLevel(level);
            CommandsNetworking.Host();

            while (!PlayerUser.Local) await Task.Delay(20);

            PlayerUser.Local.isAlive = true;
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Host Error: \n" + e);
            SceneManager.LoadScene(0);
        }
    }

    public async void Connect()
    {
        try
        {
            MultiplayerManager.onMessage += RecieveLevel;
            
            await LoadGame();

            CommandsNetworking.Connect();

            while (!recievedLevel && !PlayerUser.Local && !LevelManager.Instance) 
            {
                loadingBar.value = 0;
                loadingText.text = "Fetching Host Level...";
                await Task.Delay(20);
            }
            LevelManager.Instance.SetLevel(levelToLoad);
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Connection Error: \n" + e);
            SceneManager.LoadScene(0);
        }
    }

    async Task LoadGame()
    {
        AsyncOperation levelLoad = SceneManager.LoadSceneAsync("CTF");

        while (!levelLoad.isDone)
        {
            loadingBar.value = levelLoad.progress;
            loadingText.text = string.Format("Loading Scene...({0:p})", levelLoad.progress);
            await Task.Delay(20);
        }

        while (!LevelManager.hasLoadedLevels)
        {
            loadingBar.value = 0;
            loadingText.text = "Loading Levels...";
            await Task.Delay(20);
        }
    }

    bool recievedLevel = false;
    int levelToLoad = 0;
    public void RecieveLevel(Message message)
    {
        if ((NMType)message.Type == NMType.LevelSend)
        {
            MultiplayerManager.onMessage -= RecieveLevel;
            message.Rewind();
            levelToLoad = message.Read<int>();
            recievedLevel = true;
        }
    }

    public void LoadScene(int buildIndex)
    {
        Net.Disconnect();
        SceneManager.LoadScene(buildIndex);
    }
    
    public void Quit()
    {
        Application.Quit();
    }

    public void TrySpawnPlayer(GameObject cam)
    {
        try
        {
            PlayerUser.Local.isAlive = true;
            cam.SetActive(false);
        }
        catch (NullReferenceException e)
        {
            Debug.LogError("Nope: \n" + e);
        }
    }
}

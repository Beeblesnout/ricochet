using System.Threading.Tasks;
using Popcron.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        catch
        {
            Debug.Log("Could not host");
        }
    }

    public async void Connect()
    {
        try
        {
            MultiplayerManager.onMessage += RecieveLevel;
            
            await LoadGame();

            CommandsNetworking.Connect();

            while (!recievedLevel) await Task.Delay(25);

            LevelManager.Instance.SetLevel(levelToLoad);
            PlayerUser.Local.isAlive = true;
        }
        catch
        {
            Debug.Log("Could not connect");
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
    
    public void Quit()
    {
        Application.Quit();
    }
}

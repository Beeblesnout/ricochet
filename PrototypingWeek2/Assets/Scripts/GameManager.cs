using System.Threading.Tasks;
using Popcron.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public async void Host(int level)
    {
        AsyncOperation levelLoad = SceneManager.LoadSceneAsync("CTF");
        while (!levelLoad.isDone || !LevelManager.hasLoadedLevels) await Task.Delay(25);
        LevelManager.Instance.LoadLevels();
        LevelManager.Instance.SetLevel(level);
        CommandsNetworking.Host();
    }

    public async void Connect()
    {
        MultiplayerManager.onMessage += RecieveLevel;
        AsyncOperation levelLoad = SceneManager.LoadSceneAsync("CTF");
        LevelManager.Instance.LoadLevels();
        CommandsNetworking.Connect();

        while (!recievedLevel || ) await Task.Delay(25);

        LevelManager.Instance.SetLevel(levelToLoad);
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

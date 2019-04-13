using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Popcron.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void Host()
    {
        SceneManager.LoadScene("CTF");
        CommandsNetworking.Host();
        PlayerUser.Local.isAlive = true;
    }

    public Task RecieveLevel(Message message)
    {
        bool recieved = false;
        return Task.Run(() => {
            
        });
    }

    public async void Connect()
    {
        CommandsNetworking.Connect();
        
        SceneManager.LoadScene("CTF");
    }
}

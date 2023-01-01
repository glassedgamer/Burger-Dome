using UnityEngine;
using UnityEngine.Events;

public class SteamManager : MonoBehaviour {
    
    public uint appId;

    void Awake() {
        DontDestroyOnLoad(this);

        try {
            Steamworks.SteamClient.Init(appId, true);
            Debug.Log("Steam is up and running!");
        }
        catch (System.Exception e) {
            Debug.Log(e.Message);
        }
    }

    void OnApplicationQuit() {
        try {
            Steamworks.SteamClient.Shutdown();
        }
        catch {

        }
    }

}

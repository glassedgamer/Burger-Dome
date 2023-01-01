using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SteamLobbyManager : MonoBehaviour {

    public static Lobby currentLobby;

    public static bool userInLobby;

    public UnityEvent OnLobbyCreated;
    public UnityEvent OnLobbyJoined;
    public UnityEvent OnLobbyLeave;

    public GameObject InLobbyFriend;
    public Transform content;

    public Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();

    void Start() {
        DontDestroyOnLoad(this);

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnChatMessage += OnChatMessage;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite; 
    }

    void OnLobbyInvite(Friend friend, Lobby lobby) {
        //UI
        Debug.Log($"{friend.Name} invited you to their lobby");
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id) {}

    private async void OnLobbyMemberJoined(Lobby lobby, Friend friend) {
        Debug.Log($"{friend.Name} joined the lobby");
        GameObject obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = friend.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);
        inLobby.Add(friend.Id, obj);
    }

    void OnLobbyMemberDisconnected(Lobby lobby, Friend friend) {
        Debug.Log($"{friend.Name} left the lobby");
        Debug.Log($"New lobby owner is {currentLobby.Owner}");

        if(inLobby.ContainsKey(friend.Id)) {
            Destroy(inLobby[friend.Id]);
            inLobby.Remove(friend.Id);
        }
    }

    void OnChatMessage(Lobby lobby, Friend friend, string message) {
        Debug.Log($"Incoming chat message from {friend.Name} : {message}");
    }

    async void OnGameLobbyJoinRequest(Lobby joinedLobby, SteamId id) {
        RoomEnter joinedLobbySuccessfully = await joinedLobby.Join();

        if(joinedLobbySuccessfully != RoomEnter.Success) {
            Debug.Log("failed to join lobby" + joinedLobbySuccessfully);
        } else {
            currentLobby = joinedLobby;
        }
    }

    void OnLobbyCreatedCallback(Result result, Lobby lobby) {
        if(result != Result.OK) {
            Debug.Log("lobby creation result not ok : " + result);
        } else {
            OnLobbyCreated.Invoke();
            Debug.Log("Lobby Created");
        }
    }

    async void OnLobbyEntered(Lobby lobby) {
        Debug.Log("Client joined the lobby");

        userInLobby = true;

        foreach (var user in inLobby.Values) {
            Destroy(user);
        }
        inLobby.Clear();

        GameObject obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = SteamClient.Name;
        obj.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(SteamClient.SteamId);

        inLobby.Add(SteamClient.SteamId, obj);

        foreach (var friend in currentLobby.Members) {
            if(friend.Id != SteamClient.SteamId) {
                GameObject obj2 = Instantiate(InLobbyFriend, content);
                obj2.GetComponentInChildren<Text>().text = friend.Name;
                obj2.GetComponentInChildren<RawImage>().texture = await SteamFriendsManager.GetTextureFromSteamIdAsync(friend.Id);

                inLobby.Add(friend.Id, obj2);
            }
        }
        OnLobbyJoined.Invoke();
    }


    public async void CreatLobbyAsync() {
        bool result = await CreateLobby();
        if(!result) {
            //Invoke error message
        }
    }

    public static async Task<bool> CreateLobby() {
        try {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync();
            if (!createLobbyOutput.HasValue) {
                Debug.Log("Lobby created but not correctly instantiated.");
                return false;
            }
            currentLobby = createLobbyOutput.Value;

            currentLobby.SetPublic();
            //currentLobby.SetPrivate();
            currentLobby.SetJoinable(true);

            return true;
        }
        catch(System.Exception exception) {
            Debug.Log("Failed to create multiplayer lobby : " + exception);
            return false;
        }
    }

    public void LeaveLobby() {
        try {
            userInLobby = false;
            currentLobby.Leave();
            OnLobbyLeave.Invoke();
            foreach (var user in inLobby.Values) {
                Destroy(user);
            }
            inLobby.Clear();
        }
        catch (System.Exception) {
            
        }
    }

}

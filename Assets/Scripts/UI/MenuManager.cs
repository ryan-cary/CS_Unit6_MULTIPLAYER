using UnityEngine;

using UnityEngine.UI;
using TMPro;

using Unity.Netcode;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject lobbyPanel;

    [Header("Lobby UI")]
    public TMP_Text joinCodeText;
    public TMP_Text playerListText;
    public Button startMatchButton;
    public Button readyButton;
    public TMP_Text readyListText;
    public TMP_Dropdown arenaSelection;

    [Header("Join UI")]
    public TMP_InputField joinCodeInputField;


    private SessionManager sessions;

    private void Start()
    {
        sessions = SessionManager.Instance;
        sessions.OnLobbyUpdated += RefreshLobby;

        RefreshLobby();
    }

    private void OnDestroy()
    {
        if (sessions != null)
            sessions.OnLobbyUpdated -= RefreshLobby;
    }

    public void Host()
    {
        sessions.CreateLobby();
    }

    public void Join()
    {        
        sessions.JoinLobby(joinCodeInputField.text);
    }

    public void Leave()
    {
        sessions.LeaveLobby();
    }

    public void StartMatch()
    {
        
        sessions.StartMatch(arenaSelection.options[arenaSelection.value].text);
    }

    private void RefreshLobby()
    {
        var session = sessions.CurrentSession;

        bool inLobby = session != null;

        mainMenuPanel.SetActive(!inLobby);
        lobbyPanel.SetActive(inLobby);

        if (!inLobby)
            return;

        joinCodeText.text = $"{session.Code}";
        playerListText.text = "";
        readyListText.text = "";

        foreach (var player in session.Players)
        {
            bool isHost = player.Id == session.Host;

            playerListText.text += player.Id + (isHost ? " (Host)" : "") + "\n";

            // ================ READY STATUS IMPLEMENTATION ====================
            readyListText.text += (sessions.GetPlayerReady(player) ? "Ready" : "Not Ready") + "\n";
            // ================ READY STATUS IMPLEMENTATION ====================
        }

        startMatchButton.gameObject.SetActive(NetworkManager.Singleton.IsHost);

        // ================ READY STATUS IMPLEMENTATION ====================        
        startMatchButton.interactable = sessions.AllPlayersReady();
        // ================ READY STATUS IMPLEMENTATION ====================
    }

    // ================ READY STATUS IMPLEMENTATION ====================

    public void Ready()
    {    
        var session = sessions.CurrentSession;

        if (session == null)
            return;

        bool currentlyReady =sessions.GetPlayerReady(session.CurrentPlayer);
        sessions.SetReady(!currentlyReady);
    }

    // ================ READY STATUS IMPLEMENTATION ====================

}

using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    public enum State { CONNECTING, WAITING_PLAYERS, LOBBY_SELECTION, JOINED_GAME }
    public Multiplayer multiplayer;
    public GameObject WaitingPlayersText;
    public GameObject JoinLobbyButton;
    public GameObject ConnectingText;
    public GameObject LobbyInput;
    public GameObject JoinButton;
    public GameObject ServerSelector;

    public void setState(State state)
    {
        WaitingPlayersText.SetActive(state == State.WAITING_PLAYERS);
        JoinLobbyButton.SetActive(state == State.WAITING_PLAYERS);
        ConnectingText.SetActive(state == State.CONNECTING);
        LobbyInput.SetActive(state == State.LOBBY_SELECTION);
        JoinButton.SetActive(state == State.LOBBY_SELECTION);
        ServerSelector.SetActive(state != State.JOINED_GAME);
    }

    public void setLobbyId(int lobbyId)
    {
        Text t = WaitingPlayersText.GetComponent<Text>();
        t.text = t.text.Substring(0, t.text.LastIndexOf(' ') + 1) + lobbyId;
    }

    public int getLobbyId()
    {
        return int.Parse(LobbyInput.GetComponentsInChildren<Text>()[1].text);
    }

    public async void selectedServer()
    {
        int serverId = ServerSelector.GetComponent<Dropdown>().value;
        await multiplayer.switchServer(serverId);
    }
}

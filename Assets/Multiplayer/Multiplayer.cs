using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Multiplayer : MonoBehaviour
{
    public GameObject WaitingPlayersText;
    public GameObject JoinLobbyButton;
    public GameObject ConnectingText;
    public GameObject LobbyInput;
    public GameObject JoinButton;
    public GameObject YouWonText;
    public Rigidbody Player;
    public Rigidbody Enemy;

    public string ServerIp;
    ServerConnection serverConnection;

    bool gameStarted = false;

    // Start is called before the first frame update
    async Task Start()
    {
        Time.timeScale = 0;

        serverConnection = await Task.Run(() => new ServerConnection(ServerIp));
        serverConnection.send(new Packet(Packet.Type.CREATE_LOBBY, serverConnection.playerId));
        while (serverConnection.lobbyId == 0)
        {
            await Task.Delay(50);
        }

        WaitingPlayersText.GetComponent<Text>().text += serverConnection.lobbyId;
        WaitingPlayersText.SetActive(true);
        JoinLobbyButton.SetActive(true);
        ConnectingText.SetActive(false);


        while (!serverConnection.secondPlayerConnected)
        {
            await Task.Delay(50);
        }
        WaitingPlayersText.SetActive(false);
        JoinLobbyButton.SetActive(false);
        if (!gameStarted) gameStart();

    }

    public void joinOtherLobbyBtnClick()
    {
        WaitingPlayersText.SetActive(false);
        JoinLobbyButton.SetActive(false);
        LobbyInput.SetActive(true);
        JoinButton.SetActive(true);
    }

    public void joinBtnClick()
    {
        serverConnection.lobbyId = int.Parse(LobbyInput.GetComponentsInChildren<Text>()[1].text);
        serverConnection.send(new Packet(Packet.Type.LEFT, serverConnection.playerId));
        serverConnection.joinedLobby = false;
        serverConnection.send(new Packet(Packet.Type.JOIN, serverConnection.playerId, serverConnection.lobbyId));
        int startWait = Environment.TickCount;
        while (!serverConnection.joinedLobby && Environment.TickCount < startWait + 2000)
        {
            Thread.Sleep(50);
        }
        if (serverConnection.joinedLobby)
        {
            LobbyInput.SetActive(false);
            JoinButton.SetActive(false);
            if (!gameStarted) gameStart();
        }
    }

    void gameStart()
    {
        gameStarted = true;
        Time.timeScale = 1;
    }

    void Update(){
        if (serverConnection == null) return;
        if(serverConnection.won && YouWonText.activeSelf == false){
            YouWonText.SetActive(true);
        }
    }
    void FixedUpdate()
    {
        if (!gameStarted) return;

        if (Player != null)
        {
            byte[] positionBytes = new byte[4 * 4];
            BitConverter.GetBytes(Player.position.x).CopyTo(positionBytes, 0);
            BitConverter.GetBytes(Player.position.y).CopyTo(positionBytes, 4);
            BitConverter.GetBytes(Player.velocity.x).CopyTo(positionBytes, 8);
            BitConverter.GetBytes(Player.velocity.y).CopyTo(positionBytes, 12);
            Task.Run(() => serverConnection.send(new Packet(Packet.Type.MOVED, serverConnection.playerId, positionBytes)));
        }

        if (Enemy != null && serverConnection.newPosition)
        {
            serverConnection.newPosition = false;
            Enemy.position = new Vector3(serverConnection.x, serverConnection.y, 0);
            Enemy.velocity = new Vector3(serverConnection.vx, serverConnection.vy, 0);
        }
    }

    public void die(){
        serverConnection.send(new Packet(Packet.Type.LOST,serverConnection.playerId));
    }
}

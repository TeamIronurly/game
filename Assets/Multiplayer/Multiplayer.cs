using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Multiplayer : MonoBehaviour
{
    public MultiplayerUI multiplayerUI;
    public Rigidbody Player;
    public Rigidbody EnemyPrefab;
    public UnityEvent onWin;

    [Serializable]
    public struct ServerIpPort
    {
        public string ip;
        public int port;
    }
    public List<ServerIpPort> serverIPs;



    struct EnemyMove
    {
        public int id;
        public Vector3 p;
        public Vector3 v;
    }

    //queues for updating enemies in main thread
    Queue<EnemyMove> enemyMoves = new Queue<EnemyMove>();
    Queue<Action> runInUpdate = new Queue<Action>();

    Dictionary<int, Rigidbody> Enemies = new Dictionary<int, Rigidbody>();
    ServerConnection serverConnection;
    string serverIp;
    int serverPort;

    async Task Start()
    {
        serverIp = serverIPs[0].ip;
        serverPort = serverIPs[0].port;
        //pause game
        Time.timeScale = 0;
        //create server connection
        serverConnection = new ServerConnection();
        //set callbacks
        serverConnection.onWin = () =>
        {
            runInUpdate.Enqueue(onWin.Invoke);
        };
        serverConnection.onPlayerMove = enemyMoved;
        serverConnection.onPlayerJoined = enemyJoined;
        //connect to server
        multiplayerUI.setState(MultiplayerUI.State.CONNECTING);
        await serverConnection.connect(serverIp,serverPort);
        Debug.Log("connected to server");
        await serverConnection.login();
        Debug.Log("logged in");

        await createLobby();
    }

    public async Task createLobby()
    {
        //create lobby
        int lobbyId = await serverConnection.createLobby();
        Debug.Log("created lobby");
        //update UI
        multiplayerUI.setLobbyId(lobbyId);
        multiplayerUI.setState(MultiplayerUI.State.WAITING_PLAYERS);
        //wait for enemies
        while (Enemies.Count < 1)
        {
            await Task.Delay(50);
        }
        Debug.Log("stated game");
        //update UI
        multiplayerUI.setState(MultiplayerUI.State.JOINED_GAME);
        //start game
        Time.timeScale = 1;
    }

    public void joinOtherLobby()
    {
        //leave lobby
        serverConnection.leaveLobby();
        //update UI
        multiplayerUI.setState(MultiplayerUI.State.LOBBY_SELECTION);
    }

    public void joinBtnClick()
    {
        //join lobby
        int lobby = multiplayerUI.getLobbyId();
        bool joined = serverConnection.joinLobby(lobby);
        Debug.Log("joined lobby");
        if (joined)
        {
            Debug.Log("stated game");
            //update UI
            multiplayerUI.setState(MultiplayerUI.State.JOINED_GAME);
            //start game
            Time.timeScale = 1;
        }
    }

    public async Task switchServer(int serverId)
    {
        multiplayerUI.setState(MultiplayerUI.State.CONNECTING);
        serverIp = serverIPs[serverId].ip;
        serverPort = serverIPs[serverId].port;
        //disconnect
        serverConnection.disconnect();
        //wait for all network threads to stop
        await Task.Delay(100);
        //connect to server
        await serverConnection.connect(serverIp,serverPort);
        Debug.Log("connected to server");
        await serverConnection.login();
        Debug.Log("logged in");
        //create lobby
        await createLobby();
    }

    public void enemyJoined(int id)
    {
        runInUpdate.Enqueue(() =>
        {
            Enemies[id] = Instantiate(EnemyPrefab);
        });
    }
    public void enemyMoved(int id, Vector3 p, Vector3 v)
    {
        //same as enemyJoined but for moving enemies instead of instantiating
        EnemyMove update = new EnemyMove { id = id, p = p, v = v };
        enemyMoves.Enqueue(update);
    }
    public void lost()
    {
        //tell enemies that you lost
        serverConnection.sendLost();
    }


    void Update()
    {
        //run functions queued to be run in update
        while (runInUpdate.Count != 0)
        {
            runInUpdate.Dequeue().Invoke();
        }
    }
    void FixedUpdate()
    {
        //send player position to enemies
        if (Player != null)
        {
            serverConnection.sendPlayerPosition(Player.position, Player.velocity);
        }

        //move enemies enemies
        while (enemyMoves.Count != 0)
        {
            EnemyMove moved = enemyMoves.Dequeue();
            if (Enemies.ContainsKey(moved.id))
            {
                Enemies[moved.id].position = moved.p;
                Enemies[moved.id].velocity = moved.v;
            }
        }
    }

    void OnDestroy()
    {
        //disconnect from server
        serverConnection.disconnect();
        //resume game when goind back to the main menu
        Time.timeScale = 1;
    }
}

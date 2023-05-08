using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
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
    public Rigidbody EnemyPrefab;
    public string ServerIp;
    public UnityEvent onWin;


    struct EnemyMove
    {
        public int id;
        public Vector3 p;
        public Vector3 v;
    }

    //queues for updating enemies in main thread
    Queue<EnemyMove> enemyMoves = new Queue<EnemyMove>();
    Queue<int> enemyJoins = new Queue<int>();

    Dictionary<int, Rigidbody> Enemies = new Dictionary<int, Rigidbody>();
    ServerConnection serverConnection;

    async Task Start()
    {
        //pause game
        Time.timeScale = 0;
        //create server connection
        serverConnection = new ServerConnection();
        //set callbacks
        serverConnection.onWin = onWin.Invoke;
        serverConnection.onPlayerMove = enemyMoved;
        serverConnection.onPlayerJoined = enemyJoined;
        //connect to server
        await serverConnection.connect(ServerIp);
        Debug.Log("connected to server");
        await serverConnection.login();
        Debug.Log("loggen in");
        //create lobby
        int lobbyId = await serverConnection.createLobby();
        Debug.Log("created lobby");
        //update UI
        WaitingPlayersText.GetComponent<Text>().text += lobbyId;
        WaitingPlayersText.SetActive(true);
        JoinLobbyButton.SetActive(true);
        ConnectingText.SetActive(false);
        //wait for enemies
        while (Enemies.Count < 1)
        {
            await Task.Delay(50);
        }
        Debug.Log("stated game");
        //update UI
        WaitingPlayersText.SetActive(false);
        JoinLobbyButton.SetActive(false);
        //start game
        Time.timeScale = 1;
    }

    public void joinOtherLobbyBtnClick()
    {
        //leave lobby
        serverConnection.leaveLobby();
        //update UI
        WaitingPlayersText.SetActive(false);
        JoinLobbyButton.SetActive(false);
        LobbyInput.SetActive(true);
        JoinButton.SetActive(true);
    }

    public void joinBtnClick()
    {
        //join lobby
        int lobby = int.Parse(LobbyInput.GetComponentsInChildren<Text>()[1].text);
        bool joined = serverConnection.joinLobby(lobby);
        Debug.Log("joined lobby");
        if (joined)
        {
            Debug.Log("stated game");
            //update UI
            LobbyInput.SetActive(false);
            JoinButton.SetActive(false);
            //start game
            Time.timeScale = 1;
        }
    }

    public void enemyJoined(int id)
    {
        //adding new enemies to queue to be instantiated in Update
        //because instantiating from another thread does not work
        enemyJoins.Enqueue(id);
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
        //create enemies joined to the lobby from prefab
        while (enemyJoins.Count != 0)
        {
            int id = enemyJoins.Dequeue();
            Enemies[id] = Instantiate(EnemyPrefab);
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

    void OnDestroy(){
        //resume game when goind back to the main menu
        Time.timeScale = 1;
    }
}

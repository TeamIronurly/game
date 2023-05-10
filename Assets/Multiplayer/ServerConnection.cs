using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


class ServerConnection
{
    public Action onWin;
    public Action<int, Vector3, Vector3> onPlayerMove;
    public Action<int> onPlayerJoined;


    public int playerId = -1;
    Action pingCallback;
    Action<Packet> getIdCallback;
    Action<Packet> createdLobbyCallback;
    Action<Packet> playerJoinedCallback;
    Action joinFailedCallback;
    TcpClient tcpClient;
    UdpClient udp;
    NetworkStream tcp;
    public async Task<bool> connect(string ip, int port)
    {
        tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ip, port);
        tcp = tcpClient.GetStream();
        udp = new UdpClient(ip, port);
        startReceiveLoops();
        startPingLoop();
        return true;
    }

    public void disconnect(){
        tcpClient?.Close();
        udp?.Close();
    }

    public Task<bool> login()
    {
        var tcs = new TaskCompletionSource<bool>();
        getIdCallback = (Packet packet) =>
        {
            getIdCallback = null;
            playerId = BitConverter.ToInt32(packet.bytes, 8);
            tcs.SetResult(true);
        };
        send(new Packet(Packet.Type.GET_ID, 0));
        return tcs.Task;
    }

    public Task<int> ping()
    {
        int startTime = Environment.TickCount;
        var tcs = new TaskCompletionSource<int>();
        pingCallback = () =>
        {
            pingCallback = null;
            int stopTime = Environment.TickCount;
            tcs.SetResult(stopTime - startTime);
        };
        send(new Packet(Packet.Type.PING, playerId));
        return tcs.Task;
    }

    public Task<int> createLobby()
    {
        var tcs = new TaskCompletionSource<int>();
        createdLobbyCallback = (Packet packet) =>
        {
            createdLobbyCallback = null;
            int lobbyId = BitConverter.ToInt32(packet.bytes, 12);
            tcs.SetResult(lobbyId);
            startWaitingForPlayers();
        };
        send(new Packet(Packet.Type.CREATE_LOBBY, playerId));
        return tcs.Task;
    }

    public bool joinLobby(int lobbyId)
    {
        var tcs = new TaskCompletionSource<bool>();
        playerJoinedCallback = (Packet packet) =>
        {
            if (BitConverter.ToInt32(packet.bytes, 8) != playerId){
                onPlayerJoined.Invoke(BitConverter.ToInt32(packet.bytes, 8));
            };
            startWaitingForPlayers();
            joinFailedCallback = null;
            tcs.SetResult(true);
            
        };
        joinFailedCallback = () =>
        {
            playerJoinedCallback = null;
            joinFailedCallback = null;
            tcs.SetResult(false);
        };
        send(new Packet(Packet.Type.JOIN, playerId, lobbyId));
        return tcs.Task.Result;
    }

    public void leaveLobby()
    {
        send(new Packet(Packet.Type.LEFT, playerId));
    }

    public void sendPlayerPosition(Vector3 p, Vector3 v)
    {
        byte[] positionBytes = new byte[4 * 4];
        BitConverter.GetBytes(p.x).CopyTo(positionBytes, 0);
        BitConverter.GetBytes(p.y).CopyTo(positionBytes, 4);
        BitConverter.GetBytes(v.x).CopyTo(positionBytes, 8);
        BitConverter.GetBytes(v.y).CopyTo(positionBytes, 12);
        send(new Packet(Packet.Type.MOVED, playerId, positionBytes));
    }

    public void sendLost()
    {
        send(new Packet(Packet.Type.LOST, playerId));
    }
    void playerMovedCallback(Packet packet)
    {
        int id = BitConverter.ToInt32(packet.bytes, 8);
        float x = BitConverter.ToSingle(packet.bytes, 12);
        float y = BitConverter.ToSingle(packet.bytes, 16);
        float vx = BitConverter.ToSingle(packet.bytes, 20);
        float vy = BitConverter.ToSingle(packet.bytes, 24);
        Vector3 p = new Vector3(x, y, 0);
        Vector3 v = new Vector3(vx, vy, 0);
        onPlayerMove.Invoke(id, p, v);
    }

    void playerLostCallback(Packet packet)
    {
        int id = BitConverter.ToInt32(packet.bytes, 8);
        if (id != playerId)
        {
            onWin();
        }
    }
    void playerWonCallback(Packet packet)
    {
        int id = BitConverter.ToInt32(packet.bytes, 8);
        if (id == playerId)
        {
            onWin();
        }
    }

    void startWaitingForPlayers()
    {
        playerJoinedCallback = (Packet packet) =>
        {
            int id = BitConverter.ToInt32(packet.bytes, 8);
            if(id==playerId)return;
            onPlayerJoined.Invoke(id);
        };
    }

    public void startPingLoop()
    {
        new Thread(() =>
        {
            try{
            while (tcpClient.Connected)
            {
                new Thread(() =>
                {
                    int p = ping().Result;
                    //Debug.Log($"ping: {p}ms");
                }).Start();
                Thread.Sleep(100);
            }
            }catch(Exception e){
                tcpClient.Close();
                Console.Error.WriteLine(e);
            }
        }).Start();
    }
    public void send(Packet packet)
    {
        if(!tcpClient.Connected)return;
        if (packet.protocol == Packet.Protocol.TCP)
        {
            tcp.Write(packet.bytes);
        }
        else
        {
            udp.Send(packet.bytes, packet.bytes.Length);
        }
    }

    void receive(Packet packet)
    {
        switch (packet.type)
        {
            case Packet.Type.PING:
                pingCallback?.Invoke();
                break;
            case Packet.Type.GET_ID:
                getIdCallback?.Invoke(packet);
                break;
            case Packet.Type.WON:
                playerWonCallback(packet);
                break;
            case Packet.Type.LOST:
                playerLostCallback(packet);
                break;
            case Packet.Type.JOIN:
                //only for server
                break;
            case Packet.Type.JOIN_FAILED:
                joinFailedCallback?.Invoke();
                break;
            case Packet.Type.JOINED:
                Debug.Log($"player {BitConverter.ToInt32(packet.bytes, 8)} joined");
                playerJoinedCallback?.Invoke(packet);
                break;
            case Packet.Type.CREATE_LOBBY:
                //only for server
                break;
            case Packet.Type.CREATED_LOBBY:
                createdLobbyCallback?.Invoke(packet);
                break;
            case Packet.Type.LEFT:
                Debug.Log("player " + BitConverter.ToInt32(packet.bytes, 8) + " left");
                break;
            case Packet.Type.MOVED:
                playerMovedCallback(packet);
                break;
        }
    }

    void startReceiveLoops()
    {
        new Thread(TcpLoop).Start();
        new Thread(UdpLoop).Start();
    }

    void TcpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                byte[] lengthAndTypeBuffer = new byte[8];
                tcp.Read(lengthAndTypeBuffer, 0, 8);
                int length = BitConverter.ToInt32(lengthAndTypeBuffer, 0);
                Packet.Type type = (Packet.Type)BitConverter.ToInt32(lengthAndTypeBuffer, 4);

                byte[] bytes = new byte[Packet.Lengths[type]];
                lengthAndTypeBuffer.CopyTo(bytes, 0);
                int read = tcp.Read(bytes, 8, Packet.Lengths[type] - 8);
                if (read == 0)
                {
                    tcpClient.Close();
                    break;
                }

                receive(new Packet(Packet.Protocol.TCP, bytes));
            }
        }
        catch (Exception e)
        {
            tcpClient.Close();
            Console.Error.WriteLine(e);
        }
        tcpClient.Close();
        udp.Close();
    }
    void UdpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udp.Receive(ref ip);
                receive(new Packet(Packet.Protocol.UDP, bytes));
            }
        }
        catch (Exception e)
        {
            udp.Close();
            Console.Error.WriteLine(e);
        }
    }
}
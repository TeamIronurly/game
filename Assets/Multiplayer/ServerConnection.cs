using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UnityEngine;

class ServerConnection
{
    TcpClient tcpClient;
    UdpClient udp;
    NetworkStream tcp;


    public bool connected = false;
    public int playerId = 0;
    public bool joinedLobby = false;
    public int lobbyId = 0;
    public bool secondPlayerConnected = false;

    public float x = 0,y = 0,vx = 0,vy = 0;
    public bool newPosition = false;
    public bool won = false;
    public ServerConnection(string ip)
    {
        tcpClient = new TcpClient(ip, 42069);
        tcp = tcpClient.GetStream();
        udp = new UdpClient(ip, 42069);
        startReceiveLoops();

        while (playerId == 0)
        {
            send(new Packet(Packet.Type.PING, playerId));
            Thread.Sleep(50);
        }
        connected = true;
    }

    public void send(Packet packet)
    {
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
                if (connected == false)
                {
                    connected = true;
                    send(new Packet(Packet.Type.GET_ID, 0));
                }
                Debug.Log("PONG");
                break;
            case Packet.Type.GET_ID:
                Debug.Log("my id: " + BitConverter.ToInt32(packet.bytes, 4));
                playerId = BitConverter.ToInt32(packet.bytes, 4);
                break;
            case Packet.Type.WON:
                Debug.Log("player " + BitConverter.ToInt32(packet.bytes, 4) + " won");
                break;
            case Packet.Type.LOST:
                Debug.Log("player " + BitConverter.ToInt32(packet.bytes, 4) + " lost");
                won = true;
                break;
            case Packet.Type.JOIN:
                //only for server
                break;
            case Packet.Type.JOIN_FAILED:
                Debug.Log("lobby is full");
                break;
            case Packet.Type.JOINED:
                Debug.Log("player " + BitConverter.ToInt32(packet.bytes, 4) + " joined");
                if(BitConverter.ToInt32(packet.bytes, 4) != playerId){
                    secondPlayerConnected = true;
                }else{
                    joinedLobby = true;
                }
                break;
            case Packet.Type.CREATE_LOBBY:
                //only for server
                break;
            case Packet.Type.CREATED_LOBBY:
                Debug.Log("new lobby id: " + BitConverter.ToInt32(packet.bytes, 8));
                lobbyId = BitConverter.ToInt32(packet.bytes, 8);
                break;
            case Packet.Type.LEFT:
                Debug.Log("player " + BitConverter.ToInt32(packet.bytes, 4) + " left");
                break;
            case Packet.Type.MOVED:
                Debug.Log("player " + BitConverter.ToInt32(packet.bytes, 4) + " moved");
                x = BitConverter.ToSingle(packet.bytes, 8);
                y = BitConverter.ToSingle(packet.bytes, 12);
                vx = BitConverter.ToSingle(packet.bytes, 16);
                vy = BitConverter.ToSingle(packet.bytes, 20);
                newPosition = true;
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
                byte[] typeBuffer = new byte[4];
                tcp.Read(typeBuffer, 0, 4);
                Packet.Type type = (Packet.Type)BitConverter.ToInt32(typeBuffer);
                byte[] bytes = new byte[Packet.Lengths[type]];
                typeBuffer.CopyTo(bytes, 0);
                int read = tcp.Read(bytes, 4, Packet.Lengths[type] - 4);
                if (read != Packet.Lengths[type] - 4)
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
using System;
using System.Collections.Generic;

class Packet
{
    public enum Type { PING, GET_ID, WON, LOST, JOIN, JOINED, JOIN_FAILED, CREATE_LOBBY, CREATED_LOBBY, LEFT, MOVED };
    public enum Protocol { TCP, UDP };
    public static Dictionary<Type, int> Lengths = new Dictionary<Type, int>() {
        { Type.PING, 12 },               //type | player_id
        { Type.GET_ID, 12 },             //type | player_id
        { Type.WON, 12 },                //type | player_id
        { Type.LOST, 12 },               //type | player_id
        { Type.JOIN, 16 },              //type | player_id | lobby_id
        { Type.JOINED, 12 },             //type | player_id
        { Type.JOIN_FAILED, 12 },        //type | player_id
        { Type.CREATE_LOBBY, 12 },       //type | player_id
        { Type.CREATED_LOBBY, 16 },     //type | player_id | lobby_id
        { Type.LEFT, 12 },               //type | player_id
        { Type.MOVED, 28 },             //type | player_id | x | y | vx | vy
    };
    public Type type;
    public int length;
    public Protocol protocol;
    public byte[] bytes = new byte[0];
    public Packet(Type type)
    {
        this.type = type;
        length = 8;
        bytes = new byte[8];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player)
    {
        this.type = type;
        length = 12;
        bytes = new byte[12];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        BitConverter.GetBytes(player).CopyTo(bytes, 8);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player, int a)
    {
        this.type = type;
        length = 16;
        bytes = new byte[16];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        BitConverter.GetBytes(player).CopyTo(bytes, 8);
        BitConverter.GetBytes(a).CopyTo(bytes, 12);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player, byte[] extraBytes)
    {
        this.type = type;
        length = 12+extraBytes.Length;
        bytes = new byte[12+extraBytes.Length];
        BitConverter.GetBytes(length).CopyTo(bytes, 0);
        BitConverter.GetBytes((int)type).CopyTo(bytes, 4);
        BitConverter.GetBytes(player).CopyTo(bytes, 8);
        extraBytes.CopyTo(bytes, 12);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }

    public Packet(Protocol protocol, byte[] bytes)
    {
        this.bytes = bytes;
        this.protocol = protocol;
        type = (Type)BitConverter.ToInt32(bytes, 4);
    }
}

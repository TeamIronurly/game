using System;
using System.Collections;
using System.Collections.Generic;

class Packet
{
    public enum Type { PING, GET_ID, WON, LOST, JOIN, JOINED, JOIN_FAILED, CREATE_LOBBY, CREATED_LOBBY, LEFT, MOVED };
    public enum Protocol { TCP, UDP };
    public static Dictionary<Type, int> Lengths = new Dictionary<Type, int>() {
        { Type.PING, 8 },               //type | player_id
        { Type.GET_ID, 8 },             //type | player_id
        { Type.WON, 8 },                //type | player_id
        { Type.LOST, 8 },               //type | player_id
        { Type.JOIN, 12 },              //type | player_id | lobby_id
        { Type.JOINED, 8 },             //type | player_id
        { Type.JOIN_FAILED, 8 },        //type | player_id
        { Type.CREATE_LOBBY, 8 },       //type | player_id
        { Type.CREATED_LOBBY, 12 },     //type | player_id | lobby_id
        { Type.LEFT, 8 },               //type | player_id
        { Type.MOVED, 24 },             //type | player_id | x | y | vx | vy
    };
    public Type type;
    public Protocol protocol;
    public byte[] bytes = new byte[0];

    public Packet(Type type)
    {
        this.type = type;
        bytes = BitConverter.GetBytes((int)type);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player)
    {
        this.type = type;
        bytes = new byte[8];
        BitConverter.GetBytes((int)type).CopyTo(bytes, 0);
        BitConverter.GetBytes(player).CopyTo(bytes, 4);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player, int a)
    {
        this.type = type;
        bytes = new byte[12];
        BitConverter.GetBytes((int)type).CopyTo(bytes, 0);
        BitConverter.GetBytes(player).CopyTo(bytes, 4);
        BitConverter.GetBytes(a).CopyTo(bytes, 8);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }
    public Packet(Type type, int player, byte[] extraBytes)
    {
        this.type = type;
        bytes = new byte[8+extraBytes.Length];
        BitConverter.GetBytes((int)type).CopyTo(bytes, 0);
        BitConverter.GetBytes(player).CopyTo(bytes, 4);
        extraBytes.CopyTo(bytes, 8);
        protocol = (type == Type.MOVED||type==Type.PING) ? Protocol.UDP : Protocol.TCP;
    }

    public Packet(Protocol protocol, byte[] bytes)
    {
        this.bytes = bytes;
        this.protocol = protocol;
        type = (Type)BitConverter.ToInt32(bytes, 0);
    }
}

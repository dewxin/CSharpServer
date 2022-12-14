// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: ServerProtocols.proto

#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using CommonRpc.RpcBase;
using FlatSharp.Attributes;
using MessagePack;
using ProtoBuf;
using System.Collections.Generic;
using System.ComponentModel;

namespace Protocol.Param
{

    public enum ServerState
    {
        Off = 0,
        Idle = 1,
        Avail = 2,
        Busy = 3,
        Full = 4,
    }

    [MessagePackObject]
    public class ServerInfo
    {
        [Key(0)]
        public ushort ID { get; set; }
        [Key(1)]
        public string PeerType { get; set; }
        [Key(2)]
        public string IP { get; set; }
        [Key(3)]
        public int Port { get; set; }
        [Key(4)]
        public ServerState State { get; set; }
    }
    [MessagePackObject]
    public class ServerRegisterData
    {
        [Key(1)]
        public string PeerType { get; set; }
        [Key(2)]
        public string ServerIP { get; set; }
        [Key(3)]
        public int ServerPort { get; set; }
    }

    [MessagePackObject]
    public class RegisterServerRet : RpcResult<RegisterServerRet>
    {
        [Key(0)]
        public ServerInfo ServerInfo { get; set; }
        [Key(1)]
        public List<ServerInfo> ServerInfoList { get; set; }
    }

    [MessagePackObject]
    public class ServerListInfo : RpcResult<ServerListInfo>
    {
        [Key(0)]
        public List<ServerInfo> ServerInfoList { get; set; }
    }


    public enum EnumResultType:byte
    {
        UnknowError = 0,
        Success = 1,
        Fail = 2,
        KickPlayer = 3,
        Relogin = 4,
        ServerStop = 5,
        TimeOut = 6,
        NotTimeRange = 7,
        GameClosed = 8,
        PlayerFull = 9,
        CantLeave = 10,
        IsGaming = 11,
        CountLimit = 12,
    }


    [MessagePackObject]
    public class ServerConnectData
    {
        [Key(0)]
        public ushort ServerId {get;set;}

        [Key(1)]
        public string PeerType { get; set; }

    }

    [MessagePackObject]
    public class ServerConnectRet : RpcResult<ServerConnectRet>
    {
        [Key(1)]
        public ushort ServerId { get; set; }

        [Key(2)]
        public string PeerType { get; set; }
    }


    [MessagePackObject()]
    public class PacketPlayerDisconnect
    {
        [Key(2)]
        public uint GlobalSessionID { get; set; }
    }


    [MessagePackObject()]
    public class PlayerInfo
    {
        [Key(1)]
        public int PlayerId { get; set; }

        [Key(2)]
        public string NickName { get; set; } = string.Empty;

        [Key(3)]
        public string HeadIcon { get; set; } = string.Empty;

        [Key(4)]
        public long Gold { get; set; } = 0;
    }


}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006

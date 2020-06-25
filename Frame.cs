using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceGame
{
    public enum MessageType
    {
        Undefined,
        Connect,
        Exchange,
        Disconnect
    }

    public enum GameMessageType
    {
        Undefined,
        CreateGame,
        Connect,
        Send,
        Wait,
        GameOver
    }

    [Serializable]
    public class Frame
    {
        public MessageType MsgType;
        public GameMessageType GameMsgType;
        public int RoomId { get; }
        public object Data { get; }

        public Frame(int roomid, object data, MessageType messageType, GameMessageType gameMessageType)
        {
            RoomId = roomid;
            Data = data;
            MsgType = messageType;
            GameMsgType = gameMessageType;
        }

    }
}

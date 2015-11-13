using System;

namespace Direct3DHookLib.Interface
{
    [Serializable]
    public class MessageReceivedEventArgs : MarshalByRefObject
    {
        public MessageReceivedEventArgs(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
        }

        public MessageType MessageType { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}", MessageType, Message);
        }
    }
}
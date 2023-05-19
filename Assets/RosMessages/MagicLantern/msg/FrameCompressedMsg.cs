//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;

namespace RosMessageTypes.MagicLantern
{
    [Serializable]
    public class FrameCompressedMsg : Message
    {
        public const string k_RosMessageName = "magic_lantern_msgs/FrameCompressed";
        public override string RosMessageName => k_RosMessageName;

        //  This message contains a compressed image
        public HeaderMsg header;
        //  Header timestamp should be acquisition time of image
        //  Header frame_id should be optical frame of camera
        //  origin of frame should be optical center of camera
        //  +x should point to the right in the image
        //  +y should point down in the image
        //  +z should point into to plane of the image
        public string format;
        //  Specifies the format of the data
        //    Acceptable values:
        //      jpeg, png
        public byte[] data;
        //  Compressed image buffer

        public FrameCompressedMsg()
        {
            this.header = new HeaderMsg();
            this.format = "";
            this.data = new byte[0];
        }

        public FrameCompressedMsg(HeaderMsg header, string format, byte[] data)
        {
            this.header = header;
            this.format = format;
            this.data = data;
        }

        public static FrameCompressedMsg Deserialize(MessageDeserializer deserializer) => new FrameCompressedMsg(deserializer);

        private FrameCompressedMsg(MessageDeserializer deserializer)
        {
            this.header = HeaderMsg.Deserialize(deserializer);
            deserializer.Read(out this.format);
            deserializer.Read(out this.data, sizeof(byte), deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.format);
            serializer.WriteLength(this.data);
            serializer.Write(this.data);
        }

        public override string ToString()
        {
            return "FrameCompressedMsg: " +
            "\nheader: " + header.ToString() +
            "\nformat: " + format.ToString() +
            "\ndata: " + System.String.Join(", ", data.ToList());
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}

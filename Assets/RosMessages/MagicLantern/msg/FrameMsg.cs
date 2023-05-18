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
    public class FrameMsg : Message
    {
        public const string k_RosMessageName = "magic_lantern_msgs/Frame";
        public override string RosMessageName => k_RosMessageName;

        //  This message contains an uncompressed image
        //  (0, 0) is at top-left corner of image
        // 
        public HeaderMsg header;
        //  Header timestamp should be acquisition time of image
        //  Header frame_id should be optical frame of camera
        //  origin of frame should be optical center of camera
        //  +x should point to the right in the image
        //  +y should point down in the image
        //  +z should point into to plane of the image
        //  If the frame_id here and the frame_id of the CameraInfo
        //  message associated with the image conflict
        //  the behavior is undefined
        public uint height;
        //  image height, that is, number of rows
        public uint width;
        //  image width, that is, number of columns
        //  The legal values for encoding are in file src/image_encodings.cpp
        //  If you want to standardize a new string format, join
        //  ros-users@lists.sourceforge.net and send an email proposing a new encoding.
        public string encoding;
        //  Encoding of pixels -- channel meaning, ordering, size
        //  taken from the list of strings in include/sensor_msgs/image_encodings.h
        public byte is_bigendian;
        //  is this data bigendian?
        public uint step;
        //  Full row length in bytes
        public byte[] data;
        //  actual matrix data, size is (step * rows)

        public FrameMsg()
        {
            this.header = new HeaderMsg();
            this.height = 0;
            this.width = 0;
            this.encoding = "";
            this.is_bigendian = 0;
            this.step = 0;
            this.data = new byte[0];
        }

        public FrameMsg(HeaderMsg header, uint height, uint width, string encoding, byte is_bigendian, uint step, byte[] data)
        {
            this.header = header;
            this.height = height;
            this.width = width;
            this.encoding = encoding;
            this.is_bigendian = is_bigendian;
            this.step = step;
            this.data = data;
        }

        public static FrameMsg Deserialize(MessageDeserializer deserializer) => new FrameMsg(deserializer);

        private FrameMsg(MessageDeserializer deserializer)
        {
            this.header = HeaderMsg.Deserialize(deserializer);
            deserializer.Read(out this.height);
            deserializer.Read(out this.width);
            deserializer.Read(out this.encoding);
            deserializer.Read(out this.is_bigendian);
            deserializer.Read(out this.step);
            deserializer.Read(out this.data, sizeof(byte), deserializer.ReadLength());
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.height);
            serializer.Write(this.width);
            serializer.Write(this.encoding);
            serializer.Write(this.is_bigendian);
            serializer.Write(this.step);
            serializer.WriteLength(this.data);
            serializer.Write(this.data);
        }

        public override string ToString()
        {
            return "FrameMsg: " +
            "\nheader: " + header.ToString() +
            "\nheight: " + height.ToString() +
            "\nwidth: " + width.ToString() +
            "\nencoding: " + encoding.ToString() +
            "\nis_bigendian: " + is_bigendian.ToString() +
            "\nstep: " + step.ToString() +
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

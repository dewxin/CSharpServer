using FlatSharp;
using MessagePack;
using Microsoft.IO;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace CommonRpc.Net
{
    public class SerializerHelper
    {

        public static ArraySegment<byte> Serialize<T>(T t)
            where T : class 
        {
            if(t.GetType().IsAgressiveCompress())
            {
                var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                byte[] array =MessagePackSerializer.Serialize(t.GetType(), lz4Options);
                return new ArraySegment<byte>(array);
            }

            byte[] bytes = MessagePackSerializer.Serialize(t.GetType(),t);
            return new ArraySegment<byte>(bytes);

            ////Protobuf
            //if (t.GetType().IsAgressiveCompress())
            //{
            //    //TODO 考虑使用RecyclableMemoryStream
            //    //或者是pipelines
            //    //https://learn.microsoft.com/en-us/dotnet/standard/io/pipelines
            //    MemoryStream stream = new MemoryStream();
            //    ProtoBuf.Serializer.Serialize<T>(stream, t);
            //    stream.TryGetBuffer(out ArraySegment<byte> pbBuffer);
            //    return pbBuffer;
            //}


            ////FlatBuffer
            ////TODO  使用MsgPack， FlatBuffer生成的二进制数据太大了
            //int maxBytesNeeded = FlatBufferSerializer.Default.GetMaxSize(t);
            //byte[] buffer = new byte[maxBytesNeeded];
            //int bytesWritten = FlatBufferSerializer.Default.Serialize(t, buffer);
            //return new ArraySegment<byte>(buffer, 0, bytesWritten);
        }

        public static object Deserialize(Type type, byte[] content)
        {
            if (type.IsAgressiveCompress())
            {
                var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                return MessagePackSerializer.Deserialize(type, content, lz4Options);
            }

            return  MessagePackSerializer.Deserialize(type, content);

            ////ProtoBuf Parse
            //if(type.IsAgressiveCompress())
            //{
            //    using (MemoryStream ms = new MemoryStream(content))
            //    {
            //        object t = ProtoBuf.Serializer.Deserialize(type, ms);
            //        return t;
            //    }
            //}

            ////FlatBuffer 
            //return FlatBufferSerializer.Default.Compile(type).Parse(content);

        }

    }

}
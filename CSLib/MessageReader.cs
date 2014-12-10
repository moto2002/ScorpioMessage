﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
namespace Scorpio.Message
{
    public class MessageReader
    {
        MemoryStream stream;
        BinaryReader reader;
        public MessageReader(byte[] buffer)
        {
            stream = new MemoryStream(buffer);
            reader = new BinaryReader(stream);
        }
        public bool ReadBool()
        {
            return ReadInt8() == 1;
        }
        public sbyte ReadInt8()
        {
            return reader.ReadSByte();
        }
        public short ReadInt16()
        {
            return reader.ReadInt16();
        }
        public int ReadInt32()
        {
            return reader.ReadInt32();
        }
        public long ReadInt64()
        {
            return reader.ReadInt64();
        }
        public float ReadFloat()
        {
            return reader.ReadSingle();
        }
        public double ReadDouble()
        {
            return reader.ReadDouble();
        }
        public String ReadString()
        {
            List<byte> sb = new List<byte>();
            byte ch;
            while ((ch = reader.ReadByte()) != 0)
                sb.Add(ch);
            return Encoding.UTF8.GetString(sb.ToArray());
        }
        public byte[] ReadBytes()
        {
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }
        public void Close()
        {
            stream.Close();
            reader.Close();
        }
    }
}

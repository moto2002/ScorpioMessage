using System;
using System.Collections.Generic;
using System.Text;
using Scorpio;
namespace Scorpio.Message
{
    public class MessageSerializer
    {
        const string Index = "Index";       //索引
        const string Name = "Name";         //名字
        const string Type = "Type";         //数据类型
        const string Array = "Array";       //是否是数组

        const string BoolType = "bool";
        const string Int8Type = "int8";
        const string Int16Type = "int16";
        const string Int32Type = "int32";
        const string Int64Type = "int64";
        const string FloatType = "float";
        const string DoubleType = "double";
        const string StringType = "string";
        const string BytesType = "bytes";

        private static Script m_Script;
        public static void SetScript(Script script)
        {
            m_Script = script;
        }
        public static ScriptTable Deserialize(byte[] data, string name)
        {
            return Read(new MessageReader(data), name);
        }
        private static ScriptTable Read(MessageReader reader, string tableName)
        {
            ScriptTable table = m_Script.CreateTable();
            ScriptArray layout = (ScriptArray)m_Script.GetValue(tableName);
            int sign = reader.ReadInt32();
            for (int i = 0; i < layout.Count(); ++i) {
                ScriptObject config = layout.GetValue(i);
                if (MessageUtil.HasSign(sign, MessageUtil.ToInt32(config.GetValue(Index).ObjectValue))) {
                    string name = (string)config.GetValue(Name).ObjectValue;
                    string type = (string)config.GetValue(Type).ObjectValue;
                    bool array = (bool)config.GetValue(Array).ObjectValue;
                    if (array) {
                        int count = reader.ReadInt32();
                        ScriptArray arr = m_Script.CreateArray();
                        for (int j = 0; j < count;++j ) {
                            arr.Add(ReadObject(reader, type));
                        }
                        table.SetValue(name, arr);
                    } else {
                        table.SetValue(name, ReadObject(reader, type));
                    }
                }
            }
            return table;
        }
        private static ScriptObject ReadObject(MessageReader reader, string type)
        {
            object value = ReadField(reader, type);
            return value != null ? m_Script.CreateObject(value) : Read(reader, type);
        }
        private static object ReadField(MessageReader reader, string type)
        {
            if (type == BoolType) {
                return reader.ReadBool();
            } else if (type == Int8Type) {
                return reader.ReadInt8();
            } else if (type == Int16Type) {
                return reader.ReadInt16();
            } else if (type == Int32Type) {
                return reader.ReadInt32();
            } else if (type == Int64Type) {
                return reader.ReadInt64();
            } else if (type == FloatType) {
                return reader.ReadFloat();
            } else if (type == DoubleType) {
                return reader.ReadDouble();
            } else if (type == StringType) {
                return reader.ReadString();
            } else if (type == BytesType) {
                return reader.ReadBytes();
            }
            return null;
        }


        public static byte[] Serialize(ScriptTable table, string name)
        {
            MessageWriter write = new MessageWriter();
            Write(write, table, name);
            return write.ToArray();
        }
        private static void Write(MessageWriter writer, ScriptTable table, string tableName)
        {
            ScriptArray layout = (ScriptArray)m_Script.GetValue(tableName);
            int sign = 0;
            for (int i = 0; i < layout.Count(); ++i)
            {
                ScriptObject config = layout.GetValue(i);
                if (table.HasValue(config.GetValue(Name).ObjectValue))
                    sign = MessageUtil.AddSign(sign, MessageUtil.ToInt32(config.GetValue(Index).ObjectValue));
            }
            writer.WriteInt32(sign);
            for (int i = 0; i < layout.Count(); ++i)
            {
                ScriptObject config = layout.GetValue(i);
                string name = (string)config.GetValue(Name).ObjectValue;
                if (table.HasValue(name)) {
                    string type = (string)config.GetValue(Type).ObjectValue;
                    bool array = (bool)config.GetValue(Array).ObjectValue;
                    if (array) {
                        ScriptArray arr = table.GetValue(name) as ScriptArray;
                        writer.WriteInt32(arr.Count());
                        for (int j = 0; j < arr.Count(); ++j) {
                            WriteObject(writer, type, arr.GetValue(j));
                        }
                    } else {
                        WriteObject(writer, type, table.GetValue(name));
                    }
                }
            }
        }
        private static void WriteObject(MessageWriter writer, string type, ScriptObject value)
        {
            if (!WriteField(writer, type, value.ObjectValue))
                Write(writer, (ScriptTable)value, type);
        }
        private static bool WriteField(MessageWriter write, string type, object value)
        {
            if (type == BoolType) {
                write.WriteBool((bool)value);
            } else if (type == Int8Type) {
                write.WriteInt8(MessageUtil.ToInt8(value));
            } else if (type == Int16Type) {
                write.WriteInt16(MessageUtil.ToInt16(value));
            } else if (type == Int32Type) {
                write.WriteInt32(MessageUtil.ToInt32(value));
            } else if (type == Int64Type) {
                write.WriteInt64(MessageUtil.ToInt64(value));
            } else if (type == FloatType) {
                write.WriteFloat(MessageUtil.ToFloat(value));
            } else if (type == DoubleType) {
                write.WriteDouble(MessageUtil.ToDouble(value));
            } else if (type == StringType) {
                write.WriteString((string)value);
            } else if (type == BytesType) {
                write.WriteBytes((byte[])value);
            } else {
                return false;
            }
            return true;
        }
    }
}

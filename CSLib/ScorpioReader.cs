using System;
using System.Collections.Generic;
using System.Text;
using Scorpio;
namespace Scorpio.Message
{
    public interface IManager
    {
        ScriptArray GetArray(string name);
        Script GetScript();
    }
    public class ScorpioReader
    {
        private static IManager m_Manager;
        public static void SetManager(IManager manager)
        {
            m_Manager = manager;
        }
        private static bool HasSign(int sign, int index)
        {
            return (sign & (1 << index)) != 0;
        }
        public static ScriptTable Deserialize(byte[] data, string name)
        {
            return Read(new MessageReader(data), name);
        }
        private static object ReadField(MessageReader reader, string type)
        {
            if (type == "bool") {
                return reader.ReadBool();
            } else if (type == "int8") {
                return reader.ReadInt8();
            } else if (type == "int16") {
                return reader.ReadInt16();
            } else if (type == "int32") {
                return reader.ReadInt32();
            } else if (type == "int64") {
                return reader.ReadInt64();
            } else if (type == "float") {
                return reader.ReadFloat();
            } else if (type == "double") {
                return reader.ReadDouble();
            } else if (type == "string") {
                return reader.ReadString();
            } else if (type == "bytes") {
                return reader.ReadBytes();
            }
            return null;
        }
        private static ScriptObject ReadObject(MessageReader reader, string type)
        {
            object value = ReadField(reader, type);
            return value != null ? m_Manager.GetScript().CreateObject(value) : Read(reader, type);
        }
        private static ScriptTable Read(MessageReader reader, string tableName)
        {
            ScriptTable table = m_Manager.GetScript().CreateTable();
            ScriptArray layout = m_Manager.GetArray(tableName);
            int sign = reader.ReadInt32();
            for (int i = 0; i < layout.Count(); ++i)
            {
                if (HasSign(sign, i + 1))
                {
                    ScriptTable config = (ScriptTable)layout.GetValue(i);
                    string name = (string)config.GetValue("Name").ObjectValue;
                    string type = (string)config.GetValue("Name").ObjectValue;
                    bool array = (bool)config.GetValue("Array").ObjectValue;
                    if (array) {
                        int count = reader.ReadInt32();
                        ScriptArray arr = m_Manager.GetScript().CreateArray();
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
    }
}

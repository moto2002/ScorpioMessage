package Scorpio.Message;

import Scorpio.*;

public class MessageSerializer {
    private static final String Index = "Index"; //索引
    private static final String Name = "Name"; //名字
    private static final String Type = "Type"; //数据类型
    private static final String Array = "Array"; //是否是数组

    private static final String BoolType = "bool";
    private static final String Int8Type = "int8";
    private static final String Int16Type = "int16";
    private static final String Int32Type = "int32";
    private static final String Int64Type = "int64";
    private static final String FloatType = "float";
    private static final String DoubleType = "double";
    private static final String StringType = "string";
    private static final String BytesType = "bytes";

    private static Script m_Script;
    public static void SetScript(Script script) {
        m_Script = script;
    }
    public static ScriptTable Deserialize(byte[] data, String name) throws Exception {
        return Read(new MessageReader(data), name);
    }
    private static ScriptTable Read(MessageReader reader, String tableName) throws Exception {
        ScriptTable table = m_Script.CreateTable();
        ScriptArray layout = (ScriptArray)m_Script.GetValue(tableName);
        int sign = reader.ReadInt32();
        for (int i = 0; i < layout.Count(); ++i) {
            ScriptObject config = layout.GetValue(i);
            if (MessageUtil.HasSign(sign, MessageUtil.ToInt32(config.GetValue(Index).getObjectValue()))) {
                String name = (String)config.GetValue(Name).getObjectValue();
                String type = (String)config.GetValue(Type).getObjectValue();
                Boolean array = (Boolean)config.GetValue(Array).getObjectValue();
                if (array) {
                    int count = reader.ReadInt32();
                    ScriptArray arr = m_Script.CreateArray();
                    for (int j = 0; j < count;++j) {
                        arr.Add(ReadObject(reader, type));
                    }
                    table.SetValue(name, arr);
                }
                else {
                    table.SetValue(name, ReadObject(reader, type));
                }
            }
        }
        return table;
    }
    private static ScriptObject ReadObject(MessageReader reader, String type) throws Exception {
        Object value = ReadField(reader, type);
        return value != null ? m_Script.CreateObject(value) : Read(reader, type);
    }
    private static Object ReadField(MessageReader reader, String type) {
        if (BoolType.equals(type)) {
            return reader.ReadBool();
        }
        else if (Int8Type.equals(type)) {
            return reader.ReadInt8();
        }
        else if (Int16Type.equals(type)) {
            return reader.ReadInt16();
        }
        else if (Int32Type.equals(type)) {
            return reader.ReadInt32();
        }
        else if (Int64Type.equals(type)) {
            return reader.ReadInt64();
        }
        else if (FloatType.equals(type)) {
            return reader.ReadFloat();
        }
        else if (DoubleType.equals(type)) {
            return reader.ReadDouble();
        }
        else if (StringType.equals(type)) {
            return reader.ReadString();
        }
        else if (BytesType.equals(type)) {
            return reader.ReadBytes();
        }
        return null;
    }


    public static byte[] Serialize(ScriptTable table, String name) throws Exception {
        MessageWriter write = new MessageWriter();
        Write(write, table, name);
        return write.ToArray();
    }
    private static void Write(MessageWriter writer, ScriptTable table, String tableName) throws Exception {
        ScriptArray layout = (ScriptArray)m_Script.GetValue(tableName);
        int sign = 0;
        for (int i = 0; i < layout.Count(); ++i) {
            ScriptObject config = layout.GetValue(i);
            if (table.HasValue(config.GetValue(Name).getObjectValue())) {
                sign = MessageUtil.AddSign(sign, MessageUtil.ToInt32(config.GetValue(Index).getObjectValue()));
            }
        }
        writer.WriteInt32(sign);
        for (int i = 0; i < layout.Count(); ++i) {
            ScriptObject config = layout.GetValue(i);
            String name = (String)config.GetValue(Name).getObjectValue();
            if (table.HasValue(name)) {
                String type = (String)config.GetValue(Type).getObjectValue();
                Boolean array = (Boolean)config.GetValue(Array).getObjectValue();
                if (array) {
                    ScriptArray arr = (ScriptArray)table.GetValue(name);
                    writer.WriteInt32(arr.Count());
                    for (int j = 0; j < arr.Count(); ++j) {
                        WriteObject(writer, type, arr.GetValue(j));
                    }
                }
                else {
                    WriteObject(writer, type, table.GetValue(name));
                }
            }
        }
    }
    private static void WriteObject(MessageWriter writer, String type, ScriptObject value) throws Exception {
        if (!WriteField(writer, type, value.getObjectValue())) {
            Write(writer, (ScriptTable)value, type);
        }
    }
    private static boolean WriteField(MessageWriter write, String type, Object value) {
        if (BoolType.equals(type)) {
            write.WriteBool(((Boolean)value).booleanValue());
        }
        else if (Int8Type.equals(type)) {
            write.WriteInt8(MessageUtil.ToInt8(value));
        }
        else if (Int16Type.equals(type)) {
            write.WriteInt16(MessageUtil.ToInt16(value));
        }
        else if (Int32Type.equals(type)) {
            write.WriteInt32(MessageUtil.ToInt32(value));
        }
        else if (Int64Type.equals(type)) {
            write.WriteInt64(MessageUtil.ToInt64(value));
        }
        else if (FloatType.equals(type)) {
            write.WriteFloat(MessageUtil.ToFloat(value));
        }
        else if (DoubleType.equals(type)) {
            write.WriteDouble(MessageUtil.ToDouble(value));
        }
        else if (StringType.equals(type)) {
            write.WriteString((String)value);
        }
        else if (BytesType.equals(type)) {
            write.WriteBytes((byte[])value);
        }
        else {
            return false;
        }
        return true;
    }
}
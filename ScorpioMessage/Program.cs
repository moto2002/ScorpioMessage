using System;
using System.Collections.Generic;
using System.Text;
using Scorpio;
using System.IO;
namespace ScorpioMessage
{
    class BasicType
    {
        public int index;
        public string con;
        public string cs;
        public string java;
        public string write;
        public string read;
        public BasicType(int index,string con, string cs, string java, string write, string read)
        {
            this.index = index;
            this.con = con;
            this.cs = cs;
            this.java = java;
            this.write = write;
            this.read = read;
        }
    }
    class MessageField
    {
        public int index;
        public string name;
        public BasicType type;
        public string typeName;
        public bool list;
    }
    class Program
    {
        private static readonly List<BasicType> BasicTypes = new List<BasicType>()
        {
            new BasicType(0, "bool", "bool", "boolean", "WriteBool", "ReadBool") ,
            new BasicType(1, "int8", "sbyte", "byte", "WriteInt8", "ReadInt8") ,
            new BasicType(2, "int16", "short", "short", "WriteInt16", "ReadInt16") ,
            new BasicType(3, "int32", "int", "int", "WriteInt32", "ReadInt32") ,
            new BasicType(4, "int64", "long", "long", "WriteInt64", "ReadInt64") ,
            new BasicType(5, "float", "float", "float", "WriteFloat", "ReadFloat") ,
            new BasicType(6, "double", "double", "double", "WriteDouble", "ReadDouble") ,
            new BasicType(7, "string", "string", "String", "WriteString", "ReadString") ,
        };
        private static List<MessageField> Fields = new List<MessageField>();
        static BasicType GetType(object index)
        {
            foreach (var info in BasicTypes)
            {
                if ((index is string && info.con.Equals(index)) || (index is double && info.index.Equals(Convert.ToInt32(index))))
                    return info;
            }
            return null;
        }
        private static ScriptTable GlobalTable;
        static void Main(string[] args)
        {
            try {
                StringBuilder basicBuilder = new StringBuilder();
                foreach (var pair in BasicTypes) {
                    basicBuilder.AppendLine(pair.con + " = " + pair.index);
                }
                Script m_Script = new Script();
                m_Script.LoadLibrary();
                m_Script.LoadString(basicBuilder.ToString());
                string[] files = Directory.GetFiles(Environment.CurrentDirectory + "/Message", "*.sco", SearchOption.AllDirectories);
                foreach (var file in files) {
                    m_Script.LoadFile(file);
                }
                GlobalTable = m_Script.GetGlobalTable();
                var itor = GlobalTable.GetIterator();
                while (itor.MoveNext())
                {
                    string name = itor.Current.Key as string;
                    ScriptTable table = itor.Current.Value as ScriptTable;
                    if (name != null && table != null && name.StartsWith("Msg_"))
                    {
                        GenerateMessage(name, table, true);
                        GenerateMessage(name, table, false);
                    }
                }
                Console.WriteLine("生成完成");
            } catch (System.Exception ex) {
                Console.WriteLine("error : " + ex.ToString());
            }
            Console.ReadKey();
        }
        static void GenerateMessage(string className, ScriptTable table, bool cs)
        {
            try {
                Fields.Clear();
                int index = 1;
                var itor = table.GetIterator();
                while (itor.MoveNext()) {
                    string name = itor.Current.Key as string;
                    ScriptObject info = itor.Current.Value;
                    if (info is ScriptNumber || info is ScriptString)
                    {
                        BasicType type = GetType(info.ObjectValue);
                        Fields.Add(new MessageField() { index = index++, type = type, name = name, typeName = type != null ? (cs ? type.cs : type.java) : (string)info.ObjectValue, list = false });
                    }
                    else if (info is ScriptArray)
                    {
                        ScriptArray array = (ScriptArray)info;
                        BasicType type = GetType(array.GetValue(0).ObjectValue);
                        Fields.Add(new MessageField() { index = index++, type = type, name = name, typeName = type != null ? (cs ? type.cs : type.java) : (string)array.GetValue(0).ObjectValue, list = array.Count() >= 1 ? (bool)info.GetValue(1).ObjectValue : false });
                    }
                }
                StringBuilder builder = new StringBuilder();
                if (!cs)
                {
                    builder.AppendLine(@"package Message;");
                }
                builder.Append(@"
public class __ClassName {
");
                builder.Append(GenerateMessageFields(cs));
                builder.Append(GenerateMessageWrite());
                builder.Append(GenerateMessageRead());
                builder.Append(GenerateMessageSerialize());
                builder.Append(GenerateMessageDeserialize());
                builder.Append(@"}");
                builder.Replace("__ClassName", className);
                //File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/" + className + ".cs", builder.ToString(), Encoding.UTF8);
                if (cs)
                    File.WriteAllText(@"C:\Users\while\Desktop\TestMessage\" + className + ".cs", builder.ToString(), Encoding.UTF8);
                else
                    File.WriteAllText(@"C:\Users\while\Desktop\Prog\Scorpio-Java\trunk\src\Message\" + className + ".java", builder.ToString(), Encoding.UTF8);
            } catch (System.Exception ex) {
                throw new Exception("生成协议 " + className + " 出错 " + ex.ToString());
            }
        }
        static string GenerateMessageFields(bool cs)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"    private int __Sign = 0;
    private void AddSign(int index) {
        if ((__Sign & (1 << index)) == 0)
            __Sign |= (1 << index);
    }
    public __Bool HasSign(int index) {
        return (__Sign & (1 << index)) != 0;
    }");
            builder = builder.Replace("__Bool", cs ? "bool" : "boolean");
            for (int i=0;i<Fields.Count;++i) {
                var field = Fields[i];
                string str = "";
                if (field.list) {
                    str = @"    private __TypeName[] ___Name;
    public __TypeName[] get__Name() { return ___Name; }
    public void set__Name(__TypeName[] value) { ___Name = value; AddSign(__Index); } ";
                } else {
                    str = @"    private __TypeName ___Name;
    public __TypeName get__Name() { return ___Name; }
    public void set__Name(__TypeName value) { ___Name = value; AddSign(__Index); } ";
                }
                str = str.Replace("__TypeName", field.typeName);
                str = str.Replace("__Name", field.name);
                str = str.Replace("__Index", field.index.ToString());
                builder.AppendLine(str);
            }
            return builder.ToString();
        }
        static string GenerateMessageWrite()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"    public void Write(MessageWriter writer) {
        writer.WriteInt32(__Sign);");
            foreach (var field in Fields)
            {
                string str = "";
                if (field.list) {
                    str = @"        if (HasSign(__Index)) {
            writer.WriteInt32(MessageUtil.GetArrayLength(___Name));
            for (int i = 0;i < MessageUtil.GetArrayLength(___Name); ++i) {
                __FieldWrite
            }
        }";
                } else {
                    str = @"        if (HasSign(__Index)) {
            __FieldWrite
        }";
                }
                str = str.Replace("__FieldWrite", WriteFieldString(field, field.list ? "___Name[i]" : "___Name"));
                str = str.Replace("__Write", field.type != null ? field.type.write : "");
                str = str.Replace("__Index", field.index.ToString());
                str = str.Replace("__Name", field.name);
                builder.AppendLine(str);
            }
            builder.AppendLine(@"    }");
            return builder.ToString();
        }
        static string WriteFieldString(MessageField field, string name)
        {
            string str = "";
            if (field.type == null)
                str = "__Name.Write(writer);";
            else
                str = "writer.__Write(__Name);";
            return str.Replace("__Name", name);
        }
        static string GenerateMessageRead()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"    public static __ClassName Read(MessageReader reader) {
        __ClassName ret = new __ClassName();
        ret.__Sign = reader.ReadInt32();");
            foreach (var field in Fields)
            {
                string str = "";
                if (field.list) {
                    str = @"        if (ret.HasSign(__Index)) {
            ret.___Name = new __TypeName[reader.ReadInt32()];
            for (int i = 0;i < MessageUtil.GetArrayLength(ret.___Name); ++i) {
                ret.___Name[i] = __FieldRead;
            }
        }";
                } else {
                    str = @"        if (ret.HasSign(__Index)) {
            ret.___Name = __FieldRead;
        }";
                }
                str = str.Replace("__FieldRead", ReadFieldString(field));
                str = str.Replace("__Read", field.type != null ? field.type.read : "");
                str = str.Replace("__TypeName", field.typeName);
                str = str.Replace("__Index", field.index.ToString());
                str = str.Replace("__Name", field.name);
                builder.AppendLine(str);
            }
            builder.AppendLine(@"        return ret;
    }");
            return builder.ToString();
        }
        static string ReadFieldString(MessageField field)
        {
            string str = "";
            if (field.type == null)
                str = "__TypeName.Read(reader)";
            else
                str = "reader.__Read()";
            return str;
        }
        static string GenerateMessageSerialize()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"    public byte[] Serialize() {
        MessageWriter writer = new MessageWriter();
        Write(writer);
        return writer.ToArray();
    }");
            return builder.ToString();
        }
        static string GenerateMessageDeserialize()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"    public static __ClassName Deserialize(byte[] data) {
        return Read(new MessageReader(data));
    }");
            return builder.ToString();
        }
    }
}

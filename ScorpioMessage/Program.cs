using System;
using System.Collections.Generic;
using System.Text;
using Scorpio;
using System.IO;
namespace ScorpioMessage
{
    //基本类型
    public class BasicType
    {
        public string ScorpioName;          //脚本名称
        public string CSharpName;           //CS类型
        public string JavaName;             //JAVA类型
        public string WriteFunction;        //Write函数
        public string ReadFunction;         //Readh函数
        public BasicType(string con, string cs, string java, string write, string read)
        {
            this.ScorpioName = con;
            this.CSharpName = cs;
            this.JavaName = java;
            this.WriteFunction = write;
            this.ReadFunction = read;
        }
    }
    //变量类型
    public class MessageField
    {
        public int Index;           //变量索引
        public string Name;         //变量名字
        public BasicType Type;      //变量基本类型（如果是类则此值为null）
        public string TypeName;     //生成程序的类型
        public bool Array;          //是否是数组
    }
    class Program
    {
        private static readonly List<BasicType> BasicTypes = new List<BasicType>()
        {
            new BasicType( "bool", "bool", "boolean", "WriteBool", "ReadBool") ,
            new BasicType( "int8", "sbyte", "byte", "WriteInt8", "ReadInt8") ,
            new BasicType( "int16", "short", "short", "WriteInt16", "ReadInt16") ,
            new BasicType( "int32", "int", "int", "WriteInt32", "ReadInt32") ,
            new BasicType( "int64", "long", "long", "WriteInt64", "ReadInt64") ,
            new BasicType( "float", "float", "float", "WriteFloat", "ReadFloat") ,
            new BasicType( "double", "double", "double", "WriteDouble", "ReadDouble") ,
            new BasicType( "string", "string", "String", "WriteString", "ReadString") ,
            new BasicType( "bytes", "byte[]", "byte[]", "WriteBytes", "ReadBytes") ,
        };
        private static List<MessageField> Fields = new List<MessageField>();
        static BasicType GetType(string index)
        {
            foreach (var info in BasicTypes)
            {
                if (info.ScorpioName == index)
                    return info;
            }
            return null;
        }
        private static ScriptTable GlobalTable;
        private static string Package;
        private static string Path;
        private static string CSOut;
        private static string JavaOut;
        private static string ScoOut;
        static void Main(string[] args)
        {
            try {
                for (int i = 0; i < args.Length; ++i) {
                    if (args[i] == "-p") {
                        Package = args[i + 1];
                    } else if (args[i] == "-m") {
                        Path = args[i + 1];
                    } else if (args[i] == "-co") {
                        CSOut = args[i + 1];
                    } else if (args[i] == "-jo") {
                        JavaOut = args[i + 1];
                    } else if (args[i] == "-so") {
                        ScoOut = args[i + 1];
                    }
                }
            } catch (System.Exception ex) {
                Console.WriteLine("参数出错 -p [package] -m [sco配置目录] -co [cs生成目录] -jo [java生成目录] -so [Sco脚本生成目录] error : " + ex.ToString());
                goto exit;
            }
            if (string.IsNullOrEmpty(Package) || string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(CSOut) || string.IsNullOrEmpty(JavaOut) || string.IsNullOrEmpty(ScoOut)) {
                Console.WriteLine("参数出错 -p [package] -m [sco配置目录] -co [cs生成目录] -jo [java生成目录] -so [Sco脚本生成目录]");
                goto exit;
            }
            Console.WriteLine("Package = " + Package);
            Console.WriteLine("Path = " + System.IO.Path.Combine(Environment.CurrentDirectory, Path));
            Console.WriteLine("CSOut = " + System.IO.Path.Combine(Environment.CurrentDirectory, CSOut));
            Console.WriteLine("JavaOut = " + System.IO.Path.Combine(Environment.CurrentDirectory, JavaOut));
            Console.WriteLine("ScoOut = " + System.IO.Path.Combine(Environment.CurrentDirectory, ScoOut));
            try
            {
                Script m_Script = new Script();
                m_Script.LoadLibrary();
                List<ScriptObject> GlobalBasic = new List<ScriptObject>();
                {
                    GlobalTable = m_Script.GetGlobalTable();
                    var itor = GlobalTable.GetIterator();
                    while (itor.MoveNext()) {
                        GlobalBasic.Add(itor.Current.Value);
                    }
                }
                string[] files = Directory.GetFiles(System.IO.Path.Combine(Environment.CurrentDirectory, Path), "*.sco", SearchOption.AllDirectories);
                foreach (var file in files) {
                    m_Script.LoadFile(file);
                }
                {
                    GlobalTable = m_Script.GetGlobalTable();
                    var itor = GlobalTable.GetIterator();
                    while (itor.MoveNext()) {
                        if (GlobalBasic.Contains(itor.Current.Value)) continue;
                        string name = itor.Current.Key as string;
                        ScriptTable table = itor.Current.Value as ScriptTable;
                        if (name != null && table != null) {
                            GenerateMessage(name, table, true);
                            GenerateMessage(name, table, false);
                        }
                    }
                }
                Console.WriteLine("生成完成");
            } catch (System.Exception ex) {
                Console.WriteLine("error : " + ex.ToString());
            }
        exit:
            Console.WriteLine("按任意键继续");
            Console.ReadKey();
        }
        static void GenerateMessage(string className, ScriptTable table, bool cs)
        {
            try
            {
                Fields.Clear();
                int index = 1;
                var itor = table.GetIterator();
                while (itor.MoveNext())
                {
                    string name = itor.Current.Key as string;
                    ScriptObject info = itor.Current.Value;
                    if (info is ScriptNumber || info is ScriptString)
                    {
                        BasicType type = GetType(info.ObjectValue);
                        Fields.Add(new MessageField() { Index = index++, Type = type, Name = name, TypeName = type != null ? (cs ? type.CSharpName : type.JavaName) : (string)info.ObjectValue, Array = false });
                    }
                    else if (info is ScriptArray)
                    {
                        ScriptArray array = (ScriptArray)info;
                        BasicType type = GetType(array.GetValue(0).ObjectValue);
                        Fields.Add(new MessageField() { Index = index++, Type = type, Name = name, TypeName = type != null ? (cs ? type.CSharpName : type.JavaName) : (string)array.GetValue(0).ObjectValue, Array = array.Count() >= 1 ? (bool)info.GetValue(1).ObjectValue : false });
                    }
                }
                StringBuilder builder = new StringBuilder();
                if (!cs)
                {
                    builder.AppendLine(@"package __Package;
import Scorpio.Message.*;");
                }
                else
                {
                    builder.AppendLine(@"using Scorpio.Message;
namespace __Package {");
                }
                builder.Append(@"public class __ClassName {
");
                builder.Append(GenerateMessageFields(cs));
                builder.Append(GenerateMessageWrite());
                builder.Append(GenerateMessageRead());
                builder.Append(GenerateMessageSerialize());
                builder.Append(GenerateMessageDeserialize());
                builder.Append(@"}");
                if (cs)
                    builder.Append("}");
                builder.Replace("__ClassName", className);
                builder.Replace("__Package", Package);
                string csdir = System.IO.Path.Combine(Environment.CurrentDirectory, CSOut);
                {
                    if (!Directory.Exists(csdir))
                        Directory.CreateDirectory(csdir);
                }
                string javadir = System.IO.Path.Combine(Environment.CurrentDirectory, JavaOut);
                {
                    if (!Directory.Exists(javadir))
                        Directory.CreateDirectory(javadir);
                }

                if (cs)
                    File.WriteAllText(System.IO.Path.Combine(csdir, className + ".cs"), builder.ToString(), Encoding.UTF8);
                else
                    File.WriteAllText(System.IO.Path.Combine(javadir, className + ".java"), builder.ToString(), Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
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
            for (int i = 0; i < Fields.Count; ++i)
            {
                var field = Fields[i];
                string str = "";
                if (field.Array)
                {
                    str = @"    private __TypeName[] ___Name;
    public __TypeName[] get__Name() { return ___Name; }
    public void set__Name(__TypeName[] value) { ___Name = value; AddSign(__Index); } ";
                }
                else
                {
                    str = @"    private __TypeName ___Name;
    public __TypeName get__Name() { return ___Name; }
    public void set__Name(__TypeName value) { ___Name = value; AddSign(__Index); } ";
                }
                str = str.Replace("__TypeName", field.TypeName);
                str = str.Replace("__Name", field.Name);
                str = str.Replace("__Index", field.Index.ToString());
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
                if (field.Array)
                {
                    str = @"        if (HasSign(__Index)) {
            writer.WriteInt32(MessageUtil.GetArrayLength(___Name));
            for (int i = 0;i < MessageUtil.GetArrayLength(___Name); ++i) {
                __FieldWrite
            }
        }";
                }
                else
                {
                    str = @"        if (HasSign(__Index)) {
            __FieldWrite
        }";
                }
                str = str.Replace("__FieldWrite", WriteFieldString(field, field.Array ? "___Name[i]" : "___Name"));
                str = str.Replace("__Write", field.Type != null ? field.Type.WriteFunction : "");
                str = str.Replace("__Index", field.Index.ToString());
                str = str.Replace("__Name", field.Name);
                builder.AppendLine(str);
            }
            builder.AppendLine(@"    }");
            return builder.ToString();
        }
        static string WriteFieldString(MessageField field, string name)
        {
            string str = "";
            if (field.Type == null)
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
                if (field.Array)
                {
                    str = @"        if (ret.HasSign(__Index)) {
            ret.___Name = new __TypeName[reader.ReadInt32()];
            for (int i = 0;i < MessageUtil.GetArrayLength(ret.___Name); ++i) {
                ret.___Name[i] = __FieldRead;
            }
        }";
                }
                else
                {
                    str = @"        if (ret.HasSign(__Index)) {
            ret.___Name = __FieldRead;
        }";
                }
                str = str.Replace("__FieldRead", ReadFieldString(field));
                str = str.Replace("__Read", field.Type != null ? field.Type.ReadFunction : "");
                str = str.Replace("__TypeName", field.TypeName);
                str = str.Replace("__Index", field.Index.ToString());
                str = str.Replace("__Name", field.Name);
                builder.AppendLine(str);
            }
            builder.AppendLine(@"        return ret;
    }");
            return builder.ToString();
        }
        static string ReadFieldString(MessageField field)
        {
            string str = "";
            if (field.Type == null)
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

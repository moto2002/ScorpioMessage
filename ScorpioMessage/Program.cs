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
        public string SourceType;   //原始类型
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
            Path = System.IO.Path.Combine(Environment.CurrentDirectory, Path);
            CSOut = System.IO.Path.Combine(Environment.CurrentDirectory, CSOut);
            JavaOut = System.IO.Path.Combine(Environment.CurrentDirectory, JavaOut);
            ScoOut = System.IO.Path.Combine(Environment.CurrentDirectory, ScoOut);
            Console.WriteLine("Package = " + Package);
            Console.WriteLine("Path = " + Path);
            Console.WriteLine("CSOut = " + CSOut);
            Console.WriteLine("JavaOut = " + JavaOut);
            Console.WriteLine("ScoOut = " + ScoOut);
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
                string[] files = Directory.GetFiles(Path, "*.sco", SearchOption.AllDirectories);
                foreach (var file in files) {
                    m_Script.LoadFile(file);
                }
                {
                    List<MessageField> Fields = new List<MessageField>();
                    GlobalTable = m_Script.GetGlobalTable();
                    var itor = GlobalTable.GetIterator();
                    while (itor.MoveNext()) {
                        if (GlobalBasic.Contains(itor.Current.Value)) continue;
                        string name = itor.Current.Key as string;
                        ScriptTable table = itor.Current.Value as ScriptTable;
                        if (name != null && table != null) {
                            GenerateCS.Generate(name, Package, CSOut, GetFields(name, table, Fields, true));
                            GenerateJava.Generate(name, Package, JavaOut, GetFields(name, table, Fields, false));
                            GenerateScorpio.Generate(name, Package, ScoOut, GetFields(name, table, Fields, true));
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
        static List<MessageField> GetFields(string tableName, ScriptTable table, List<MessageField> Fields, bool cs)
        {
            Fields.Clear();
            var itor = table.GetIterator();
            while (itor.MoveNext())
            {
                string name = itor.Current.Key as string;
                ScriptString val = itor.Current.Value as ScriptString;
                if (string.IsNullOrEmpty(name) || val == null)
                    throw new Exception(string.Format("Message:{0} Field:{1} 参数出错 参数模版 \"索引,类型,是否数组=false\"", tableName, name));
                string[] infos = val.Value.Split(',');
                if (infos.Length < 2)
                    throw new Exception(string.Format("Message:{0} Field:{1} 参数出错 参数模版 \"索引,类型,是否数组=false\"", tableName, name));
                bool array = infos.Length > 2 && infos[2] == "true";
                BasicType type = GetType(infos[1]);
                Fields.Add(new MessageField() { 
                    Index = int.Parse(infos[0]), 
                    SourceType = infos[1],
                    Type = type, 
                    Name = name, 
                    TypeName = type != null ? (cs ? type.CSharpName : type.JavaName) : infos[1], 
                    Array = array });
            }
            Fields.Sort((m1, m2) => {
                    return m1.Index.CompareTo(m2.Index);
                });
            return Fields;
        }
    }
}

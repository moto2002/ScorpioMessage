using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ScorpioMessage
{
    public static class GenerateJava
    {
        private static List<MessageField> Fields;
        public static void Generate(string className, string Package, string outPath, List<MessageField> fields)
        {
            Fields = fields;
            StringBuilder builder = new StringBuilder();
            builder.Append(@"package __Package;
import Scorpio.Message.*;
    public class __ClassName extends IMessage {");
            builder.Append(GenerateMessageFields());
            builder.Append(GenerateMessageWrite());
            builder.Append(GenerateMessageRead());
            builder.Append(GenerateMessageDeserialize());
            builder.Append(@"
    }");
            builder.Replace("__ClassName", className);
            builder.Replace("__Package", Package);
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
            File.WriteAllBytes(System.IO.Path.Combine(outPath, className + ".java"), Encoding.UTF8.GetBytes(builder.ToString()));
        }
        static string GenerateMessageFields()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var field in Fields)
            {
                string str = "";
                if (field.Array)
                {
                    str = @"
        private __TypeName[] ___Name;
        public __TypeName[] get__Name() { return ___Name; }
        public void set__Name(__TypeName[] value) { ___Name = value; AddSign(__Index); } ";
                }
                else
                {
                    str = @"
        private __TypeName ___Name;
        public __TypeName get__Name() { return ___Name; }
        public void set__Name(__TypeName value) { ___Name = value; AddSign(__Index); } ";
                }
                str = str.Replace("__TypeName", field.TypeName);
                str = str.Replace("__Name", field.Name);
                str = str.Replace("__Index", field.Index.ToString());
                builder.Append(str);
            }
            return builder.ToString();
        }
        static string GenerateMessageWrite()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@"
        public override void Write(MessageWriter writer) {
            writer.WriteInt32(__Sign);");
            foreach (var field in Fields)
            {
                string str = "";
                if (field.Array)
                {
                    str = @"
            if (HasSign(__Index)) {
                writer.WriteInt32(___Name.length);
                for (int i = 0;i < ___Name.length; ++i) { __FieldWrite; }
            }";
                }
                else
                {
                    str = @"
            if (HasSign(__Index)) { __FieldWrite; }";
                }
                string write = "";
                if (field.Array) {
                    write = field.Type == null ? "___Name[i].Write(writer)" : "writer.__Write(___Name[i])";
                } else {
                    write = field.Type == null ? "___Name.Write(writer)" : "writer.__Write(___Name)";
                }
                str = str.Replace("__FieldWrite", write);
                str = str.Replace("__Write", field.Type != null ? field.Type.WriteFunction : "");
                str = str.Replace("__Index", field.Index.ToString());
                str = str.Replace("__Name", field.Name);
                builder.Append(str);
            }
            builder.Append(@"
        }");
            return builder.ToString();
        }
        static string GenerateMessageRead()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@"
        public static __ClassName Read(MessageReader reader) {
            __ClassName ret = new __ClassName();
            ret.__Sign = reader.ReadInt32();");
            foreach (var field in Fields)
            {
                string str = "";
                if (field.Array)
                {
                    str = @" 
            if (ret.HasSign(__Index)) {
                ret.___Name = new __TypeName[reader.ReadInt32()];
                for (int i = 0;i <ret.___Name.length; ++i) { ret.___Name[i] = __FieldRead; }
            }";
                }
                else
                {
                    str = @"  
            if (ret.HasSign(__Index)) { ret.___Name = __FieldRead; }";
                }
                str = str.Replace("__FieldRead", field.Type == null ? "__TypeName.Read(reader)" : "reader.__Read()");
                str = str.Replace("__Read", field.Type != null ? field.Type.ReadFunction : "");
                str = str.Replace("__TypeName", field.TypeName);
                str = str.Replace("__Index", field.Index.ToString());
                str = str.Replace("__Name", field.Name);
                builder.Append(str);
            }
            builder.Append(@"
            return ret;
        }");
            return builder.ToString();
        }
        static string GenerateMessageDeserialize()
        {
            return @"
        public static __ClassName Deserialize(byte[] data) {
            return Read(new MessageReader(data));
        }";
        }
    }
}

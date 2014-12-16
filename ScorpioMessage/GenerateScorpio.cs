using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ScorpioMessage
{
    public static class GenerateScorpio
    {
        public static void Generate(string className, string Package, string outPath, List<MessageField> fields)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@"__ClassName = [");
            foreach (var field in fields)
            {
                string str = @"
    { Name = ""__Name"", Type = ""__Type"", Array = __Array },";
                str = str.Replace("__Name", field.Name);
                str = str.Replace("__Type", field.SourceType);
                str = str.Replace("__Array", field.Array ? "true" : "false");
                builder.Append(str);
            }
            builder.Append(@"
]");
            builder = builder.Replace("__ClassName", className);
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
            File.WriteAllText(System.IO.Path.Combine(outPath, className + ".sco"), builder.ToString(), Encoding.UTF8);
        }
    }
}

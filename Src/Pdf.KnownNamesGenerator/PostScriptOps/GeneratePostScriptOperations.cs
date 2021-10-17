using System.Text;

namespace Pdf.KnownNamesGenerator.PostScriptOps
{
    public class GeneratePostScriptOperations
    {
        private static (string name, string code )[] operations = new (string name, string code)[]
        {
            ("Abs", "s.Push(Math.Abs(s.Pop()));"),
            ("Add", "s.Push(s.Pop()+s.Pop());"),
            ("Atan", "var den = s.Pop();var num = s.Pop(); s.Push(180.0 * Math.Atan2(num, den)/Math.PI);"),
            ("Mul", "s.Push(s.Pop()*s.Pop());"),
        };

        public static string ClassText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter");
            sb.AppendLine("{");
            sb.AppendLine("    public static partial class PostScriptOperations");
            sb.AppendLine("    {");
            GenerateVariables(sb);
            sb.AppendLine("    ");
            GenerateClasses(sb);
            sb.AppendLine("    ");
            sb.AppendLine("    }");
            sb.AppendLine("    public static partial class PostScriptOperationsDict");
            sb.AppendLine("    {");
            GenerateDictionary(sb);
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void GenerateDictionary(StringBuilder sb)
        {
            sb.AppendLine("      public static IReadOnlyDictionary<uint, IPostScriptOperation> CreateDict()");
            sb.AppendLine("      {");
            sb.AppendLine("          var ret = new Dictionary<uint, IPostScriptOperation>();");
            foreach (var (name, _) in operations)
            {
                sb.AppendLine($"          AddPdfOperation(ret, \"{name}\", PostScriptOperations.{name});");
            }
            sb.AppendLine("          return ret;");
            sb.AppendLine("      }");
        }

        private static void GenerateClasses(StringBuilder? sb)
        {
            foreach (var (name, code) in operations)
            {
                sb.AppendLine($"      private sealed class Operation{name}:IPostScriptOperation {{ public void Do(Stack<double> s) {{ {code} }} }} ");
            }
        }
        private static void GenerateVariables(StringBuilder? sb)
        {
            foreach (var (name, code) in operations)
            {
                sb.AppendLine($"      public static readonly IPostScriptOperation {name} = new Operation{name}(); ");
            }
        }
    }
}
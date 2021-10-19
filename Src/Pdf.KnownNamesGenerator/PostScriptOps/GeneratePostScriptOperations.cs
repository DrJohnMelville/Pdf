using System;
using System.Text;

namespace Pdf.KnownNamesGenerator.PostScriptOps
{
    public class GeneratePostScriptOperations
    {
        private static (string name, string code )[] operations = new (string name, string code)[]
        {
            // sentinels
            ("Open_Brace", "throw new NotSupportedException();"),
            ("Close_Brace", "throw new NotSupportedException();"),
            ("If", "throw new NotSupportedException();"),
            ("IfElse", ""),
            // arithmetic operators
            ("Abs", "s.Push(Math.Abs(s.Pop()));"),
            ("Add", "s.Push(s.Pop()+s.Pop());"),
            ("Atan", "var den = s.Pop();var num = s.Pop(); s.Push(CanonicalDegrees(RadToDeg( Math.Atan2(num, den))));"),
            ("Ceiling", "s.Push(Math.Ceiling(s.Pop()));"),
            ("Cos", "s.Push(Math.Cos(DegToRad(s.Pop())));"),
            ("Cvi", "s.Push((int)s.Pop());"),
            ("Cvr", ""),
            ("Div", "var den = s.Pop();var num = s.Pop(); s.Push(num/den);"),
            ("Exp", "var exp = s.Pop();var @base = s.Pop(); s.Push(Math.Pow(@base, exp));"),
            ("Floor", "s.Push(Math.Floor(s.Pop()));"),
            ("Idiv", "var den = (long)s.Pop();var num = (long)s.Pop(); s.Push(num/den);"),
            ("ln", "s.Push(Math.Log(s.Pop()));"),
            ("log", "s.Push(Math.Log10(s.Pop()));"),
            ("Mod", "var den = (long)s.Pop();var num = (long)s.Pop(); s.Push(num%den);"),
            ("Mul", "s.Push(s.Pop()*s.Pop());"),
            ("Neg", "s.Push(-s.Pop());"),
            ("Round", "s.Push(PostscriptRound(s.Pop()));"),
            ("Sin", "s.Push(Math.Sin(DegToRad(s.Pop())));"),
            ("Sqrt", "s.Push(Math.Sqrt(s.Pop()));"),
            ("Sub", "var den = s.Pop();var num = s.Pop(); s.Push(num - den);"),
            ("Truncate", "s.Push(Math.Truncate(s.Pop()));"),
            
            // relational boolean and butwise operators
            ("And", "var b = (long) s.Pop(); var a = (long)s.Pop(); s.Push(a&b);"),
            ("Or", "var b = (long) s.Pop(); var a = (long)s.Pop(); s.Push(a|b);"),
            ("Xor", "var b = (long) s.Pop(); var a = (long)s.Pop(); s.Push(a^b);"),
            ("Not", "s.Push(~(long)s.Pop());"),
            ("BitShift", "var b = (int) s.Pop(); var a = (long)s.Pop(); s.Push(PostscriptBitShift(a,b));"),
            ("Eq", "s.Push(PostscriptEqual(s.Pop(),s.Pop()));"),
            ("Ne", "s.Push(PostscriptNotEqual(s.Pop(),s.Pop()));"),
            ("True", "s.Push(-1.0);"),
            ("False", "s.Push(0.0);"),
            ("Ge", "var b = s.Pop(); var a = s.Pop(); s.Push(a>=b?-1:0);"),
            ("Gt", "var b = s.Pop(); var a = s.Pop(); s.Push(a> b?-1:0);"),
            ("Le", "var b = s.Pop(); var a = s.Pop(); s.Push(a<=b?-1:0);"),
            ("Lt", "var b = s.Pop(); var a = s.Pop(); s.Push(a< b?-1:0);"),
            
            // stack operators
            ("Copy", "PostscriptCopy(s);"),
            ("Dup", "s.Push(s.Peek());"),
            ("Exch", "s.Exchange();"),
            ("Index", "s.Push(s.Peek((int)s.Pop()));"),
            ("Pop", "s.Pop();"),
            ("Roll", "var delta= (int) s.Pop(); var count = (int)s.Pop(); RollSpan(s.AsSpan()[^count..], delta);"),
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

        private static void GenerateClasses(StringBuilder sb)
        {
            foreach (var (name, code) in operations)
            {
                sb.AppendLine($"      public sealed class Operation{name}:IPostScriptOperation {{ public void Do(PostscriptStack s) {{ {code} }} }} ");
            }
        }
        private static void GenerateVariables(StringBuilder sb)
        {
            foreach (var (name, _) in operations)
            {
                sb.AppendLine($"      public static readonly IPostScriptOperation {name} = new Operation{name}(); ");
            }
        }
    }
}
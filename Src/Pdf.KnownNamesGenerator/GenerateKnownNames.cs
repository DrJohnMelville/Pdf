using System;
using System.Linq;
using System.Text;

namespace Pdf.KnownNamesGenerator
{
    public static class GenerateKnownNames
    {
        public static string ClassText()
        {
            return @"
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model;
namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public static partial class KnownNames
    {
"+ AllDecls()+@"
    }

}";
        }

        private static string AllDecls()
        {
            var sb = new StringBuilder();
            GenerateNameConstants(sb);
            AddConstantsToDictionary(sb);
            return sb.ToString();
        }

        private static void GenerateNameConstants(StringBuilder sb)
        {
            foreach (var (value, name) in NameDictionary.AddAllNames)
            {
                sb.Append("        public static readonly PdfName ");
                sb.Append(name);
                sb.Append(" = new KnownPdfName(new byte[]{");
                foreach (var character in value)
                {
                    sb.Append(((byte) character).ToString());
                    sb.Append(", ");
                }

                sb.Append("}); //");
                sb.AppendLine(value);
            }
        }

        private static void AddConstantsToDictionary(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("        public static void AddItemsToDict(Dictionary<int, PdfName> dict) ");
            sb.AppendLine("        {");
            foreach (var (_, name) in NameDictionary.AddAllNames)
            {
                sb.Append("            AddTo(dict, ");
                sb.Append(name);
                sb.AppendLine(");");
            }
            sb.AppendLine("        }");
        }
    }
}
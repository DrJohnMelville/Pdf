using System.Text;
using System.Xml.Schema;

namespace Pdf.KnownNamesGenerator.KnownNames
{
    public static class GenerateKnownNames
    {
        public static string ClassText()
        {
            return @"
#nullable enable
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
namespace Melville.Pdf.LowLevel.Model.Conventions
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
                sb.Append("        private static readonly PdfName? _");
                sb.Append(name);
                sb.AppendLine(";");
                sb.Append("        public static readonly PdfName ");
                sb.Append(name);
                sb.Append(" = _");
                sb.Append(name);
                sb.Append(" ??= allKnownNames.DonateKey(");
                WriteStringAsByteArray(sb, value);
                sb.Append("); //");
                sb.AppendLine(value);
            }
        }

        private static void WriteStringAsByteArray(StringBuilder sb, string value)
        {
            sb.Append("new byte[]{");
            foreach (var character in value)
            {
                sb.Append(((byte)character).ToString());
                sb.Append(", ");
            }

            sb.Append("}");
        }

        private static void AddConstantsToDictionary(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("        public static void AddItemsToDict(NameDictionay dict) ");
            sb.AppendLine("        {");

            foreach (var (preferred, synonym) in NameDictionary.Synonyms)
            {
                sb.Append("            AddSynonym(dict, ");
                sb.Append(preferred);
                sb.Append(", ");
                WriteStringAsByteArray(sb, synonym);
                sb.Append("); //");
                sb.AppendLine(synonym);

            }
            sb.AppendLine("        }");
        }
    }
}
using System.Linq;
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
"+ AllDecls()+@"
    }

}";
        }

        private static string AllDecls()
        {
            var sb = new StringBuilder();
            GenerateNameConstants(sb);
            sb.Append(@"    public static partial class KnownNames
            {
                ");
            AddConstantsToDictionary(sb);
            return sb.ToString();
        }

        private static void GenerateNameConstants(StringBuilder sb)
        {
            foreach (var items in NameDictionary.AddAllNames.GroupBy(i => i.type))
            {
                sb.AppendLine($"      //{items.Key}Names");
                if (items.Key != "Pdf")
                {
                    sb.AppendLine(
                        $"      public class {items.Key}Name: PdfName {{ internal {items.Key}Name(byte[] name):base(name){{ }} ");
                    foreach (var (name, value,type) in items)
                    {
                        sb.AppendLine($"        public static {type}Name {name} => KnownNames.{name};");
                    }
                    sb.AppendLine(
                        "      }");
                }
                sb.AppendLine($"      public static partial class KnownNames {{");
                foreach (var (name, value, type) in items)
                {
                    sb.Append($"        public static readonly {type}Name ");
                    sb.Append(name);
                    sb.Append($" = NameDirectory.ForceAdd(new {type}Name(");
                    WriteStringAsByteArray(sb, value, "new");
                    sb.Append(")); //");
                    sb.AppendLine(value);
                    
                }

                sb.AppendLine("          }");
            }
        }

        private static void WriteStringAsByteArray(StringBuilder sb, string value, string creationMethod)
        {
            sb.Append(creationMethod+" byte[]{");
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
            sb.AppendLine("        public static void AddItemsToDict() ");
            sb.AppendLine("        {");

            foreach (var (preferred, synonym, type) in NameDictionary.Synonyms)
            {
                sb.Append("            NameDirectory.AddSynonym(");
                sb.Append("KnownNames.");
                sb.Append(preferred);
                sb.Append(", ");
                WriteStringAsByteArray(sb, synonym, "new");
                sb.Append("); //");
                sb.AppendLine(synonym);

            }
            sb.AppendLine("        }");
        }
    }
}
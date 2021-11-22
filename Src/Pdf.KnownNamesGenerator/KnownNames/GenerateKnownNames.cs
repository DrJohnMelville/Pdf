using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Differencing;

namespace Pdf.KnownNamesGenerator.KnownNames
{
    public  class GenerateKnownNames
    {
        private string data;

        public GenerateKnownNames(string data)
        {
            this.data = data;
        }

        public string ClassText()
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

        private string AllDecls()
        {
            var sb = new StringBuilder();
            GenerateNameConstants(sb);
            sb.Append(@"    public static partial class KnownNames
            {
                ");
            AddConstantsToDictionary(sb);
            return sb.ToString();
        }

        private static readonly Regex NameFinder = new(@"^\s*([^:\s]+)\:([^!\s]*)(?:\!([^\s]*))?",
            RegexOptions.Multiline);

        private (string Value, string CSharpName, string type)[] ReadNames() =>
            NameFinder.Matches(data).OfType<Match>()
                .Select(i => (
                    Value: TryString(i.Groups[3].Value, i.Groups[2].Value), 
                    CSharpName: i.Groups[2].Value,
                    i.Groups[1].Value)).ToArray();

        private string TryString(string value, string fallBack) =>
            String.IsNullOrWhiteSpace(value) ? fallBack : value;
        

        private void GenerateNameConstants(StringBuilder sb)
        {
            foreach (var items in ReadNames().GroupBy(i => i.type))
            {
                sb.AppendLine($"      //{items.Key}Names");
                if (items.Key != "Pdf")
                {
                    sb.AppendLine(
                        @$"      public readonly struct {items.Key}Name 
      {{
         private readonly PdfName name;
         internal {items.Key}Name(PdfName name){{ this.name = name;}}
         public static implicit operator PdfName({items.Key}Name wrapper) => wrapper.name; ");
                    foreach (var (value, name,type) in items)
                    {
                        sb.AppendLine($"        public static {type}Name {name} => new(KnownNames.{name});");
                    }
                    sb.AppendLine(
                        "      }");
                }
                sb.AppendLine($"      public static partial class KnownNames {{");
                foreach (var (value, name, type) in items)
                {
                    sb.Append($"        public static readonly PdfName ");
                    sb.Append(name);
                    sb.Append($" = NameDirectory.ForceAdd(new PdfName(");
                    ByteStreamWriter.WriteByteDecl(sb, value);
                }

                sb.AppendLine("          }");
            }
        }


        private void AddConstantsToDictionary(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("        public static void AddItemsToDict() ");
            sb.AppendLine("        {");

            foreach (var (preferred, synonym) in Synonyms())
            {
                sb.Append("            NameDirectory.AddSynonym(");
                sb.Append("KnownNames.");
                sb.Append(preferred);
                sb.Append(", ");
                ByteStreamWriter.WriteStringAsByteArray(sb, synonym, "new");
                sb.Append("); //");
                sb.AppendLine(synonym);

            }
            sb.AppendLine("        }");
        }

        private static readonly Regex synonymFinder = new Regex(
            @"^\s*([^\=\>\s]+)\=\>\s*([^\s]+)", RegexOptions.Multiline);
        private IEnumerable<(string, string)> Synonyms() =>
            synonymFinder.Matches(data).OfType<Match>()
                .Select(i => (i.Groups[2].Value, i.Groups[1].Value));
    }
}
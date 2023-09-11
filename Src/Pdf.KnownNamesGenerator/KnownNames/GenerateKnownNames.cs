using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pdf.KnownNamesGenerator.KnownNames
{
    //this is a simplified version of the FNV algorithm for short strings, it does not do any of the modulos
    public static class Fnv
    {
        private const uint offsetBasis = 0x811c9dc5;
        private const uint prime = 0x01000193;

        public static uint FromString(string s)
        {
            unchecked
            {
                var hash = offsetBasis;
                foreach (var character in s)
                {
                    hash = (hash * prime) ^ (uint)(character & 0xFF);
                }

                return hash;
                
            }
        }
    }
    public  class GenerateKnownNames
    {
        private readonly string data;

        public GenerateKnownNames(string data)
        {
            this.data = data;
        }

        public string ClassText()
        {
            return @"
#nullable enable
using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
namespace Melville.Pdf.LowLevel.Model.Conventions
{
"+ AllDecls()+@"
}";
        }

        private string AllDecls()
        {
            var sb = new StringBuilder();
            GenerateNameConstants(sb);
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

        private static string TryString(string value, string fallBack) =>
            string.IsNullOrWhiteSpace(value) ? fallBack : value;
        

        private void GenerateNameConstants(StringBuilder sb)
        {
            var allNames = ReadNames().ToList();
            RenderPdfNameSubsets(sb, allNames);
            RenderPdfNameDeclarations(sb, allNames);
        }

        private static void RenderPdfNameSubsets(StringBuilder sb, List<(string Value, string CSharpName, string type)> allNames)
        {
            foreach (var items in allNames.GroupBy(i => i.type))
            {
                RenderPdfNameGroup(sb, items);
            }
        }

        private static void RenderPdfNameDeclarations(StringBuilder sb, List<(string Value, string CSharpName, string type)> allNames)
        {
            sb.AppendLine("      /// <summary>");
            sb.AppendLine("      /// Precomputed FNV known names.");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      public static partial class KnownNames {");
            foreach (var (value, name, _) in UniquePdfNames(allNames))
            {
                RenderPdfNameCreation(sb, name, value);
            }

            sb.AppendLine("          }");
        }

        private static IOrderedEnumerable<(string Value, string CSharpName, string type)> UniquePdfNames(List<(string Value, string CSharpName, string type)> allNames) =>
            allNames
                .GroupBy(i => i.CSharpName).Select(i => i.First())
                .OrderBy(i=>i.CSharpName);

        private static void RenderPdfNameCreation(StringBuilder sb, string name, string value)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// PdfDirectObject for for ({value})");
            sb.AppendLine("        /// </summary>");
                CreateLongString(sb, name, value);
        }

        private static void CreateLongString(StringBuilder sb, string name, string value) =>
            sb.AppendLine(
                $"""        public static readonly PdfDirectObject {name} = PdfDirectObject.CreateName("{value}"u8);""");

        private static void RenderPdfNameGroup(StringBuilder sb, 
            IGrouping<string, (string Value, string CSharpName, string type)> items)
        {
            if (items.Key != "Pdf")
            {
                sb.AppendLine(
                    $$"""
                              /// <summary>
                              /// This struct allows the builder APIs to request a {{items.Key}} to suggest 
                              /// proper PdfNames for the parameter.
                              /// </summary>
                              public readonly struct {{items.Key}}Name 
                              {
                                 private readonly PdfDirectObject name;
                                 /// <summary>
                                 /// Implicitly convert a PdfDirectObject to a {{items.Key}}
                                 /// </summary>    
                                 public {{items.Key}}Name(PdfDirectObject name){ this.name = name;}
                                 /// <summary>
                                 /// Implicitly convert a {{items.Key}} to a PdfDirectObject
                                 /// </summary>    
                                 public static implicit operator PdfDirectObject({{items.Key}}Name wrapper) => wrapper.name; 
                        """);
                foreach (var (value, name, type) in items)
                {
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine($"        /// {type} value for {name}");
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine($"        public static {type}Name {name} => new(KnownNames.{name});");
                }

                sb.AppendLine(
                    "      }");
            }
        }
    }
}
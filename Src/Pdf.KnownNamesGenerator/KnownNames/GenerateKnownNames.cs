using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Pdf.KnownNamesGenerator.KnownNames
{
    //this is a simplified version of the FNV algorithm for short strings, it does not do any of the modulos
    public static class Fnv
    {
        private const uint offsetBasis = 0x811c9dc5;
        private const uint prime = 0x01000193;

        public static int FromString(string s)
        {
            unchecked
            {
                var hash = offsetBasis;
                foreach (var character in s)
                {
                    hash = (hash * prime) ^ (uint)(character & 0xFF);
                }

                return (int)hash;
                
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
using Melville.Pdf.LowLevel.Model.Objects2;
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
            RenderPdfNameKeyDeclarations(sb, allNames);
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
            sb.AppendLine("      /// Precomputed FNV hashes for the known names.");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      public static partial class KnownNames {");
            foreach (var (value, name, _) in UniquePdfNames(allNames))
            {
                RenderPdfNameCreation(sb, name, value);
            }

            sb.AppendLine("          }");
        }
        private static void RenderPdfNameKeyDeclarations(StringBuilder sb, List<(string Value, string CSharpName, string type)> allNames)
        {
            sb.AppendLine("      /// <summary>");
            sb.AppendLine($"      /// Precomputed keys for the known names");
            sb.AppendLine("      /// </summary>");
            sb.AppendLine("      public static partial class KnownNameKeys {");
            foreach (var (value, name, _) in UniquePdfNames(allNames))
            {
                var fnvValue = Fnv.FromString(value);
                sb.AppendLine("        /// <summary>");
                sb.AppendLine($"        /// PdfName ({value}) has FNV Hash of {fnvValue}");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine($"        public const int {name} = {fnvValue};");
                sb.AppendLine();
            }

            sb.AppendLine("          }");
        }

        private static bool TryPackNum(string value, out string result)
        {
            if (value.Length > 18)
            {
                result = "";
                return false;
            }
            var ret = new BigInteger();
            foreach (var character in value)
            {
                ret <<= 7;
                ret |= (character & 0x7F);
            }
            result =  ret.ToString();
            return true;
        }

        private static IOrderedEnumerable<(string Value, string CSharpName, string type)> UniquePdfNames(List<(string Value, string CSharpName, string type)> allNames) =>
            allNames
                .GroupBy(i => i.CSharpName).Select(i => i.First())
                .OrderBy(i=>i.CSharpName);

        private static void RenderPdfNameCreation(StringBuilder sb, string name, string value)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// u8 span for ({value})");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"        public static ReadOnlySpan<byte> {name}U8 => \"{value}\"u8;");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// PdfDirectValue for span for ({value})");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"""        public static PdfDirectValue {name}TName => PdfDirectValue.CreateName({name}U8);""");
            sb.AppendLine("        /// <summary>");
            #warning -- get rid of this
            // sb.AppendLine($"        /// PdfName with value: ({value})");
            // sb.AppendLine("        /// </summary>");
            // sb.AppendLine($"""        public static readonly PdfName {name} = NameDirectory.ForceAdd({name}U8);""");
        }

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
                                 private readonly PdfDirectValue name;
                                 /// <summary>
                                 /// Implicitly convert a PdfDirectValue to a {{items.Key}}
                                 /// </summary>    
                                 public {{items.Key}}Name(PdfDirectValue name){ this.name = name;}
                                 /// <summary>
                                 /// Implicitly convert a {{items.Key}} to a PdfDirectValue
                                 /// </summary>    
                                 public static implicit operator PdfDirectValue({{items.Key}}Name wrapper) => wrapper.name; 
                        """);
                foreach (var (value, name, type) in items)
                {
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine($"        /// {type} value for {name}");
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine($"        public static {type}Name {name} => new(KnownNames.{name}TName);");
                }

                sb.AppendLine(
                    "      }");
            }
        }
    }
}
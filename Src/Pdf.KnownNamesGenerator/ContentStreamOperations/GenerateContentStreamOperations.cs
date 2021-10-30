using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Pdf.KnownNamesGenerator.ContentStreamOperations
{
    public class GenerateContentStreamOperations
    {
        
        public static string ClassText()
        {
            var sb = new StringBuilder();
            ClassText(sb);
            return sb.ToString();
        }
        private static void ClassText(StringBuilder sb)
        {
            sb.AppendLine("namespace Melville.Pdf.LowLevel.Model.ContentStreams;");
            sb.AppendLine();
            sb.AppendLine("public static class ContentStreamOperatorNames");
            sb.AppendLine("{");
            foreach (var (op, name) in operators)
            {
                sb.Append($"    public static readonly byte[] {name} = ((");
                ByteStreamWriter.WriteByteDecl(sb, op);
            }
            sb.AppendLine("}");
            
            sb.AppendLine("public enum ContentStreamOperatorValue");
            sb.AppendLine("{");
            foreach (var (op, name) in operators)
            {
                sb.Append($"    {name} = 0x");
                foreach (var character in op)
                {
                    sb.Append(((byte)(character)).ToString("X2"));
                }

                sb.AppendLine($", // {op}");
            }
            sb.AppendLine("}");
        }

        private static (string Operator, string Name)[] operators = new[]
        {
            ("b", "b"),
            ("B", "B"),
            ("b*", "bStar"),
            ("B*", "BStar"),
            ("BDC", "BDC"),
            ("BI", "BI"),
            ("BMC", "BMC"),
            ("BT", "BT"),
            ("BX", "BX"),
            ("c", "c"),
            ("cm", "cm"),
            ("CS", "CS"),
            ("cs", "cs"),
            ("d", "d"),
            ("d0", "d0"),
            ("d1", "d1"),
            ("Do", "Do"),
            ("DP", "DP"),
            ("EI", "EI"),
            ("EMC", "EMC"),
            ("ET", "ET"),
            ("f", "f"),
            ("F", "F"),
            ("f*", "fStar"),
            ("G", "G"),
            ("g", "g"),
            ("gs", "gs"),
            ("h", "h"),
            ("i", "i"),
            ("ID", "ID"),
            ("j", "j"),
            ("J", "J"),
            ("K", "K"),
            ("k", "k"),
            ("l", "l"),
            ("m", "m"),
            ("M", "M"),
            ("MP", "MP"),
            ("n", "n"),
            ("q", "q"),
            ("Q", "Q"),
            ("re", "re"),
            ("RG", "RG"),
            ("rg", "rg"),
            ("ri", "ri"),
            ("s", "s"),
            ("S", "S"),
            ("SC", "SC"),
            ("SCN", "SCN"),
            ("sh", "sh"),
            ("T*", "TStar"),
            ("Tc", "Tc"),
            ("Td", "Td"),
            ("TD", "TD"),
            ("Td", "Tf"),
            ("Tj", "Tj"),
            ("TJ", "TJ"),
            ("TL", "TL"),
            ("Tm", "Tm"),
            ("Tr", "Tr"),
            ("Ts", "Ts"),
            ("Tw", "Tw"),
            ("Tz", "Tz"),
            ("v", "v"),
            ("w", "w"),
            ("W", "W"),
            ("W*", "WStar"),
            ("y", "y"),
            ("'", "SingleQuote"),
            ("\"", "DoubleQuote")
        };
    }
}
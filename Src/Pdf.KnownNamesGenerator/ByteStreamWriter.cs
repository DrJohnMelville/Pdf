using System.Text;

namespace Pdf.KnownNamesGenerator;

public static class ByteStreamWriter
{
    public static void WriteByteDecl(StringBuilder sb, string value)
    {
        WriteStringAsByteArray(sb, value, "new");
        sb.Append(")); //");
        sb.AppendLine(value);
    }

    public static void WriteStringAsByteArray(StringBuilder sb, string value, string creationMethod)
    {
        sb.Append(creationMethod + " byte[]{");
        foreach (var character in value)
        {
            sb.Append(((byte)character).ToString());
            sb.Append(", ");
        }

        sb.Append("}");
    }
}
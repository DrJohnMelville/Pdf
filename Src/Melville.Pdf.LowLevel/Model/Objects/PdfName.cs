using System.Text;
using System.Text.RegularExpressions;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects;

public class PdfName: PdfByteArrayObject
{

    public PdfName(byte[] name): base(name){}
    public PdfName(string s):this(Encoding.UTF8.GetBytes(s)){}
    public override string ToString() => "/"+Encoding.UTF8.GetString(Bytes);
    public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);

    public static implicit operator PdfName(string s) => NameDirectory.Get(s);

    public PdfName FilterTo(Regex excludedPattern) => 
        NameDirectory.Get(excludedPattern.Replace(Encoding.UTF8.GetString(Bytes), ""));
}
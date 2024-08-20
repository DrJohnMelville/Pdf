using System;
using System.Text;
using System.Threading.Tasks;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_3;

public readonly struct FileBuilder
{
    private readonly StringBuilder sb = new();

    public FileBuilder()
    {
    }

    public void Append(string s) => sb.Append(s);
    public void AppendLine(string s)
    {
        sb.Append(s);
        sb.Append('\n');
    }

    public int Position => sb.Length;
    public override string ToString() => sb.ToString();
}

public class S_7_3_10_IndirectObjectsDefined: IDisposable
{
    private IDisposable ctx = RentalPolicyChecker.RentalScope();
    public void Dispose() => ctx.Dispose();

    [Fact]
    public async Task ParseMinimalLowLevelFileAsync()
    {
        var builder = new FileBuilder();
       builder.AppendLine("%PDF-1.7");
       builder.AppendLine("%ÿÿÿÿ Created with Melville.Pdf");
       var objPos = builder.Position;
       builder.AppendLine("1 0 obj 10 endobj");
       var xrefPos = builder.Position;
       builder.AppendLine("xref");
       builder.AppendLine("0 2");
       builder.AppendLine("0000000000 65535 f\r");
       builder.AppendLine($"{objPos:0000000000} 00000 n\r");
       builder.AppendLine("trailer <</Root 1 0 R>>");
       builder.AppendLine("startxref");
       builder.AppendLine(xrefPos.ToString());
       builder.AppendLine("%EOF");

       var asStr = builder.ToString();
       using var lld = await new PdfLowLevelReader().ReadFromAsync(asStr.AsExtendedAsciiBytes());

       Assert.Equal(10, await lld.TrailerDictionary.GetAsync<int>(KnownNames.Root));
    }
    [Fact]
    public async Task ParseDoubleDereferenceAsync()
    {
       // Pdf 2.0 spec section 7.3.10 adds that an unenclosed top level object can be just an object reference.
       var builder = new FileBuilder();
       builder.AppendLine("%PDF-1.7");
       builder.AppendLine("%ÿÿÿÿ Created with Melville.Pdf");
       var obj1Pos = builder.Position;
       builder.AppendLine("1 0 obj (John Melville) endobj");
       var obj2Pos = builder.Position;
       builder.AppendLine("2 0 obj 1 0 R endobj");
       var xrefPos = builder.Position;
       builder.AppendLine("xref");
       builder.AppendLine("0 3");
       builder.AppendLine("0000000000 65535 f\r");
       builder.AppendLine($"{obj1Pos:0000000000} 00000 n\r");
       builder.AppendLine($"{obj2Pos:0000000000} 00000 n\r");
       builder.AppendLine("trailer <</Root 2 0 R>>");
       builder.AppendLine("startxref");
       builder.AppendLine(xrefPos.ToString());
       builder.AppendLine("%EOF");

       var asStr = builder.ToString();
       var lld = await new PdfLowLevelReader().ReadFromAsync(asStr.AsExtendedAsciiBytes());

       Assert.Equal("John Melville", (await lld.TrailerDictionary[KnownNames.Root]).ToString());
    }
}
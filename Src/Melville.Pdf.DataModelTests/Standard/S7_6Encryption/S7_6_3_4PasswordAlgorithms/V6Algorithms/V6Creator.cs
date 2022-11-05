﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;
using Xunit.Abstractions;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms.V6Algorithms;

public class RngStub:IRandomNumberSource
{
    private readonly byte[][] sources;
    private int next = 0;
    
    public RngStub(params byte[][] sources)
    {
        this.sources = sources;
    }

    public void Fill(in Span<byte> bytes)
    {
        sources[next++].AsSpan().CopyTo(bytes);
    }
}

public class V6Creator
{
    public V6Creator(ITestOutputHelper toh) => Console.SetOut(new OutputAdapter(toh));
    [Fact]
    public async Task ComputeV6EncryptDictionary()
    {
        var rng = new RngStub(
             "59D66D8A8192A7F2176FF01AC0BD50B4".BitsFromHex(),
             "56569AAB5856B73AF0D38D86BA03C104".BitsFromHex()
            );
        var dict = new V6Encryptor("User", "User", (PdfPermission)(-4),
            KnownNames.StdCF, KnownNames.StdCF, null, new V4CfDictionary(KnownNames.AESV3, 256),
            rng).CreateEncryptionDictionary(
            new PdfArray(
                (PdfObject)new PdfString("591462DB348F2F4E849B5C9195C94B95".BitsFromHex()),
                (PdfObject)new PdfString("DAC57C30E8425659C52B7DDE83523235".BitsFromHex())
            ));

    Assert.Equal("/Standard", (await dict.GetAsync<PdfObject>(KnownNames.Filter)).ToString());
    Assert.Equal("5", (await dict.GetAsync<PdfObject>(KnownNames.V)).ToString());
    Assert.Equal("6", (await dict.GetAsync<PdfObject>(KnownNames.R)).ToString());
    Assert.Equal("256", (await dict.GetAsync<PdfObject>(KnownNames.Length)).ToString());
    Assert.Equal("9339F5439BC00EE5DD113FE21796E2D7A2FA0D2864CC86F1947F925A1521849259D66D8A8192A7F2176FF01AC0BD50B4",
        (await dict.GetAsync<PdfString>(KnownNames.U)).Bytes.AsSpan().HexFromBits());
    Assert.Equal("2662FD1939D25F33DA79A35FC18BD18F9E363E704AA8C792452B440DB17B093456569AAB5856B73AF0D38D86BA03C104",
        (await dict.GetAsync<PdfString>(KnownNames.O)).Bytes.AsSpan().HexFromBits());
    }

}

public class OutputAdapter : TextWriter
{
    private readonly ITestOutputHelper toh;

    public OutputAdapter(ITestOutputHelper toh)
    {
        this.toh = toh;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void WriteLine(string? value) => toh.WriteLine(value);
    public override void WriteLine(string format, params object?[] arg) => toh.WriteLine(format, arg);
}

/* some sampe data from a v5/6 encryupted file

Password is: User.  for both owner and user passwords

TrailerDictionary
<<
/Size 111
/Root 2 0 R
/Encrypt 1 0 R
/Info 4 0 R
/ID [<591462DB348F2F4E849B5C9195C94B95> <DAC57C30E8425659C52B7DDE83523235>]
/Type /XRef
/W [1 3 2]
/Filter /FlateDecode
/Index [0 111]
/Length 342
>>


Encrypt Dictionary
1 0 obj
<<
/Filter /Standard
/V 5
/Length 256
/R 6
/O <2662FD1939D25F33DA79A35FC18BD18F9E363E704AA8C792452B440DB17B093456569AAB5856B73AF0D38D86BA03C104>
/U <9339F5439BC00EE5DD113FE21796E2D7A2FA0D2864CC86F1947F925A1521849259D66D8A8192A7F2176FF01AC0BD50B4>
/P -4
/OE <5C106EAA70A47C00E476B8FF8CDF36D208D4D29B9C390B2F9A4DC86F41B8ED74>
/UE <7263A4774ED221E01C50DFB1772B1EB1565F42EA12696C2981802D1787423EE2>
/Perms <C78CE80AABBAF32EF29E84C78E1473A3>
/CF <<
/StdCF <<
/Type /CryptFilter
/CFM /AESV3
/AuthEvent /DocOpen
/Length 32
>>
>>
/StrF /StdCF
/StmF /StdCF
>>

*/
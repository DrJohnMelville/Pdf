using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.ReferenceDocuments.Utility;
using Melville.Postscript.Interpreter.Values;
using Xunit;
using V6Encryptor = Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6.V6Encryptor;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms.V6Algorithms;

public class RngStub : IRandomNumberSource
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
    [Fact]
    public async Task ComputeV6EncryptDictionaryAsync()
    {
        var rng = new RngStub(
            "72F8431B6E1D37EA7EFB2BE8C0B4B6CCB16CCC7962DA71400258CAA2F7E4C6C0".BitsFromHex(),
            "59D66D8A8192A7F2176FF01AC0BD50B4".BitsFromHex(),
            "56569AAB5856B73AF0D38D86BA03C104".BitsFromHex(),
            "407B6076".BitsFromHex()
        );
        var dict = new V6Encryptor("User", "User", (PdfPermission)(-4),
            KnownNames.StdCF, KnownNames.StdCF, default, new V4CfDictionary(KnownNames.AESV3, 256),
            rng).CreateEncryptionDictionary(
            new PdfArray(
                PdfDirectObject.CreateString("591462DB348F2F4E849B5C9195C94B95".BitsFromHex()),
                PdfDirectObject.CreateString("DAC57C30E8425659C52B7DDE83523235".BitsFromHex())
            ));

        Assert.Equal("Standard", (await dict.GetAsync<string>(KnownNames.Filter)));
        Assert.Equal("5", (await dict.GetAsync<string>(KnownNames.V)).ToString());
        Assert.Equal("6", (await dict.GetAsync<string>(KnownNames.R)).ToString());
        Assert.Equal("256", (await dict.GetAsync<string>(KnownNames.Length)).ToString());
        Assert.Equal("9339F5439BC00EE5DD113FE21796E2D7A2FA0D2864CC86F1947F925A1521849259D66D8A8192A7F2176FF01AC0BD50B4",
            (await dict.GetAsync<StringSpanSource>(KnownNames.U)).GetSpan().HexFromBits());
        Assert.Equal("7263A4774ED221E01C50DFB1772B1EB1565F42EA12696C2981802D1787423EE2",
            (await dict.GetAsync<StringSpanSource>(KnownNames.UE)).GetSpan().HexFromBits());
        Assert.Equal("2662FD1939D25F33DA79A35FC18BD18F9E363E704AA8C792452B440DB17B093456569AAB5856B73AF0D38D86BA03C104",
            (await dict.GetAsync<StringSpanSource>(KnownNames.O)).GetSpan().HexFromBits());
        Assert.Equal("5C106EAA70A47C00E476B8FF8CDF36D208D4D29B9C390B2F9A4DC86F41B8ED74",
            (await dict.GetAsync<StringSpanSource>(KnownNames.OE)).GetSpan().HexFromBits());
        Assert.Equal(-4, (await dict.GetAsync<long>(KnownNames.P)));
        Assert.Equal("C78CE80AABBAF32EF29E84C78E1473A3",
            (await dict.GetAsync<StringSpanSource>(KnownNames.Perms)).GetSpan().HexFromBits());
        Assert.Equal(KnownNames.StdCF, await dict[KnownNames.StmF]);
        Assert.Equal(KnownNames.StdCF, await dict[KnownNames.StrF]);
        var cf = await dict.GetAsync<PdfDictionary>(KnownNames.CF);
        var stdCf = await cf.GetAsync<PdfDictionary>(KnownNames.StdCF);
        Assert.Equal(KnownNames.AESV3, await stdCf[KnownNames.CFM]);
        Assert.Equal(KnownNames.DocOpen, await stdCf[KnownNames.AuthEvent]);
        Assert.Equal(256, await stdCf.GetAsync<int>(KnownNames.Length));
    }
}
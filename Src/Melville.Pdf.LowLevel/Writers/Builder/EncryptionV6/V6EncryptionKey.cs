using System;
using System.Diagnostics;
using Melville.INPC;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

internal readonly partial struct V6EncryptionKey
{
    [FromConstructor]private readonly Memory<byte> data;
    [FromConstructor] private readonly Memory<byte> encryptedFileKey;
    
    partial void OnConstructed() =>  Debug.Assert(data.Length >= 48);

    public Span<byte> Hash => data.Span.Slice(0, 32);
    public Span<byte> ValidationSalt => data.Span.Slice(32,8);
    public Span<byte> KeySalt => data.Span.Slice(40,8);
    public Span<byte> WholeKey => data.Span;
    public Span<byte> EncryptedFileKey => encryptedFileKey.Span;

    public static V6EncryptionKey FromRandomSource(IRandomNumberSource source)
    {
        var rawBits = new byte[48];
        var ret = new V6EncryptionKey(rawBits, new byte[32]);
        ret.SetRandomBits(source);
        return ret;
    }

    private void SetRandomBits(IRandomNumberSource source) => source.Fill(data.Span.Slice(32, 16));
    public PdfDirectValue HashAsPdfString() => PdfDirectValue.CreateString(data.Span);
    public PdfDirectValue EncodedKeyAsPdfString() => PdfDirectValue.CreateString(encryptedFileKey.Span);
    
}
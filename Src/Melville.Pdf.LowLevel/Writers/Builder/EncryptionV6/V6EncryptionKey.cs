using System;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

public readonly partial struct V6EncryptionKey
{
    [FromConstructor]private readonly byte[] data;
    [FromConstructor] private readonly byte[] encryptedFileKey;
    
    partial void OnConstructed() =>  Debug.Assert(data.Length >= 48);

    public Span<byte> Hash => data.AsSpan(0, 32);
    public Span<byte> ValidationSalt => data.AsSpan(32,8);
    public Span<byte> KeySalt => data.AsSpan(40,8);
    public Span<byte> WholeKey => data.AsSpan();
    public Span<byte> EncryptedFileKey => encryptedFileKey.AsSpan();

    public static V6EncryptionKey FromRandomSource(IRandomNumberSource source)
    {
        var rawBits = new byte[48];
        var ret = new V6EncryptionKey(rawBits, new byte[32]);
        ret.SetRandomBits(source);
        return ret;
    }

    private void SetRandomBits(IRandomNumberSource source) => source.Fill(data.AsSpan(32, 16));
    public PdfString HashAsPdfString() => new(data);
    public PdfString EncodedKeyAsPdfString() => new(encryptedFileKey);
    
}
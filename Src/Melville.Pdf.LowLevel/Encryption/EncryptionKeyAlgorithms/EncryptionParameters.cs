using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

internal readonly partial struct EncryptionParameters
{
    [FromConstructor]public Memory<byte> IdFirstElement {get;}
    [FromConstructor]public Memory<byte> OwnerPasswordHash {get;}
    [FromConstructor]public Memory<byte> UserPasswordHash {get;}
    [FromConstructor]public uint Permissions {get;}
    [FromConstructor]public int KeyLengthInBits {get;}

    public int KeyLengthInBytes => KeyLengthInBits / 8;
    
    public static async ValueTask<EncryptionParameters> CreateAsync(PdfValueDictionary trailer) =>
        (await trailer.GetOrNullAsync(KnownNames.IDTName).CA()).TryGet(out PdfValueArray id)&&
        (await id[0].CA()).TryGet(out Memory<byte> firstId) &&
        (await trailer.GetOrNullAsync(KnownNames.EncryptTName).CA()).TryGet(out PdfValueDictionary dict)&&
        (await dict.GetOrNullAsync(KnownNames.OTName).CA()).TryGet(out Memory<byte> ownerHash) &&
        (await dict.GetOrNullAsync(KnownNames.UTName).CA()).TryGet(out Memory<byte> userHash) &&
        (await dict.GetOrNullAsync(KnownNames.PTName).CA()).TryGet(out long permissions)?
            new EncryptionParameters(
                firstId, ownerHash, userHash, (uint)permissions, 
                await dict.GetOrDefaultAsync(KnownNames.LengthTName, 40).CA()):
            throw new PdfSecurityException("Required parameter missing for encryption");

    public override string ToString() =>
        $"""
        IdFirstElt: {IdFirstElement.Span.AsHex()}
        Owner: {OwnerPasswordHash.Span.AsHex()}
        User: {UserPasswordHash.Span.AsHex()}
        Permissions: {Permissions}
        KeyLengthInBits: {KeyLengthInBits}
        """;
}

internal static class HexStrings
{
    public static string AsHex(in this Span<byte> str) =>
        string.Join(" ", str.ToArray().Select(i => i.ToString("X2")));
}
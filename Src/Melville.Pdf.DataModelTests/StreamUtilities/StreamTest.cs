﻿using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public static class StreamTest
{
    public static async Task TestContentAsync(
        string encoded, string decoded, PdfDirectObject decoder, PdfDirectObject parameters) =>
        await VerifyStreamContentAsync(decoded,
            await StaticCodecFactory.CodecFor(decoder)
                .DecodeOnReadStreamAsync(StringAsAsciiStream(encoded), parameters, null));

    public static async Task VerifyStreamContentAsync(string src, Stream streamToRead)
    {
        var buf = new byte[src.Length+200];
        var read = await buf.FillBufferAsync(0, buf.Length, streamToRead);
        Assert.Equal(src, buf[..read].ExtendedAsciiString());
    }
    private static MemoryStream StringAsAsciiStream(string content) => 
        new(ExtendedAsciiEncoding.AsExtendedAsciiBytes(content));
}
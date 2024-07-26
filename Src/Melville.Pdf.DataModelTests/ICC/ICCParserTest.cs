using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Model.Tags;
using Melville.Icc.Parser;
using Melville.Parsing.PipeReaders;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class ICCParserTest
{
    [Fact]
    public async Task SizeFieldAsync()
    {
        var source = LoadSampleData();
        var profile = await new IccParser( ReusableStreamByteSource.Rent(source, false)).ParseAsync();

        Assert.Equal(60960u, profile.Header.Size);
        Assert.Equal(0u, profile.Header.CmmType);
        Assert.Equal(0x4200000u, profile.Header.Version);
        Assert.Equal(ProfileClass.ColorSpace, profile.Header.ProfileClass);
        Assert.Equal(ColorSpace.RGB, profile.Header.DeviceColorSpace);
        Assert.Equal(ColorSpace.Lab, profile.Header.ProfileConnectionColorSpace);
        Assert.Equal(new DateTime(2007, 07, 25, 00, 05, 37, DateTimeKind.Utc), profile.Header.CreatedDate);
        Assert.Equal(0x61637370u, profile.Header.Signature);
        Assert.Equal(0u, profile.Header.Platform);
        Assert.Equal((ProfileFlags)0, profile.Header.ProfileFlags);
        Assert.Equal(0u, profile.Header.Manufacturer);
        Assert.Equal(0u, profile.Header.Device);
        Assert.Equal((DeviceAttributes)0, profile.Header.DeviceAttributes);
        Assert.Equal(RenderIntent.Perceptual, profile.Header.RenderIntent);
        Assert.Equal(0.96420f, profile.Header.Illuminant.X, 4, MidpointRounding.ToEven);
        Assert.Equal(1f, profile.Header.Illuminant.Y);
        Assert.Equal(0.82491f, profile.Header.Illuminant.Z, 4,  MidpointRounding.ToEven);
        Assert.Equal(0u, profile.Header.Creator);
        Assert.Equal((ulong)0x34562ABF994CCd06, profile.Header.ProfileIdHigh);
        Assert.Equal((ulong)0x6D2C5721D0D68C5D, profile.Header.ProfileIdLow);

        Assert.Equal(9, profile.Tags.Count);
        Assert.Equal(StrEnc("desc"), profile.Tags[0].Tag);
        Assert.Equal(240u, profile.Tags[0].Offset);
        Assert.Equal(118u, profile.Tags[0].Size);
        Assert.Equal(StrEnc("chad"), profile.Tags[8].Tag);
        Assert.Equal(60916u, profile.Tags[8].Offset);
        Assert.Equal(44u, profile.Tags[8].Size);
    }

    private uint StrEnc(string s)
    {
        uint ret = 0;
        foreach (var character in s)
        {
            ret <<= 8;
            ret |= (byte)character;
        }

        return ret;
    }

    private Stream LoadSampleData() =>
        GetType().Assembly.GetManifestResourceStream("Melville.Pdf.DataModelTests.ICC.sample.icc")!;

    private static async Task<T> ParseTagAsync<T>(string source) 
    {
        var str = (await (source+">").ParseValueObjectAsync()).ForceTo<Memory<byte>>();
        var reader = new ReadOnlySequence<byte>(str);
        return (T)TagParser.Parse(reader);
    }

    [Fact]
    public async Task ChromacityParserAsync()
    {
        var tag = await ParseTagAsync<ChromacityTag>("<6368726d0000000000030002" +
                                                "0001 0001 0002 0002" +
                                                "000A0002 00002000" +
                                                "00B00003 00003000>");
        Assert.Equal(3, tag.Channels);
        Assert.Equal(Colorant.SMPTEP145, tag.Colorant);
        Assert.Equal((1.0000153f, 2.0000305f), tag.Coordinates[0]);
        Assert.Equal((10.0000305f, 0.1250019f), tag.Coordinates[1]);
        Assert.Equal((176.00005f, 0.18750286f), tag.Coordinates[2]);
    }

    [Fact]
    public async Task ParseColorOrderAsync()
    {
        var tag = await ParseTagAsync<ColorantOrderTag>("<636c726f00000000 00000003 030201>");
        Assert.Equal(3, tag.Colors.Count);
        Assert.Equal(3, tag.Colors[0]);
        Assert.Equal(2, tag.Colors[1]);
        Assert.Equal(1, tag.Colors[2]);
    }

    [Fact]
    public async Task ColorantTableTypeAsync()
    {
        var tag = await ParseTagAsync<ColorantTableTag>("<636c727400000000 00000002" +
                                                   "6162636400000000000000000000000000000000000000000000000000000000" +
                                                   "0003 0002 0001" +
                                                   "6364656600000000000000000000000000000000000000000000000000000000" +
                                                   "0005 0006 0007" +
                                                   ">");
        Assert.Equal(2, tag.Colorants.Count);
        Assert.Equal("abcd", tag.Colorants[0].Name);
        Assert.Equal(3, tag.Colorants[0].X);
        Assert.Equal(2, tag.Colorants[0].Y);
        Assert.Equal(1, tag.Colorants[0].Z);
        Assert.Equal("cdef", tag.Colorants[1].Name);
        Assert.Equal(5, tag.Colorants[1].X);
        Assert.Equal(6, tag.Colorants[1].Y);
        Assert.Equal(7, tag.Colorants[1].Z);
    }

    [Fact]
    public async Task CurveTag0TestAsync()
    {
        var tag = await ParseTagAsync<NullCurve>("<6375727600000000 00000000>");
    }
    [Fact]
    public async Task CurveTag1TestAsync()
    {
        var tag = await ParseTagAsync<ParametricCurveTag>("<6375727600000000 00000001 0280>");
        Assert.Equal(2.5, tag.G,2);
        Assert.Equal(1, tag.A);
        Assert.Equal(0, tag.B);
        Assert.Equal(0, tag.C);
        Assert.Equal(float.MinValue, tag.D);
        Assert.Equal(1, tag.E);
        Assert.Equal(0, tag.F);
        
    }
    [Fact]
    public async Task CurveTagTestAsync()
    {
        var tag = await ParseTagAsync<SampledCurveSegment>("<6375727600000000 00000005 0005 0004 0003 0002 0001>");
        Assert.Equal(5, tag.Samples.Count);
        Assert.Equal(7.629511E-05f, tag.Samples[0]);
        Assert.Equal(6.103609E-05f, tag.Samples[1]);
        Assert.Equal(4.5777066E-05f, tag.Samples[2]);
        Assert.Equal(3.0518044E-05f, tag.Samples[3]);
        Assert.Equal(1.5259022E-05f, tag.Samples[4]);
    }

    [Fact]
    public async Task DataTagTestAsync()
    {
        var tag = await ParseTagAsync<DataTag>("<6461746100000000 00000000 616263646500>");
        Assert.Equal(DataType.String, tag.Type);
        Assert.Equal(new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65, 0 }, tag.Data);
        Assert.Equal("abcde", tag.AsString());
    }

    [Fact]
    public async Task DateTimeTagTestAsync()
    {
        var tag = await ParseTagAsync<DateTimeTag>("<6474696d 00000000 000A00010002 000300040005>");
        Assert.Equal(new DateTime(10, 1, 2, 3, 4, 5, 0, DateTimeKind.Utc), tag.DateTime);
    }

    [Fact]
    public async Task Lut16TagTestAsync()
    {
        var tag = await ParseTagAsync<LutXTag>("<6d66743200000000 02010300 " +
                                          // matrix
                                          "00010000 00000000 00000000" +
                                          "00000000 00010000 00000000" +
                                          "00000000 00000000 00010000" +
                                          //input and output table size
                                          "00030004" +
                                          // input table 2 inputs * 3 entries = 6
                                          "0001 0002 0003 0004 0005 0006" +
                                          // clut = 3 grid points ^ 2 inputs * 1 output = 9
                                          "0001 0002 0003 0004 0005 0006 0007 0008 0009" +
                                          // output table = 4 output table entries * 1 output
                                          "0001 0002 0003 0004>");

        Assert.Equal(2, tag.Inputs);
        Assert.Equal(1, tag.Outputs);
        Assert.Equal(3, tag.GridPoints);

        Assert.Equal(1f, tag.Matrix.M11);
        Assert.Equal(0f, tag.Matrix.M12);
        Assert.Equal(0f, tag.Matrix.M13);
        Assert.Equal(0f, tag.Matrix.M21);
        Assert.Equal(1f, tag.Matrix.M22);
        Assert.Equal(0f, tag.Matrix.M23);
        Assert.Equal(0f, tag.Matrix.M31);
        Assert.Equal(0f, tag.Matrix.M32);
        Assert.Equal(1f, tag.Matrix.M33);

        Assert.Equal(3, tag.InputTableEntries);
        Assert.Equal(4, tag.OutputTableEntries);

        Assert.Equal(IncrementingFloatArray(6, 16), tag.InputTables);
        Assert.Equal(IncrementingFloatArray(9, 16), tag.Clut);
        Assert.Equal(IncrementingFloatArray(4, 16), tag.OutputTables);
    }

    private float[] IncrementingFloatArray(int len, int bits)
    {
        float epsilon = 1.0f / ((1 << bits) - 1);
        return Enumerable.Range(1, len).Select(i => i * epsilon).ToArray();
    }

    [Fact]
    public async Task Lut8TagTestAsync()
    {
        var tag = await ParseTagAsync<LutXTag>("<6d66743100000000 02010300 " +
                                          // matrix
                                          "00010000 00000000 00000000" +
                                          "00000000 00010000 00000000" +
                                          "00000000 00000000 00010000" +
                                          // input table 2 inputs * 256 entries = 512
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f 00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          // clut = 3 grid points ^ 2 inputs * 1 output = 9
                                          "01 02 03 04 05 06 07 08 09" +
                                          // output table = 256 output table entries * 1 output
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f" +
                                          "00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f"
                                          );

        Assert.Equal(2, tag.Inputs);
        Assert.Equal(1, tag.Outputs);
        Assert.Equal(3, tag.GridPoints);

        Assert.Equal(1f, tag.Matrix.M11);
        Assert.Equal(0f, tag.Matrix.M12);
        Assert.Equal(0f, tag.Matrix.M13);
        Assert.Equal(0f, tag.Matrix.M21);
        Assert.Equal(1f, tag.Matrix.M22);
        Assert.Equal(0f, tag.Matrix.M23);
        Assert.Equal(0f, tag.Matrix.M31);
        Assert.Equal(0f, tag.Matrix.M32);
        Assert.Equal(1f, tag.Matrix.M33);

        Assert.Equal(256, tag.InputTableEntries);
        Assert.Equal(256, tag.OutputTableEntries);

        AssertFloatArraySame(IncrementingFloatArray(9, 8), tag.Clut, 0.00000001);
    }

    private void AssertFloatArraySame(float[] f1, IReadOnlyList<float> f2, double tolerence)
    {
        Assert.Equal(f1.Length, f2.Count);
        Assert.True(f1.Zip(f2, (i, j) => Math.Abs(i - j) < tolerence).All(i => i));
    }

    [Fact]
    public async Task MeasurementTypeTestAsync()
    {
        var tag = await ParseTagAsync<MeasurementTypeTag>("<6d65617300000000 00000001 " +
                                                     "0001 0000 00020000 00030000" +
                                                     "00000002 00010000 00000005>");

        Assert.Equal(StandardObserver.Cie1931, tag.Observer);
        Assert.Equal(1, tag.MeasurementBacking.X);
        Assert.Equal(2, tag.MeasurementBacking.Y);
        Assert.Equal(3, tag.MeasurementBacking.Z);
        Assert.Equal(MeasurmentGeomenty.a0, tag.Geometry);
        Assert.Equal(MeasurmentFlare.f100, tag.Flare);
        Assert.Equal(StandardIllumination.D55, tag.Illumination);
    }

    [Fact]
    public async Task MultiLocalizedStringAsync()
    {
        var tag = await ParseTagAsync<MultiLocalizedUnicodeTag>($"<{MultiLocalizedName}>");
        VerifyMultiLocalizedStream(tag);
    }

    private const string MultiLocalizedName = "6d6c7563 00000000 00000003 0000000C" +
                                              "00010004 0000000A 00000034" +
                                              "00020007 0000000E 00000044" +
                                              "00010009 0000000A 00000034" +
                                              "00610062 00630064 00650000 00000000" +
                                              "00410042 00430044 00450046 0047";

    private static void VerifyMultiLocalizedStream(MultiLocalizedUnicodeTag tag)
    {
        Assert.Equal(3, tag.Encodings.Count);
        Assert.Equal(1, tag.Encodings[0].Language);
        Assert.Equal(4, tag.Encodings[0].Country);
        Assert.Equal("abcde", tag.Encodings[0].Value);
        Assert.Equal(2, tag.Encodings[1].Language);
        Assert.Equal(7, tag.Encodings[1].Country);
        Assert.Equal("ABCDEFG", tag.Encodings[1].Value);
        Assert.Equal(1, tag.Encodings[2].Language);
        Assert.Equal(9, tag.Encodings[2].Country);
        Assert.Equal("abcde", tag.Encodings[2].Value);
    }

    [Fact]
    public async Task NamedColorTagAsync()
    {
        var tag = await ParseTagAsync<NamedColorTag>(
            "<6e636c32 00000000 ffff0000 00000002 00000003" +
            "61620000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" + // prefix
            "62610000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" + // postfix
            "41424300 00000000 00000000 00000000 00000000 00000000 00000000 00000000" + // first color name
            "ffff0000 ffff7777 66665555" + // first color values
            "46474800 00000000 00000000 00000000 00000000 00000000 00000000 00000000" + // Second color name
            "0000ffff ffff1111 22221234" + // Second color values
            ">");
        Assert.Equal(0xffff0000u, tag.VendorSpecificFlag);
        Assert.Equal(2, tag.Colors.Count);
        Assert.Equal("abABCba", tag.Colors[0].Name);
        Assert.Equal("abFGHba", tag.Colors[1].Name);
        Assert.Equal(1.0f, tag.Colors[0].PcsValue.X);
        Assert.Equal(0.0f, tag.Colors[0].PcsValue.Y);
        Assert.Equal(1.0f, tag.Colors[0].PcsValue.Z);
        Assert.Equal(0x7777, tag.Colors[0].DeviceValue[0]);
        Assert.Equal(0x6666, tag.Colors[0].DeviceValue[1]);
        Assert.Equal(0x5555, tag.Colors[0].DeviceValue[2]);
        Assert.Equal(0.0f, tag.Colors[1].PcsValue.X);
        Assert.Equal(1.0f, tag.Colors[1].PcsValue.Y);
        Assert.Equal(1.0f, tag.Colors[1].PcsValue.Z);
        Assert.Equal(0x1111, tag.Colors[1].DeviceValue[0]);
        Assert.Equal(0x2222, tag.Colors[1].DeviceValue[1]);
        Assert.Equal(0x1234, tag.Colors[1].DeviceValue[2]);
    }

    [Theory]
    [InlineData("<70617261 00000000 00000000 000F0000>", 15, 1, 0, 0, float.MinValue, 0, 0)]
    [InlineData("<70617261 00000000 00010000 000F0000 000E0000 000D0000>", 15, 14, 13, 0, -13f / 14, 0, 0)]
    [InlineData("<70617261 00000000 00020000 000F0000 000E0000 000D0000 000C0000>", 15, 14, 13, 12, -13f / 14, 0, 0)]
    [InlineData("<70617261 00000000 00030000 000F0000 000E0000 000D0000 000C0000 000B0000>", 15, 14, 13, 0, 11, 12, 0)]
    [InlineData("<70617261 00000000 00040000 000F0000 000E0000 000D0000 000C0000 000B0000 000A0000 00090000>",
        15, 14, 13, 12, 11, 10, 9)]
    public async Task ParametricCurveTagAsync(string text,
        float g, float a, float b, float c, float d, float e, float f)
    {
        var tag = await ParseTagAsync<ParametricCurveTag>(text);
        Assert.Equal(g, tag.G);
        Assert.Equal(a, tag.A);
        Assert.Equal(b, tag.B);
        Assert.Equal(c, tag.C);
        Assert.Equal(d, tag.D);
        Assert.Equal(e, tag.E);
        Assert.Equal(f, tag.F);
    }

    [Fact]
    public async Task ProfileSequenceDescTagAsync()
    {
        var pds = $"00000001 00000002 00000000 00000004 7669646d {MultiLocalizedName} {MultiLocalizedName}";
        var tag = await ParseTagAsync<ProfileSequenceDescriptionTag>($"<70736571 00000000 00000002 {pds} {pds}>");
        Assert.Equal(2, tag.Profiles.Count);
        Assert.Equal(1ul, tag.Profiles[0].Manufacturer);
        Assert.Equal(2ul, tag.Profiles[0].Device);
        Assert.Equal(DeviceAttributes.Negative, tag.Profiles[0].DeviceAttributes);
        Assert.Equal(DeviceTechnology.VideoMonitor, tag.Profiles[0].DeviceTechnology);
        VerifyMultiLocalizedStream(tag.Profiles[1].DeviceName);
    }

    [Fact]
    public async Task ProfileSequenceIdentifierTagAsync()
    {
        var tag = await ParseTagAsync<ProfileSequenceIdentifierTag>("<70736964 00000000 00000002" +
                                                               "0000001C 00000062 0000001c 00000062" +
                                                               $"12345678 12345678 87654321 A9876543 {MultiLocalizedName}>");
        Assert.Equal(2, tag.Profiles.Count);
        Assert.Equal(0x1234567812345678ul, tag.Profiles[0].ProfileIdHigh);
        Assert.Equal(0x87654321A9876543ul, tag.Profiles[0].ProfileIdLow);
        Assert.Equal(0x1234567812345678ul, tag.Profiles[1].ProfileIdHigh);
        Assert.Equal(0x87654321A9876543ul, tag.Profiles[1].ProfileIdLow);
        VerifyMultiLocalizedStream(tag.Profiles[0].Description);
        VerifyMultiLocalizedStream(tag.Profiles[1].Description);
    }

    [Fact]
    public async Task ResponseCurve16Set16TagTestAsync()
    {
        var tag = await ParseTagAsync<ResponseCurveSet16Tag>(
            "<72637332 00000000 00030002 00000014 00000014" +
            " 53746141 00000003 00000003 00000003" +
            " 00040000 00050000 00060000 00070000 00080000 00090000 000A0000 000B0000 000C0000" +
            " 00010000 00020000 00030000 00040000 00050000 00060000 00070000 00080000 00090000 000A0000 000B0000 000C0000" +
            "00010000 00020000 00030000 00040000 00050000 00060000>");
        Assert.Equal(2, tag.Curves.Count);
        CheckResponseCurve(tag.Curves[0]);
        CheckResponseCurve(tag.Curves[1]);
    }

    private static void CheckResponseCurve(ResponseCurve curve)
    {
        Assert.Equal(CurveMeasurement.StatusA, curve.Unit);
        Assert.Equal(3, curve.Channels.Count);
        Assert.Equal(3, curve.Channels[0].Response.Count);
        Assert.Equal(3, curve.Channels[1].Response.Count);
        Assert.Equal(3, curve.Channels[2].Response.Count);

        Assert.Equal(4, curve.Channels[0].MaximumColorantValue.X);
        Assert.Equal(5, curve.Channels[0].MaximumColorantValue.Y);
        Assert.Equal(6, curve.Channels[0].MaximumColorantValue.Z);
        Assert.Equal(7, curve.Channels[1].MaximumColorantValue.X);
        Assert.Equal(8, curve.Channels[1].MaximumColorantValue.Y);
        Assert.Equal(9, curve.Channels[1].MaximumColorantValue.Z);
        Assert.Equal(10, curve.Channels[2].MaximumColorantValue.X);
        Assert.Equal(11, curve.Channels[2].MaximumColorantValue.Y);
        Assert.Equal(12, curve.Channels[2].MaximumColorantValue.Z);

        Assert.Equal(1, curve.Channels[0].Response[0].DeviceValue);
        Assert.Equal(2, curve.Channels[0].Response[0].MeasurementValue);
        Assert.Equal(3, curve.Channels[0].Response[1].DeviceValue);
        Assert.Equal(4, curve.Channels[0].Response[1].MeasurementValue);
        Assert.Equal(5, curve.Channels[0].Response[2].DeviceValue);
        Assert.Equal(6, curve.Channels[0].Response[2].MeasurementValue);
        Assert.Equal(7, curve.Channels[1].Response[0].DeviceValue);
        Assert.Equal(8, curve.Channels[1].Response[0].MeasurementValue);
        Assert.Equal(9, curve.Channels[1].Response[1].DeviceValue);
        Assert.Equal(10, curve.Channels[1].Response[1].MeasurementValue);
        Assert.Equal(11, curve.Channels[1].Response[2].DeviceValue);
        Assert.Equal(12, curve.Channels[1].Response[2].MeasurementValue);
        Assert.Equal(1, curve.Channels[2].Response[0].DeviceValue);
        Assert.Equal(2, curve.Channels[2].Response[0].MeasurementValue);
        Assert.Equal(3, curve.Channels[2].Response[1].DeviceValue);
        Assert.Equal(4, curve.Channels[2].Response[1].MeasurementValue);
        Assert.Equal(5, curve.Channels[2].Response[2].DeviceValue);
        Assert.Equal(6, curve.Channels[2].Response[2].MeasurementValue);
    }

    [Fact]
    public async Task ParseS15Fixed16ArrayTagAsync()
    {
        var tag = await ParseTagAsync<S15Fixed16Array>("<73663332 00000000" +
                                                  "00010000 00020000 00030000 00040000>");
        Assert.Equal(4, tag.Values.Count);
        Assert.Equal(1f, tag.Values[0]);
        Assert.Equal(2f, tag.Values[1]);
        Assert.Equal(3f, tag.Values[2]);
        Assert.Equal(4f, tag.Values[3]);
    }

    [Fact]
    public async Task ParseSignatureTagAsync()
    {
        var tag = await ParseTagAsync<SignatureTag>("<73696720 00000000 12345678>");
        Assert.Equal(0x12345678u, tag.Signature);
    }

    [Fact]
    public async Task ParseTextTypeAsync()
    {
        var tag = await ParseTagAsync<TextTag>("<74657874 00000000 61626364 6566>");
        Assert.Equal("abcdef", tag.Text);
    }

    [Fact]
    public async Task Parseu16Fixed16ArrayAsync()
    {
        var tag = await ParseTagAsync<U16Fixed16Array>("<75663332 00000000 00010000 00020000>");
        Assert.Equal(2, tag.Values.Count);
        Assert.Equal(1, tag.Values[0]);
        Assert.Equal(2, tag.Values[1]);
    }

    [Fact]
    public async Task ParseUint16ArrayAsync()
    {
        var tag = await ParseTagAsync<UInt16Array>("<75693136 00000000 00010002>");
        Assert.Equal(2, tag.Values.Count);
        Assert.Equal(1, tag.Values[0]);
        Assert.Equal(2, tag.Values[1]);
    }

    [Fact]
    public async Task ParseUint32ArrayAsync()
    {
        var tag = await ParseTagAsync<UInt32Array>("<75693332 00000000 00000001 00000002>");
        Assert.Equal(2, tag.Values.Count);
        Assert.Equal(1u, tag.Values[0]);
        Assert.Equal(2u, tag.Values[1]);
    }

    [Fact]
    public async Task ParseUint64ArrayAsync()
    {
        var tag = await ParseTagAsync<UInt64Array>("<75693634 00000000 00000000 00000001 00000000 00000002>");
        Assert.Equal(2, tag.Values.Count);
        Assert.Equal(1ul, tag.Values[0]);
        Assert.Equal(2ul, tag.Values[1]);
    }

    [Fact]
    public async Task ParseXyzArrayAsync()
    {
        var tag = await ParseTagAsync<XyzArray>("<58595a20 00000000 00010000 00020000 00030000 00040000 00050000 00060000>");
        Assert.Equal(2, tag.Values.Count);
        Assert.Equal(1, tag.Values[0].X);
        Assert.Equal(2, tag.Values[0].Y);
        Assert.Equal(3, tag.Values[0].Z);
        Assert.Equal(4, tag.Values[1].X);
        Assert.Equal(5, tag.Values[1].Y);
        Assert.Equal(6, tag.Values[1].Z);
    }

    [Fact]
    public async Task ParseViewindConditionsTabAsync()
    {
        var tag = await ParseTagAsync<ViewingConditionsTag>(
            "<76696577 00000000 00010000 00020000 00030000 00040000 00050000 00060000 00000004>");
        Assert.Equal(1, tag.IlluminantValue.X);
        Assert.Equal(2, tag.IlluminantValue.Y);
        Assert.Equal(3, tag.IlluminantValue.Z);
        Assert.Equal(4, tag.SurroundValue.X);
        Assert.Equal(5, tag.SurroundValue.Y);
        Assert.Equal(6, tag.SurroundValue.Z);
        Assert.Equal(StandardIllumination.F2, tag.IlluminantType);
    }


    [Fact]
    public async Task ParseLutBAIdentityAsync()
    {
        var tag = await ParseTagAsync<LutAToBTag>("<6D414220 00000000 03020000 " +
                                             "00000000 00000000 00000000 00000000 00000000>");

        Assert.Equal(1, tag.Matrix.Kernel.M11, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.Kernel.M12, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.Kernel.M13, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.Kernel.M21, 6, MidpointRounding.ToZero);
        Assert.Equal(1, tag.Matrix.Kernel.M22, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.Kernel.M23, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.Kernel.M31, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.Kernel.M32, 6, MidpointRounding.ToZero);
        Assert.Equal(1, tag.Matrix.Kernel.M33, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.TranslateX, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.TranslateY, 6, MidpointRounding.ToZero);
        Assert.Equal(0, tag.Matrix.TranslateZ, 6, MidpointRounding.ToZero);

        VerifyCurveType<NullCurve>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<NullCurve>(2, tag.OutputCurves);

        Assert.Equal(NullColorTransform.Instance(3), tag.LookupTable);
    }

    private static void VerifyCurveType<T>(int length, IReadOnlyList<ICurveTag> curves)
    {
        Assert.Equal(length, curves.Count);
        foreach (var curve in curves)
        {
            Assert.True(curve is T);
        }
    }

    [Fact]
    public async Task ParseLutABWithMatrixAsync()
    {
        var tag = await ParseTagAsync<LutAToBTag>("<6D414220 00000000 03020000 " +
                                             "00000000 00000020 00000000 00000000 00000000" +
                                             "00000001 00000002 00000003 00000004 00000005 00000006 00000007 00000008 00000009 0000000A 0000000B 0000000C 0000000D>");

        Assert.Equal(1 * 1.5259022E-05, tag.Matrix.Kernel.M11, 6);
        Assert.Equal(2 * 1.5259022E-05, tag.Matrix.Kernel.M12, 6);
        Assert.Equal(3 * 1.5259022E-05, tag.Matrix.Kernel.M13, 6);
        Assert.Equal(4 * 1.5259022E-05, tag.Matrix.Kernel.M21, 6);
        Assert.Equal(5 * 1.5259022E-05, tag.Matrix.Kernel.M22, 6);
        Assert.Equal(6 * 1.5259022E-05, tag.Matrix.Kernel.M23, 6);
        Assert.Equal(7 * 1.5259022E-05, tag.Matrix.Kernel.M31, 6);
        Assert.Equal(8 * 1.5259022E-05, tag.Matrix.Kernel.M32, 6);
        Assert.Equal(9 * 1.5259022E-05, tag.Matrix.Kernel.M33, 6);
        Assert.Equal(10 * 1.5259022E-05, tag.Matrix.TranslateX, 6);
        Assert.Equal(11 * 1.5259022E-05, tag.Matrix.TranslateY, 6);
        Assert.Equal(12 * 1.5259022E-05, tag.Matrix.TranslateZ, 6);
    }

    private const string SimpleParametricCurve = "70617261 00000000 00000000 000F0000";

    [Fact]
    public async Task ParseLutABWithACurvesAsync()
    {
        var tag = await ParseTagAsync<LutAToBTag>("<6D414220 00000000 03020000 " +
                                             "00000000 00000000 00000000 00000000 00000020 " +
                                             SimpleParametricCurve + SimpleParametricCurve + SimpleParametricCurve
        );
        VerifyCurveType<ParametricCurveTag>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<NullCurve>(2, tag.OutputCurves);
    }

    [Fact]
    public async Task ParseLutABWithBCurvesAsync()
    {
        var tag = await ParseTagAsync<LutAToBTag>("<6D414220 00000000 03020000 " +
                                             "00000020 00000000 00000000 00000000 00000000 " +
                                             SimpleParametricCurve + SimpleParametricCurve
        );
        VerifyCurveType<NullCurve>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<ParametricCurveTag>(2, tag.OutputCurves);
    }

    [Fact]
    public async Task ParseLutABWithMatrixCurvesAsync()
    {
        var tag = await ParseTagAsync<LutAToBTag>("<6D414220 00000000 03020000 " +
                                             "00000000 00000000 00000020 00000000 00000000 " +
                                             SimpleParametricCurve + SimpleParametricCurve + SimpleParametricCurve
        );
        VerifyCurveType<NullCurve>(3, tag.InputCurves);
        VerifyCurveType<ParametricCurveTag>(3, tag.MatrixCurves);
        VerifyCurveType<NullCurve>(2, tag.OutputCurves);
    }

    [Fact]
    public async Task ParseLutBAWithACurvesAsync()
    {
        var tag = await ParseTagAsync<LutBToATag>("<6D424120 00000000 03020000 " +
                                             "00000000 00000000 00000000 00000000 00000020 " +
                                             SimpleParametricCurve + SimpleParametricCurve
        );
        VerifyCurveType<NullCurve>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<ParametricCurveTag>(2, tag.OutputCurves);
    }

    [Fact]
    public async Task ParseLutBAWithBCurvesAsync()
    {
        var tag = await ParseTagAsync<LutBToATag>("<6D424120 00000000 03020000 " +
                                             "00000020 00000000 00000000 00000000 00000000 " +
                                             SimpleParametricCurve + SimpleParametricCurve + SimpleParametricCurve
        );
        VerifyCurveType<ParametricCurveTag>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<NullCurve>(2, tag.OutputCurves);
    }

    private const string clut8Bit3To2 =
        "02030200 00000000 00000000 00000000" + // parameter dimensions -- 12 entreis/24 bytes total
        "01000000" + // use byte size data
        "01020304 05060708 090a0b0c 0d0e0f10 11121314 15161718";
    
    [Fact]
    public async Task ParseLutBAWithClut8Async()
    {
        var tag = await ParseTagAsync<LutBToATag>("<6D424120 00000000 03020000 " +
                                             "00000000 00000000 00000000 00000020 00000000 " +
                                             clut8Bit3To2+">"
        );
        VerifyCurveType<NullCurve>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<NullCurve>(2, tag.OutputCurves);
        Verify8BitClut(tag.LookupTable);
    }

    private static void Verify8BitClut(object tag)
    {
        var table = tag as MultidimensionalLookupTable;
        Assert.NotNull(table);
        Assert.Equal(2, table!.DimensionLengths[0]);
        Assert.Equal(3, table.DimensionLengths[1]);
        Assert.Equal(2, table.DimensionLengths[2]);
        Assert.Equal(0, table.DimensionLengths[3]);

        for (int i = 0; i < 24; i++)
        {
            Assert.Equal((1f + i) / 255, table.Points[i]);
        }
    }

    [Fact]
    public async Task ParseLutBAWithClut16Async()
    {
        var tag = await ParseTagAsync<LutBToATag>("<6D424120 00000000 03020000 " +
                                             "00000000 00000000 00000000 00000020 00000000 " +
                                             "02030200 00000000 00000000 00000000" + // parameter dimensions -- 12 entreis/24 bytes total
                                             "02000000" + // use ushort size data
                                             "00010002 00030004 00050006 00070008 0009000a 000b000c " +
                                             "000d000e 000f0010 00110012 00130014 00150016 00170018>"
        );
        VerifyCurveType<NullCurve>(3, tag.InputCurves);
        VerifyCurveType<NullCurve>(3, tag.MatrixCurves);
        VerifyCurveType<NullCurve>(2, tag.OutputCurves);
        var table = tag.LookupTable as MultidimensionalLookupTable;
        Assert.NotNull(table);
        Assert.Equal(2, table!.DimensionLengths[0]);
        Assert.Equal(3, table.DimensionLengths[1]);
        Assert.Equal(2, table.DimensionLengths[2]);
        Assert.Equal(0, table.DimensionLengths[3]);

        for (int i = 0; i < 24; i++)
        {
            Assert.Equal((1f + i) / ushort.MaxValue, table.Points[i]);
        }
    }

    [Fact]
    public async Task MpeWithCLutAsync()
    {
        var tag = await ParseTagAsync<MultiProcessTag>("<6d706574 00000000 00030002 00000001 00000018 00000024" +
           "636c7574 00000000 00030002" +
           "01010100 00000000 00000000 00000000 " + // every dimension has one element
           "40a33333 40a33333>");
        Assert.Single(tag.Elements);
        var clut = (MultidimensionalLookupTable)tag.Elements[0];
        Assert.Equal(5.1, clut.Points[0],2);
        Assert.Equal(5.1, clut.Points[1],2);
    }

    [Fact]
    public async Task MpeWithMatrixAsync()
    {
        var tag = await ParseTagAsync<MultiProcessTag>("<6d706574 00000000 00020002 00000001 00000018 00000024" +
           "6d617466 00000000 00020002" +
           "40a33333 40a33333 40a33333 40a33333 40a33333 40a33333 >");
        Assert.Single(tag.Elements);
        var mat = (MultiProcessMatrix)tag.Elements[0];
        Assert.Equal(2, mat.Inputs);
        Assert.Equal(2, mat.Outputs);
        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(5.1, mat.Values[i], 3);
        }
    }

    [Theory]
    [InlineData("62414353")]
    [InlineData("65414353")]
    public async Task MpeWithNullItemAsync(string name)
    {
        var tag = await ParseTagAsync<MultiProcessTag>("<6d706574 00000000 00020002 00000001 00000018 00000010" +
                                                  name + " 00000000 00020002 00000000>");
        Assert.Single(tag.Elements);
        Assert.True(tag.Elements[0] is NullColorTransform);
        Assert.Equal(2, tag.Elements[0].Inputs);
        Assert.Equal(2, tag.Elements[0].Outputs);
        
    }

    [Fact]
    public async Task MpeCurveSetAsync()
    {
        var tag = await ParseTagAsync<MultiProcessTag>("<6d706574 00000000 00020002 00000001 00000018 00000064" + // mp TAG
           "6d666c74 00000000 00020002 0000001C 00000048 0000001C 00000048  " + // curve set - 2 identical curves
           "63757266 00000000 00020000 40a33333" + // formula curve segment with 2 parts
           "70617266 00000000 00000000 40a33333 40a33333 40a33333 40a33333" + // formula part 1
           "70617266 00000000 00000000 40a33333 40a33333 40a33333 40a33333" + // formula part 2
           ">");
        Assert.Single(tag.Elements);
        var funcs = (MultiProcessCurveSet)tag.Elements[0];
        Assert.Equal(2, funcs.Inputs);
        Assert.Equal(2, funcs.Outputs);
        Assert.Equal(2, funcs.Curves.Count);
        VerifyFunc(funcs.Curves[0]);
        VerifyFunc(funcs.Curves[1]);
    }

    private void VerifyFunc(MultiProcessCurve curve)
    {
        Assert.Single(curve.BreakPoints);
        Assert.Equal(5.1, curve.BreakPoints[0], 2);
        Assert.Equal(2, curve.Segments.Count);
        VerifySegment(curve.Segments[0]);
        VerifySegment(curve.Segments[1]);
    }

    private void VerifySegment(ICurveTag curveSegment)
    {
        var fs = (FormulaSegmentType0)curveSegment;
        Assert.NotNull(fs);
        Assert.Equal(5.1, fs.Gamma, 3);
        Assert.Equal(5.1, fs.A, 3);
        Assert.Equal(5.1, fs.B, 3);
        Assert.Equal(5.1, fs.C, 3); 
    }

    [Fact]
    public async Task ParseType1FormulaCurveSegmentAsync()
    {
        var tag = await ParseTagAsync<FormulaSegmentType1>(
            "<70617266 00000000 00010000 40a33333 40a33333 40a33333 40a33333 40a33333>");
        Assert.Equal(5.1, tag.Gamma, 3);
        Assert.Equal(5.1, tag.A, 3);
        Assert.Equal(5.1, tag.B, 3);
        Assert.Equal(5.1, tag.C, 3); 
        Assert.Equal(5.1, tag.D, 3); 
    }
    [Fact]
    public async Task ParseType2FormulaCurveSegmentAsync()
    {
        var tag = await ParseTagAsync<FormulaSegmentType2>(
            "<70617266 00000000 00020000 40a33333 40a33333 40a33333 40a33333 40a33333>");
        Assert.Equal(5.1, tag.A, 3);
        Assert.Equal(5.1, tag.B, 3);
        Assert.Equal(5.1, tag.C, 3);
        Assert.Equal(5.1, tag.D, 3);
        Assert.Equal(5.1, tag.E, 3);
    }
    [Fact]
    public async Task ParseSampledCurveSegmentAsync()
    {
        var tag = await ParseTagAsync<SampledCurveSegment>(
            "<73616d66 00000000 00000005 40a33333 40a33333 40a33333 40a33333 40a33333>");
        Assert.Equal(6, tag.Samples.Count);
        for (int i = 1; i < 6; i++)
        {
            Assert.Equal(5.1, tag.Samples[i], 3);
            
        }

        Assert.Equal(0, tag.Samples[0]);
        ((ICurveSegment)tag).Initialize(1,10,233);
        Assert.Equal(1, tag.Minimum, 3 , MidpointRounding.ToEven);
        Assert.Equal(10, tag.Maximum, 3, MidpointRounding.ToEven);
        Assert.Equal(233, tag.Samples[0], 3, MidpointRounding.ToEven);
        
    }
}
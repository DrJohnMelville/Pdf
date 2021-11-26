using System;
using System.IO;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Xunit;

namespace Melville.Pdf.DataModelTests.ICC;

public class ICCParserTest
{
    [Fact]
    public async Task SizeField()
    {
      var source = LoadSampleData();
      var profile = await new IccParser(PipeReader.Create(source)).ParseAsync();

      Assert.Equal(60960u, profile.Header.Size);
      Assert.Equal(0u, profile.Header.CmmType);
      Assert.Equal(0x4200000u, profile.Header.Version);
      Assert.Equal(ProfileClass.ColorSpace, profile.Header.ProfileClass);
      Assert.Equal(ColorSpace.RGB, profile.Header.DeviceColorSpace);
      Assert.Equal(ColorSpace.Lab, profile.Header.ProfileConnectionColorSpace);
      Assert.Equal(new DateTime(2007,07, 25, 00, 05,37, DateTimeKind.Utc), profile.Header.CreatedDate);
      Assert.Equal(0x61637370u, profile.Header.Signature);
      Assert.Equal(0u, profile.Header.Platform);
      Assert.Equal((ProfileFlags)0, profile.Header.ProfileFlags);
      Assert.Equal(0u, profile.Header.Manufacturer);
      Assert.Equal(0u, profile.Header.Device);
      Assert.Equal((DeviceAttributes)0, profile.Header.DeviceAttributes);
      Assert.Equal(RenderIntent.Perceptual, profile.Header.RenderIntent);
      Assert.Equal(0.96420f, profile.Header.Illuminant.X,4);
      Assert.Equal(1f, profile.Header.Illuminant.Y);
      Assert.Equal(0.82491f, profile.Header.Illuminant.Z, 4);
      Assert.Equal(0u, profile.Header.Creator);
      Assert.Equal((ulong)0x34562ABF994CCd06, profile.Header.ProfileIdHigh);
      Assert.Equal((ulong)0x6D2C5721D0D68C5D, profile.Header.ProfileIdLow);
    }

    private Stream LoadSampleData() => 
        GetType().Assembly.GetManifestResourceStream("Melville.Pdf.DataModelTests.ICC.sample.icc")!;
}
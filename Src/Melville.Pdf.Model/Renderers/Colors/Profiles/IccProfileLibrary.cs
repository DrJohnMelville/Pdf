using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Parser;

namespace Melville.Pdf.Model.Renderers.Colors.Profiles;

public static class IccProfileLibrary
{
    private static IccProfile? sRGB;

    public static async ValueTask<IccProfile> ReadSrgb() => sRGB ??=
        await LoadProfile(@"AdobeSrgb.icc").ConfigureAwait(false);

    private static IccProfile? cmyk;

    public static async ValueTask<IccProfile> ReadCmyk() => cmyk ??=
        await LoadProfile(@"Cmyk.icc").ConfigureAwait(false);
    
    private static ValueTask<IccProfile> LoadProfile(string profileFile) =>
        new IccParser(PipeReader.Create(
            GetIccProfileData(profileFile))).ParseAsync();

    public static Stream GetIccProfileData(string profileFile) =>
        typeof(ColorSpaceFactory).Assembly.GetManifestResourceStream(
            "Melville.Pdf.Model.Renderers.Colors.Profiles."+profileFile) ??
        throw new InvalidDataException("Cannot find resource: " + profileFile);
    
}
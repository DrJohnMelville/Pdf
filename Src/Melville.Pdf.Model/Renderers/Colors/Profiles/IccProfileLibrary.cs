using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.Colors.Profiles;

public static class IccProfileLibrary
{
    public const string CmykProfileName = @"Cmyk.icc";
    private static IccProfile? cmyk;

    public static async ValueTask<IccProfile> ReadCmyk() => cmyk ??=
        await LoadProfile(CmykProfileName).CA();
    
    private static ValueTask<IccProfile> LoadProfile(string profileFile) =>
        new IccParser(PipeReader.Create(
            GetIccProfileData(profileFile))).ParseAsync();

    public static Stream GetIccProfileData(string profileFile) =>
        typeof(ColorSpaceFactory).Assembly.GetManifestResourceStream(
            "Melville.Pdf.Model.Renderers.Colors.Profiles."+profileFile) ??
        throw new InvalidDataException("Cannot find resource: " + profileFile);
    
}
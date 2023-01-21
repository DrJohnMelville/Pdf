using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Icc.Model;
using Melville.Icc.Parser;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Pdf.Model.Renderers.Colors.Profiles;

/// <summary>
/// Holds the static Cmyk to PCS profile
/// </summary>
public static class CmykIccProfile
{
    private const string CmykProfileName = @"Cmyk.icc";
    private static IccProfile? cmyk;

    /// <summary>
    /// Get the cmyk ICC profile
    /// </summary>
    public static async ValueTask<IccProfile> ReadCmykProfile() => cmyk ??=
        await LoadProfile().CA();
    
    private static ValueTask<IccProfile> LoadProfile() =>
        new IccParser(PipeReader.Create(
            GetCmykProfileStream())).ParseAsync();

    /// <summary>
    /// Load the CMYK ICC profile as a stream
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public static Stream GetCmykProfileStream() =>
        typeof(ColorSpaceFactory).Assembly.GetManifestResourceStream(
            "Melville.Pdf.Model.Renderers.Colors.Profiles."+CmykProfileName) ??
        throw new InvalidDataException("Cannot find resource: " + CmykProfileName);
    
}
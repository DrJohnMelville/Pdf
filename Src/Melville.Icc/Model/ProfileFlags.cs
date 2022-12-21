namespace Melville.Icc.Model;

/// <summary>
/// Attributes of this profile
/// </summary>
[Flags]
public enum ProfileFlags : uint
{
    /// <summary>
    /// Inidicates that the profile is embedded in a media file
    /// </summary>
    Embedded = 1,
    /// <summary>
    /// If true then the profile cannot be used independant of the embedded color data
    /// </summary>
    Dependent = 2
}
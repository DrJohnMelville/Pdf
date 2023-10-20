namespace Melville.Pdf.FormReader.AcroForms;

[Flags]
internal enum AcroFieldFlags
{
    // common to all fields
    ReadOnly = 1 <<0,
    Required = 1 << 1,
    NoExport = 1 << 2,
    
    // Button fields
    NoToggleToOff = 1 << 14,
    Radio = 1 << 15,
    PushButton = 1 << 16,
    RadiosInUnizon = 1 << 25,

    // Text Items
    Multiline = 1 << 12,
    Password = 1 << 13,
    FileSelect = 1 << 20,
    DoNotSpellCheck = 1 << 22,
    DoNotScroll = 1 << 23,
    Comb = 1 << 24,
    RichText = 1 << 25,

    // Choice Fields
    Combo = 1 << 17,
    Edit = 1 << 18,
    Sort = 1 << 19,
    MultiSelect = 1 << 21,
    CommitOnSelChange = 1 << 26,

}
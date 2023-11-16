namespace Melville.Pdf.FormReader.XfaForms;

internal static class NameSplitter
{
    public static ReadOnlySpan<char> LocalName(string name)
    {
        int index = name.LastIndexOf('.');
        var terminal = index < 0 ? name.AsSpan() : name.AsSpan()[(index + 1)..];
        return (terminal.Length > 3 && terminal[^3..].SequenceEqual("[0]".AsSpan())) ? 
            terminal[..^3] : terminal;
    }
}
using System.IO;
using System.Net;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Wpf.FakeUris;

public sealed class TempFontDirectory: IDisposable
{
    private int serial = 1;
    private readonly string baseFolder = 
        Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())+"\\";

    public TempFontDirectory()
    {
        Directory.CreateDirectory(baseFolder);
    }

    public async ValueTask<string> StoreStream(PdfStream s)
    {
        var index = serial++;
        using (var target = File.Create($"{baseFolder}{index}.ttf"))
        { 
            await (await s.StreamContentAsync()).CopyToAsync(target);
        }

        return $"{baseFolder}{index}.ttf";
    }

    public void Dispose()
    {
        Directory.Delete(baseFolder, true);
    }
}
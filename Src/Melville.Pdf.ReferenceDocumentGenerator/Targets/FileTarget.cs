using System;
using System.IO;
using System.Linq;

namespace Melville.Pdf.ReferenceDocumentGenerator.Targets;

public class FileTarget: ITarget
{
    private string path;

    public FileTarget(string path)
    {
        this.path = path;
    }

    public Stream CreateTargetStream() => File.Create(path);

    public void View()
    {
        Console.WriteLine("Wrote to file: " + path);
    }
}
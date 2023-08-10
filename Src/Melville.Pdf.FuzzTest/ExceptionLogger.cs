using System.Collections.Generic;
using System.IO;
using Melville.INPC;
using Melville.Linq.Statistics.FileWriter;

namespace Melville.Pdf.FuzzTest;

public class ExceptionLogger
{
    private List<LoggedException> exceptions = new();
    public void Log(string path, int page, string message)
    {
        exceptions.Add(new (path, page, message));
    }

    public void WriteTo(Stream output)
    {
        var writer = new ExcelFileWriter(output);
        writer.AddObjectPage("Errors", exceptions);
        writer.Save();
    }
}

public partial class LoggedException
{
    [FromConstructor] public string Path { get; }
    [FromConstructor] public int Page { get; }
    [FromConstructor] public string Message { get; }
}
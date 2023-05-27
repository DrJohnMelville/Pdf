using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.FuzzTest;

public static class Program
{
    public static async Task Main(string[] cmdLineArgs)
    {
        if (cmdLineArgs.Length < 1)
        {
            Console.WriteLine("Must Pass a root path to find PDF files");
        }

        var pdfs = GatherPdfs(cmdLineArgs[0]);
        foreach (var pdf in pdfs)
        {
            await ParseFile.DoAsync(pdf);
        }
        Console.WriteLine();
        Console.WriteLine("Done");
    }

    private static IEnumerable<string> GatherPdfs(string cmdLineArg)
    {
        foreach (var file in Directory.EnumerateFiles(cmdLineArg, "*.pdf",
                     new EnumerationOptions(){ IgnoreInaccessible = true, RecurseSubdirectories = true, ReturnSpecialDirectories = false}))
        {
            yield return file;
        }
    }
}
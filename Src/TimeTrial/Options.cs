using CommandLine;
using Melville.Pdf.Model;
using Melville.Pdf.Model.Renderers.DocumentRenderers;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace TimeTrial;

public class Options
{
    [Value(0, MetaName = "Source", HelpText = "Source file, or blank for all built ins",
        Required = false)]
    public IEnumerable<string> Sources { get; set; } = Array.Empty<string>();

    [Option('p', "page", HelpText = "The page number to render",
        Required = false, Default = -1)]
    public int PageNumber { get; set; }

    [Option('b', "builtin", HelpText = "Name of a builtin text", Required = false)]
    public IEnumerable<string> BuiltInNames { get; set; } = Array.Empty<string>();

    public IAsyncEnumerable<SourceItem> Items()
    {
        return GetBuiltinItems().Concat(GetFiles()).SelectMany(ExpandPages);
    }

    private async IAsyncEnumerable<SourceItem> ExpandPages(SourceItem item)
    {
        if (PageNumber > 0)
        {
            yield return item with { Page = PageNumber };
            yield break;
        }

        for (int i = 1; i <= item.Renderer.TotalPages; i++)
        {
            yield return item with { Page = i };
        }
    }

    private async IAsyncEnumerable<SourceItem> GetFiles()
    {
        foreach (var fullPath in Sources.SelectMany(PathsFromGlob))
        {
            DocumentRenderer renderer;
            try
            {
                renderer = await new PdfReader().ReadFromFileAsync(fullPath);
            }
            catch (Exception)
            {
                continue;
            }

            yield return new(Path.GetFileName(fullPath), renderer);
        }
    }

    private IEnumerable<string> PathsFromGlob(string glob)
    {
        var dir = Environment.CurrentDirectory;
        return glob.Contains(':')
            ? new[] { glob }
            : new Matcher().AddInclude(glob).Execute(
                    new DirectoryInfoWrapper(new DirectoryInfo(dir))).Files
                .Select(i => Path.Join(dir, i.Path));
    }

    private async IAsyncEnumerable<SourceItem> GetBuiltinItems()
    {
        foreach (var builtin in BuiltInNames)
        {
            var generators = GeneratorFactory.AllGenerators
                .Where(i => builtin == "all" ||
                i.Prefix.AsSpan(1).StartsWith(builtin.AsSpan()));
            foreach (var prebuilt in generators)
            {
                var dr = await prebuilt.ReadDocumentAsync();
                yield return new SourceItem(prebuilt.Prefix[1..], dr);
            }
        }
    }
}
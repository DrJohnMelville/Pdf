using System.Collections.Generic;
using Melville.Linq;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public class PostscriptFunctionBuilder
{
    private List<ClosedInterval> domains = new ();
    private List<ClosedInterval> ranges = new ();

    public void AddArgument(ClosedInterval domain) => domains.Add(domain);
    public void AddOutput(ClosedInterval range) => ranges.Add(range);

    public PdfStream Create(string code) => Create(code, new DictionaryBuilder());
    public PdfStream Create(string code, DictionaryBuilder members) =>
        AddFunctionItems(members).AsStream(code);

    private DictionaryBuilder AddFunctionItems(in DictionaryBuilder builder) =>
        builder
            .WithItem(KnownNames.FunctionType, new PdfInteger(4))
            .WithItem(KnownNames.Domain, domains.AsPdfArray())
            .WithItem(KnownNames.Range, ranges.AsPdfArray());
}
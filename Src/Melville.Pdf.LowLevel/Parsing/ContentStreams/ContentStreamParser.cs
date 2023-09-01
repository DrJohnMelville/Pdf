using System.Formats.Asn1;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

/// <summary>
/// Parses a content stream (expressed as a PipeReader) and "renders" it to an IContentStreamOperations.
/// </summary>
public readonly partial struct ContentStreamParser
{
    /// <summary>
    /// The target that we are parsing the content stream to.
    /// </summary>
    [FromConstructor] private readonly IContentStreamOperations target;

    /// <summary>
    /// Render the content stream operations in the given CodeSource pipereader.
    /// </summary>
    /// <param name="source">The content stream to parse.</param>
    public async ValueTask ParseAsync(PipeReader source)
    {
        var engine = new PostscriptEngine(contentStreamCommands);
        engine.Tag = target;

        await engine.ExecuteAsync(new Tokenizer(source)).CA();
        
        if (engine.ErrorData.TryGetAs("newerror", out bool value) && value)
            throw new PdfParseException("Error parsing content stream;");
    }

    private static readonly IPostscriptDictionary contentStreamCommands =
        PostscriptValueFactory.CreateSizedDictionary(80).Get<IPostscriptDictionary>()
            .With(ContentStreamParsingOperations.AddOperations);
}
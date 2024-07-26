using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Melville.Fonts.Type1TextParsers;
using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;
using Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevelViewerParts.PostscriptDebuggers;

public static partial class TextBoxSetSelectionImplementation
{

    [GenerateDP(Default = -1)]
    public static void OnSetSelectionStartChanged(DependencyObject obj, int position)
    {
        if (obj is TextBox tb)
        {
            tb.Select(position, 1);
        }
    }
    [GenerateDP(Default = -1)]
    public static void OnSetSelectionLengthChanged(DependencyObject obj, int position)
    {
        if (obj is TextBox tb)
        {
            tb.Select(GetSetSelectionStart(obj), position);
        }
    }
}

public partial class PostscriptDebuggerViewModel
{
    [FromConstructor] [AutoNotify] private string code = "";
    [AutoNotify] private int selectionStart = -1;
    [AutoNotify] private int selectionLength= -1;
    [AutoNotify] private TextTreeViewModel operandStack = TextTreeViewModel.Empty;
    [AutoNotify] private TextTreeViewModel dictionaryStack = TextTreeViewModel.Empty;
    [AutoNotify] private string currentToken = "";
    [AutoNotify] private int interpreterStyle;

    private PostscriptEngine engine;
    private ITokenSource tokens;
    private IAsyncEnumerator<PostscriptValue?> pausedProgram;

    private void OnCodeChanged() => SetupEngine();
    private void OnInterpreterStyleChanged() => SetupEngine();

    [MemberNotNull(nameof(engine))]
    [MemberNotNull(nameof(tokens))]
    [MemberNotNull(nameof(pausedProgram))]
    partial void OnConstructed()
    {
        SetupEngine();
    }

    [MemberNotNull(nameof(engine))]
    [MemberNotNull(nameof(tokens))]
    [MemberNotNull(nameof(pausedProgram))]
    private void SetupEngine()
    {
        tokens = new Tokenizer(code);
        engine = CreateEngine();
        pausedProgram = engine.SingleStepAsync(tokens).GetAsyncEnumerator();
        StepInto();
    }

    private PostscriptEngine CreateEngine()
    {
        return InterpreterStyle switch
        {
            1 => SharedPostscriptParser.BasicPostscriptEngine(),
            2 => new Type1Parser(MultiplexSourceFactory.Create(Code.AsExtendedAsciiBytes()))
                .CreateEngine(),
            _ => new PostscriptEngine(PostscriptOperatorCollections.BaseLanguage())
        };
    }

    public async void StepInto()
    {
        await InnerStepAsync();
        DisplayOnPause();
    }

    private void DisplayOnPause()
    {
        SelectionLength = (int)tokens.CodeSource.Position - SelectionStart;
        OperandStack = TextTreeViewModel.ReadOperandStack(engine.OperandStack);
        DictionaryStack = TextTreeViewModel.ReadDictionaryStack(engine.DictionaryStack);
    }

    private async Task InnerStepAsync()
    {
        SelectionStart = (int)tokens.CodeSource.Position;
        CurrentToken = await pausedProgram.MoveNextAsync()
            ? pausedProgram.Current?.ToString() ?? ""
            : "End of Program";
    }

    public async void StepOver()
    {
        var codeSourcePosition = (int)tokens.CodeSource.Position;
        var next = Code.AsSpan(codeSourcePosition).IndexOfAny("\r\n") + codeSourcePosition;
        while (tokens.CodeSource.Position is {} pos && pos <= next && pos < Code.Length-1 &&
               CurrentToken != "End of Program")
        {
            await InnerStepAsync();
        }
        DisplayOnPause();
    }


    public bool KeyDown(KeyEventArgs kea)
    {
        switch (EffectiveKey(kea))
        {
            case Key.F11:
                StepInto();
                return true;
            case Key.F10:
                StepOver();
                return true;
        }
        return false;
    }

    private static Key EffectiveKey(KeyEventArgs kea) => 
        kea.Key == Key.System ? kea.SystemKey: kea.Key;
}
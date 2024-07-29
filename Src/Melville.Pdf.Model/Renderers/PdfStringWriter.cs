using System;
using System.Buffers;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.Model.Renderers.FontRenderings;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.Type3;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Model.Renderers;

internal sealed class PdfStringWriter : ISpacedStringBuilder
{
    private readonly IFontTarget target;
    private readonly IRealizedFont font;
    private readonly IFontWriteOperation writer;
    private readonly uint[] buffer;

    public PdfStringWriter(IFontTarget target)
    {
        this.target = target;
        font = GraphicsState.Typeface; 
        writer = font.BeginFontWrite(target);
        buffer = ArrayPool<uint>.Shared.Rent(10);
    }

    private GraphicsState GraphicsState => target.RenderTarget.GraphicsState.CurrentState();

    public ValueTask SpacedStringComponentAsync(double value)
    { 
        var delta = GraphicsState.FontSize * value/ 1000.0;
        UpdateTextPosition(-delta);
        return ValueTask.CompletedTask;
    }

    public async ValueTask SpacedStringComponentAsync(ReadOnlyMemory<byte> value)
    {
        ReadOnlyMemory<byte> remainingInput = value;
        while (remainingInput.Length > 0)
        {
            Memory<uint> characters = font.ReadCharacter.GetCharacters(ref remainingInput, buffer);
            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters.Span[i];
                var glyph = font.MapCharacterToGlyph.GetGlyph(character);
                var measuredGlyphWidth = await writer.AddGlyphToCurrentStringAsync(
                    character, glyph, CharacterPositionMatrix()).CA();
                AdjustTextPositionForCharacter(font.CharacterWidth(character, measuredGlyphWidth), character);
            }
        }
    }

    private Matrix3x2 CharacterPositionMatrix() => GraphicsState.GlyphTransformMatrix();

    private void AdjustTextPositionForCharacter(double width, uint character) => 
        UpdateTextPosition(width*GraphicsState.FontSize+CharacterSpacingAdjustment(character));

    private double CharacterSpacingAdjustment(uint character) =>
        GraphicsState.CharacterSpacing + ApplicableWordSpacing(character);

    private double ApplicableWordSpacing(uint character) => 
        IsSpaceCharacter(character)? GraphicsState.WordSpacing:0;

    private bool IsSpaceCharacter(uint character) => character == 0x20;

    private void UpdateTextPosition(double width) =>
        GraphicsState.SetTextMatrix(
            IncrementAlongActiveVector(ScaleHorizontalOffset(width))*
            GraphicsState.TextMatrix
        );

    private double ScaleHorizontalOffset(double width) => 
        width * GraphicsState.HorizontalTextScale;

    private Matrix3x2 IncrementAlongActiveVector(double width) =>
        Matrix3x2.CreateTranslation((float)width, 0.0f);

    public ValueTask DisposeAsync()
    {
        ArrayPool<uint>.Shared.Return(buffer);
        writer.RenderCurrentString(GraphicsState.TextRender, CharacterPositionMatrix());
        writer.Dispose();
        return ValueTask.CompletedTask;
    }
}
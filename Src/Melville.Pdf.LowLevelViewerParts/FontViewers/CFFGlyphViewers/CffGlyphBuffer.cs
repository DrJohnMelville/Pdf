using System.Numerics;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;

public class CffGlyphBuffer : ICffGlyphTarget
{
    public  List<ICffAction> Output { get; } = new();

    public void Operator(CharStringOperators opCode, Span<DictValue> stack)
    {
        AddInstr(new CffOperator(opCode, stack));
    }

    private void AddInstr(ICffAction action) => Output.Add(action);

    public void RelativeCharWidth(float delta) => 
        AddInstr(new CffCharWidthAction(delta));

    public void MoveTo(Vector2 point) => AddInstr(new CffMoveToAction(point));
    public void LineTo(Vector2 point) => AddInstr(new CffLineToAction(point));
    public void CurveTo(Vector2 control1, Vector2 control2, Vector2 endPoint) => 
        AddInstr(new CffCurveToAction(control1, control2, endPoint));
    // CFF fonts never call this, but it is required for the more generic
    // interface.
    public void CurveTo(Vector2 control, Vector2 endPoint) =>
        CurveTo(control, control, endPoint);
    public void EndGlyph() => AddInstr(new CffEndGlyphAction());
}
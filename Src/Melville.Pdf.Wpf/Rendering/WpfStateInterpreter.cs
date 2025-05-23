﻿using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.Model.Renderers.Colors;
using Melville.Pdf.Model.Renderers.GraphicsStates;

namespace Melville.Pdf.Wpf.Rendering;

internal static class WpfStateInterpreter
{
    public static Brush Brush(this WpfGraphicsState state) => state.NonstrokeBrush
        .TryGetNativeBrush<Func<WpfGraphicsState, Brush>>()(state);
    
    public static Pen Pen(this WpfGraphicsState state)
    {
        var lineCap = ConvertLineCap(state.LineCap);
        var pen = new Pen(Brushes.Black, state.EffectiveLineWidth())
        {
            EndLineCap = lineCap,
            StartLineCap = lineCap,
            DashCap = lineCap,
            DashStyle = ComputeDashStyle(state),
            LineJoin = ComputeLineJoin(state.LineJoinStyle),
            MiterLimit = state.MiterLimit,
            Brush = state.StrokeBrush.TryGetNativeBrush<Func<WpfGraphicsState, Brush>>()(state)
        };
        return pen;
    }
    
    public static Color AsWpfColor(in this DeviceColor dc) => 
        Color.FromArgb(dc.Alpha, dc.RedByte, dc.GreenByte, dc.BlueByte);

    private static PenLineJoin ComputeLineJoin(LineJoinStyle joinStyle) => joinStyle switch
    {
        LineJoinStyle.Miter => PenLineJoin.Miter,
        LineJoinStyle.Round => PenLineJoin.Round,
        LineJoinStyle.Bevel => PenLineJoin.Bevel,
        _ => throw new ArgumentOutOfRangeException(nameof(joinStyle), joinStyle, null)
    };
    
    private static DashStyle ComputeDashStyle(GraphicsState state) => 
        state.IsDashedStroke() ?
            CustomDashStyle(state.DashArray, state.DashPhase, state.LineWidth):
            DashStyles.Solid;

    private static DashStyle CustomDashStyle(IReadOnlyList<double> dashes, double phase, double width) =>
        new(dashes.Select(i => i / width).ToArray(), phase / width);

    private static PenLineCap ConvertLineCap(LineCap stateLineCap) => 
        stateLineCap switch
      {
          LineCap.Butt => PenLineCap.Flat,
          LineCap.Round => PenLineCap.Round,
          LineCap.Square => PenLineCap.Square,
          _ => throw new ArgumentOutOfRangeException(nameof(stateLineCap), stateLineCap, null)
      };
   
    public static MatrixTransform Transform(this GraphicsState state) => 
        WpfTransform(state.TransformMatrix);
    public static MatrixTransform WpfTransform(this Matrix3x2 m) => new(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);

    public static Rect AsWpfRect(this in PdfRect src) => new(src.Left, src.Bottom, src.Right, src.Top);

}
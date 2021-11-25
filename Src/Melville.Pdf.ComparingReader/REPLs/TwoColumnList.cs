using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Melville.Pdf.ComparingReader.REPLs;

public class TwoColumnList: Panel
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var rows = GetRows();
        double leftWidth = 0;
        foreach (var row in rows)
        {
            row[0].Measure(availableSize);
            leftWidth = Math.Max(leftWidth, row[0].DesiredSize.Width);
        }

        var rightSize = new Size(Math.Max(10, availableSize.Width - (5 + leftWidth)), availableSize.Height);
        var height = 0.0;
        foreach (var row in rows)
        {
            if (row.Length <= 1) continue; 
            row[1].Measure(rightSize);
            height += row[1].DesiredSize.Height;
        }

        return new Size(availableSize.Width, height);
    }

    private List<UIElement[]> GetRows() => InternalChildren.OfType<UIElement>().Chunk(2).ToList();

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rows = GetRows();
        var desiredWidth = rows.Max(i => i[0].DesiredSize.Width);
        var leftWidth = Math.Min(desiredWidth, finalSize.Width - 15);

        var rightWidth = finalSize.Width - (5 + leftWidth);
        var rightPos = finalSize.Width - rightWidth;
        
        var height = 0.0;
        foreach (var row in rows)
        {
            var leftHeight = row[0].DesiredSize.Height;
            row[0].Arrange(new Rect(new Point(0, height), new Size(leftWidth, leftHeight)));
            var rightHeight = 0.0;
            if (row.Length > 1)
            {
                rightHeight = row[1].DesiredSize.Height;
                row[1].Arrange(new Rect(new Point(rightPos, height), new Size(rightWidth, rightHeight)));
                
            }

            height += Math.Max(leftHeight, rightHeight);
        }

        return new(finalSize.Width, height);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Melville.Pdf.ComparingReader.Viewers.FormViewer;

[ContentProperty(nameof(Templates))]
public partial class SelectByInterface: DataTemplateSelector
{

    public List<DataTemplate> Templates { get; } = new();
    public override DataTemplate SelectTemplate(object item, DependencyObject container) =>
        Templates.FirstOrDefault(i => IsApplicableTemplate(i, item)) ??
        Templates.Last();

    private static bool IsApplicableTemplate(DataTemplate dataTemplate, object item)
    {
        var type = dataTemplate.DataType as Type;
        return type is null || type.IsInstanceOfType(item);
    }
}
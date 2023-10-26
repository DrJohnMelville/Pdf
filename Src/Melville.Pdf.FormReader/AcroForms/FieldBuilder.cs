using System.Collections.ObjectModel;
using Melville.INPC;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.FormReader.AcroForms
{
    internal readonly partial struct FieldBuilder
    {
        [FromConstructor] private readonly string name;
        [FromConstructor] private readonly PdfDirectObject value;
        [FromConstructor] private readonly PdfDirectObject type;
        [FromConstructor] private readonly AcroFieldFlags flags;
        [FromConstructor] private readonly PdfDictionary field;
        [FromConstructor] private readonly PdfIndirectObject reference;
        [FromConstructor] private readonly IList<IPdfFormField> target;

        public ValueTask CreateAsync()
        {
            return type switch
            {
                var x when x.Equals(KnownNames.Btn) => CreateButtonAsync(),
                var x when x.Equals(KnownNames.Ch) => CreateChoiceAsync(),
                _ => CreateTextBoxAsync()
            };
        }

        private ValueTask CreateButtonAsync()
        {
            switch (flags)
            {
                case var x when (x & AcroFieldFlags.PushButton) > 0:
                    target.Add(new AcroFormField(name, value, reference, field));
                    break;
                case var x when (x & AcroFieldFlags.Radio) > 0:
                    return CreateRadioButtonAsync();
                default:
                    target.Add(new AcroCheckBox(name, value, reference, field));
                    break;
            }
        
            return ValueTask.CompletedTask;
        }

        private async ValueTask CreateRadioButtonAsync()
        {
            var kids = await field.GetOrDefaultAsync(KnownNames.Kids, PdfArray.Empty);
            var options = await ReadOptionsAsync(kids);

            target.Add(
                ConstructSingleChoice(options)
            );
        }

        private static async Task<List<PdfPickOption>> ReadOptionsAsync(PdfArray kids)
        {
            var options = new List<PdfPickOption>(kids.Count);
            await foreach (var kid in kids)
            {
                var value = await kid.Get<PdfDictionary>()[KnownNames.AS];
                options.Add(new(value.ToString(), value));
            }
            return options;
        }


        private static PdfPickOption? SearchForValue(List<PdfPickOption> options, PdfDirectObject capturedValue) => 
            options.FirstOrDefault(i => i.Value.Equals(capturedValue));

        private async ValueTask CreateChoiceAsync()
        {
            var opts = await field.GetOrDefaultAsync(KnownNames.Opt, PdfArray.Empty);
            var options = new List<PdfPickOption>(opts.Count);
            await foreach (var option in opts)
            {
                if (option.TryGet(out PdfArray? array) && array.Count > 1)
                {
                    options.Add(new((await array[1]).DecodedString(), await array[0]));
                }
                else
                {
                    options.Add(new(option.DecodedString(),  option));
                }
            }



            target.Add(
                (flags & AcroFieldFlags.MultiSelect) > 0 ?
                    await ConstructMultiChoiceAsync(options):
                    ConstructSingleChoice(options)
            );
        }

        private async ValueTask<IPdfFormField> ConstructMultiChoiceAsync(List<PdfPickOption> options)
        {
            var selections = new ObservableCollection<PdfPickOption>();
            foreach (var selIndirect in value.ObjectAsUnresolvedList())
            {
                if (SearchForValue(options, await selIndirect.LoadValueAsync()) is { } selected)
                {
                    selections.Add(selected);
                }
            }

            return new AcroMultipleChoice(name, value, reference, field, options, selections);
        }

        private AcroSingleChoice ConstructSingleChoice(List<PdfPickOption> options)
        {
            return new AcroSingleChoice(name, value, reference, field, options);
        }


        private ValueTask CreateTextBoxAsync()
        {
            target.Add(new AcroTextBox(name, value, reference, field));
            return ValueTask.CompletedTask;
        }

    }
}
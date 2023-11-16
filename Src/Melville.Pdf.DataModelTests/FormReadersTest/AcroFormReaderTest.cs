using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Parsing.Streams;
using Melville.Pdf.FormReader;
using Melville.Pdf.FormReader.AcroForms;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Xunit;

namespace Melville.Pdf.DataModelTests.FormReadersTest;

public class AcroFormReaderTest
{
    private static async Task<IPdfForm> CreatSingleFieldFormAsync(PdfDictionary formField)
    {
        var builder = new PdfDocumentCreator();
        var fieldDict = builder.LowLevelCreator.Add(formField
        );

        var acroDict = builder.LowLevelCreator.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Fields, new PdfArray(
                fieldDict))
            .AsDictionary());

        builder.AddToRootDictionary(KnownNames.AcroForm, acroDict);

        var lld = builder.CreateDocument();
        return await FormReaderFacade.ReadFormAsync(lld);
    }

    [Fact]
    public async Task ReadSingleTextFieldAsync()
    {
        var reader = await SingleTextBoxFormAsync();

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        reader.Fields[0].Should().BeOfType<AcroTextBox>();
        reader.Fields[0].Name.Should().Be("Text Field");
        reader.Fields[0].Value.Should().Be("Text Value");
    }

    [Fact]
    public static async Task ModifyTextFormAsync()
    {
        var frm = await SingleTextBoxFormAsync();
        ((IPdfTextBox)frm.Fields[0]).StringValue = "FooBar";

        var stream = new MultiBufferStream();
        await (await frm.CreateModifiedDocumentAsync()).WriteToAsync(stream);

        var f2 = await FormReaderFacade.ReadFormAsync(stream.CreateReader());
        f2.Fields[0].Value.DecodedString().Should().Be("FooBar");
    }

    private static async Task<IPdfForm> SingleTextBoxFormAsync()
    {
        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Tx)
            .WithItem(KnownNames.Ff, 0)
            .WithItem(KnownNames.T, "Text Field")
            .WithItem(KnownNames.V, "Text Value")
            .AsDictionary();

        var reader = await CreatSingleFieldFormAsync(formField);
        return reader;
    }

    [Fact]
    public async Task ReadSingleCheckBoxAsync()
    {
        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Btn)
            .WithItem(KnownNames.Ff, 0)
            .WithItem(KnownNames.T, "CheckBox Field")
            .WithItem(KnownNames.V, KnownNames.Yes)
            .AsDictionary();
        
        var reader = await CreatSingleFieldFormAsync(formField);

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        reader.Fields[0].Should().BeOfType<AcroCheckBox>()
            .Subject.IsChecked.Should().Be(true);
        reader.Fields[0].Name.Should().Be("CheckBox Field");
        reader.Fields[0].Value.Should().Be("Yes");

    }

    [Fact]
    public async Task SubFormNameAsync()
    {
        var reader = await CreateSubfomDocumentAsync();

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        reader.Fields[0].Should().BeOfType<AcroCheckBox>()
            .Subject.IsChecked.Should().Be(true);
        reader.Fields[0].Name.Should().Be("FormName.CheckBox Field");
        reader.Fields[0].Value.Should().Be("Yes");

    }

    private static async Task<IPdfForm> CreateSubfomDocumentAsync()
    {
        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Btn)
            .WithItem(KnownNames.Ff, 0)
            .WithItem(KnownNames.T, "CheckBox Field")
            .WithItem(KnownNames.V, KnownNames.Yes)
            .AsDictionary();

        var formArray = new DictionaryBuilder()
            .WithItem(KnownNames.T, "FormName")
            .WithItem(KnownNames.Kids, PdfDirectObject.FromArray(formField))
            .AsDictionary();

        var reader = await CreatSingleFieldFormAsync(formArray);
        return reader;
    }

    [Theory]
    [InlineData("FormName.CheckBox Field")]
    [InlineData("FormName.CheckBox Field[0]")]
    [InlineData("FormName[0].CheckBox Field[0]")]
    [InlineData("FormName[0].CheckBox Field")]
    public async Task GetFieldByNameSucceedAsync(string name)
    {
        (await CreateSubfomDocumentAsync()).NameToField(name).Should().BeOfType<AcroCheckBox>();
    }
    
    [Fact]
    public async Task GetFieldByNameFailAsync()
    {
        (await CreateSubfomDocumentAsync()).Invoking(i=>i.NameToField("ddd"))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ReadPushButtonAsync()
    {
        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Btn)
            .WithItem(KnownNames.Ff, (int)AcroFieldFlags.PushButton)
            .WithItem(KnownNames.T, "Push Button")
            .AsDictionary();
        
        var reader = await CreatSingleFieldFormAsync(formField);

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        reader.Fields[0].Should().BeOfType<AcroFormField>();
        reader.Fields[0].Name.Should().Be("Push Button");

    }

    [Fact]
    public void CheckBoxBool()
    {
        var sut = new AcroCheckBox("xx", KnownNames.OFF, PdfDirectObject.CreateNull(), PdfDictionary.Empty);

        sut.IsChecked.Should().Be(false);
        sut.IsChecked = true;
        sut.IsChecked.Should().Be(true);
        sut.Value.Should().Be(KnownNames.Yes);
        sut.IsChecked = false;
        sut.IsChecked.Should().Be(false);
        sut.Value.Should().Be(KnownNames.OFF);
    }

    [Fact]
    public async Task ReadRadioButtonsAsync()
    {
        var rb1 = new DictionaryBuilder()
            .WithItem(KnownNames.AS, PdfDirectObject.CreateName("JDM"))
            .AsDictionary();
        var rb2 = new DictionaryBuilder()
            .WithItem(KnownNames.AS, PdfDirectObject.CreateName("MDJ"))
            .AsDictionary();

        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Btn)
            .WithItem(KnownNames.Ff, (int)AcroFieldFlags.Radio)
            .WithItem(KnownNames.T, "Radio Button")
            .WithItem(KnownNames.V, PdfDirectObject.CreateName("JDM"))
            .WithItem(KnownNames.Kids, PdfDirectObject.FromArray(rb1, rb2))
            .AsDictionary();
        
        var reader = await CreatSingleFieldFormAsync(formField);

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        var rb = reader.Fields[0].Should().BeOfType<AcroSingleChoice>().Subject;
        rb.Name.Should().Be("Radio Button");
        rb.Value.Should().Be("/JDM");
        rb.Options.Count.Should().Be(2);
        rb.Options[0].Title.Should().Be("JDM");
        rb.Options[1].Title.Should().Be("MDJ");
        rb.Selected.Should().Be(rb.Options[0]);
    }

    [Fact]
    public async Task ReadSingleChoiceAsync()
    {

        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Ch)
            .WithItem(KnownNames.Ff, 0)
            .WithItem(KnownNames.T, "Choice")
            .WithItem(KnownNames.V, "Opt2Value")
            .WithItem(KnownNames.Opt, PdfDirectObject.FromArray(
                "JDM",
                  PdfDirectObject.FromArray("Opt2Value", "Opt2Display"),
                "MDJ"
                ))
            .AsDictionary();
        
        var reader = await CreatSingleFieldFormAsync(formField);

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        var rb = reader.Fields[0].Should().BeOfType<AcroSingleChoice>().Subject;
        rb.Name.Should().Be("Choice");
        rb.Value.Should().Be("Opt2Value");
        rb.Options.Count.Should().Be(3);
        rb.Options[0].Title.Should().Be("JDM");
        rb.Options[1].Title.Should().Be("Opt2Display");
        rb.Options[1].Value.Should().Be("Opt2Value");
        rb.Options[2].Title.Should().Be("MDJ");
        rb.Selected.Should().Be(rb.Options[1]);
    }
    [Fact]
    public async Task ReadMultiChoiceAsync()
    {

        var formField = new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Annot)
            .WithItem(KnownNames.Subtype, KnownNames.Widget)
            .WithItem(KnownNames.FT, KnownNames.Ch)
            .WithItem(KnownNames.Ff, (int)AcroFieldFlags.MultiSelect)
            .WithItem(KnownNames.T, "Choice")
            .WithItem(KnownNames.V, PdfDirectObject.FromArray("Opt2Value", "MDJ"))
            .WithItem(KnownNames.Opt, PdfDirectObject.FromArray(
                "JDM",
                  PdfDirectObject.FromArray("Opt2Value", "Opt2Display"),
                "MDJ"
                ))
            .AsDictionary();
        
        var reader = await CreatSingleFieldFormAsync(formField);

        reader.Should().NotBeNull();
        reader.Fields.Should().HaveCount(1);
        var rb = reader.Fields[0].Should().BeOfType<AcroMultipleChoice>().Subject;
        rb.Name.Should().Be("Choice");

        rb.Options.Count.Should().Be(3);
        rb.Options[0].Title.Should().Be("JDM");
        rb.Options[1].Title.Should().Be("Opt2Display");
        rb.Options[1].Value.Should().Be("Opt2Value");
        rb.Options[2].Title.Should().Be("MDJ");

        var sel = (await rb.Value.Get<PdfArray>().CastAsync<string>()).Should().HaveCount(2).And.Subject;
        sel.First().Should().Be("Opt2Value");
        sel.Last().Should().Be("MDJ");

        var sel2 = rb.Selected.Should().HaveCount(2).And.Subject;
        sel2.First().Title.Should().Be("Opt2Display");
        sel2.Last().Title.Should().Be("MDJ");
    }
}
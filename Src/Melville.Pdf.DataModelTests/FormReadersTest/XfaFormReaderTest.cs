using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.Pdf.FormReader;
using Melville.Pdf.FormReader.Interface;
using Melville.Pdf.FormReader.XfaForms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.Model.Creators;
using Xunit;

namespace Melville.Pdf.DataModelTests.FormReadersTest;

public class XfaFormReaderTest
{
    private ValueTask<IPdfForm> XfaFormAsync(string templateContent, string dataContent)
    {
        var builder = new PdfDocumentCreator();
        var xfaArray = PdfDirectObject.FromArray(
            "template",
            StringStream(builder, 
                "<template xmlns=\"http://www.xfa.org/schema/xfa-template/2.5/\">\n",
                templateContent, "</template>"),
            "datasets",
            StringStream(builder, 
                "<xfa:datasets xmlns:xfa=\"http://www.xfa.org/schema/xfa-data/1.0/\">\n<xfa:data>\n",
                dataContent, "</xfa:data>\n</xfa:datasets>")
        );
        var acroDict = builder.LowLevelCreator.Add(new DictionaryBuilder()
            .WithItem(KnownNames.XFA, xfaArray)
            .AsDictionary());

        builder.AddToRootDictionary(KnownNames.AcroForm, acroDict);
        var lld = builder.CreateDocument();
        return FormReaderFacade.ReadFormAsync(lld);

    }

    private PdfIndirectObject StringStream(
        PdfDocumentCreator builder, string prefix, string body, string suffix) =>
        builder.LowLevelCreator.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.EmbeddedFile)
            .AsStream($"{prefix}\n{body}\n{suffix}"));

    [Fact]
    public async Task TextBlockAsync()
    {
        var doc = await XfaFormAsync("""
            <field name="FirstField">
                <ui>
                   <textEdit/>
                </ui>
            </field>
            """, """<FirstField>FirstField Value</FirstField>""");

        doc.Fields.Should().HaveCount(1);
        doc.Fields.First().Name.Should().Be("FirstField");
        doc.Fields.First().Should().BeOfType<XfaTextBox>()
            .Subject.StringValue.Should().Be("FirstField Value");
    }

    [Fact]
    public async Task ComoBoxAsync()
    {
        var doc = await XfaFormAsync("""
            <field name="FirstField">
                <ui>
                   <choiceList/>
                </ui>
                <items>
                    <text>A</text>
                    <text>B</text>
                    <text>C</text>
                </items>
            </field>
            """, """<FirstField>A</FirstField>""");

        doc.Fields.Should().HaveCount(1);
        doc.Fields.First().Name.Should().Be("FirstField");
        var pick = doc.Fields.First().Should().BeOfType<XfaSinglePick>()
            .Subject;
        pick.Selected?.Value.DecodedString().Should().Be("A");
        pick.Options[0].Title.Should().Be("A");
        pick.Options[0].Value.DecodedString().Should().Be("A");
        pick.Options[1].Title.Should().Be("B");
        pick.Options[1].Value.DecodedString().Should().Be("B");
        pick.Options[2].Title.Should().Be("C");
        pick.Options[2].Value.DecodedString().Should().Be("C");

    }

    [Fact]
    public async Task ComoBoxAsync2()
    {
        var doc = await XfaFormAsync("""
            <field name="FirstField">
                <ui>
                   <choiceList/>
                </ui>
                <items>
                    <text>A</text>
                    <text>B</text>
                    <text>C</text>
                </items>
                <items save="1">
                    <text>1</text>
                    <text>2</text>
                    <text>3</text>
                </items>
            </field>
            """, """<FirstField>A</FirstField>""");

        doc.Fields.Should().HaveCount(1);
        doc.Fields.First().Name.Should().Be("FirstField");
        var pick = doc.Fields.First().Should().BeOfType<XfaSinglePick>()
            .Subject;
        pick.Selected?.Value.DecodedString().Should().Be("A");
        pick.Options[0].Title.Should().Be("A");
        pick.Options[0].Value.DecodedString().Should().Be("1");
        pick.Options[1].Title.Should().Be("B");
        pick.Options[1].Value.DecodedString().Should().Be("2");
        pick.Options[2].Title.Should().Be("C");
        pick.Options[2].Value.DecodedString().Should().Be("3");

    }

    [Fact]
    public async Task CheckButtonAsync()
    {
        var doc = await XfaFormAsync("""
                                     <field name="FirstField">
                                         <ui>
                                            <checkButton/>
                                         </ui>
                                     </field>
                                     <field name="SecondField">
                                         <ui>
                                            <checkButton/>
                                         </ui>
                                     </field>
                                     """, """
                                          <FirstField>-1</FirstField>
                                          <SecondField>0</SecondField>
                                          """);

        doc.Fields.Should().HaveCount(2);
        VerifyItem(doc.Fields[0], "FirstField", "-1", true);
        VerifyItem(doc.Fields[1], "SecondField", "0", false);


        static void VerifyItem(IPdfFormField control, string name, string textValue, bool boolValue)
        {
            var checkBox = control.Should().BeOfType<XfaCheckBox>()
                .Subject;
            checkBox.Name.Should().Be(name);
            checkBox.GetSingleDataItems().InnerText().Should().Be(textValue);
            checkBox.IsChecked.Should().Be(boolValue);
        }
    }

    [Fact]
    public async Task TextBlockInSubFormAsync()
    {
        var doc = await XfaFormAsync("""
            <subform name = "JdmForm">
            <field name="FirstField">
                <ui>
                   <textEdit/>
                </ui>
            </field>
            </subform>
            """, """<JdmForm><FirstField>FirstField Value</FirstField></JdmForm>""");

        doc.Fields.Should().HaveCount(1);
        var field = doc.Fields.First();
        field.Name.Should().Be("JdmForm.FirstField");
        field.Should().BeOfType<XfaTextBox>()
            .Subject.StringValue.Should().Be("FirstField Value");
    }
}
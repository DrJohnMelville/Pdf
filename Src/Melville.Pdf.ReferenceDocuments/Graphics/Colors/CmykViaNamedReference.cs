namespace Melville.Pdf.ReferenceDocuments.Graphics.Colors
{
    public class CmykViaNamedReference : IccCMYK
    {
        public CmykViaNamedReference() : base("Reference a DeviceCMYK by name in a resource")
        {
        }

        protected override void SetPageProperties(PageCreator page)
        {
            base.SetPageProperties(page);
            page.AddResourceObject(ResourceTypeName.ColorSpace, PdfDirectObject.CreateName("CS1"),
                KnownNames.DeviceCMYK
            );
        }
    }
}
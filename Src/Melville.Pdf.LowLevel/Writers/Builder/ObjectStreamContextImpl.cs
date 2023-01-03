using System;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

internal partial class PdfObjectRegistry
{
    private class ObjectStreamContextImpl : IDisposable
    {
        private readonly PdfObjectRegistry parent;
        private readonly DictionaryBuilder dictionaryBuilder;

        public ObjectStreamContextImpl(PdfObjectRegistry parent, DictionaryBuilder dictionaryBuilder)
        {
            this.parent = parent;
            this.dictionaryBuilder = dictionaryBuilder;
        }

        public void Dispose()
        {
            var capturedBuilder = parent.objectStreamBuilder ??
                                  throw new InvalidOperationException("No parent object stream builder");
            parent.objectStreamBuilder = null;
            if (capturedBuilder.HasValues())
            {
                parent.AddDelayedObject(() => capturedBuilder.CreateStream(dictionaryBuilder));
            }
        }
    }

}
using System;
using System.Linq;
using System.Text;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Model.Objects
{
//    public class NameOrOperatorParent: PdfByteArrayObject
    public class PdfName: PdfByteArrayObject
    {

        protected PdfName(byte[] name, bool creatingKnownName): base(name){}

        public PdfName(byte[] name) : this(name, false)
        {
#if DEBUG
            if (NameAlreadyDefined(name))
                throw new InvalidOperationException("Cannot make a PdfName that matches a name in the KnownNames class.");
            static bool NameAlreadyDefined(byte[] name)
            {
                return KnownNames.LookupName(FnvHash.FnvHashAsUint(name), out var other) &&
                       other.Bytes.Length == name.Length &&
                       other.Bytes.Zip(name, (i, j) => i == j).All(i => i);
            }
#endif
        }
        public PdfName(string s):this(Encoding.UTF8.GetBytes(s)){}
        
        public override string ToString() => "/"+Encoding.UTF8.GetString(Bytes);

        public override T Visit<T>(ILowLevelVisitor<T> visitor) => visitor.Visit(this);
        public virtual PdfName PreferredName() => this;
    }
}
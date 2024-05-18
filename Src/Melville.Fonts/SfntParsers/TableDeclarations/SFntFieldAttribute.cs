namespace Melville.Fonts.SfntParsers.TableDeclarations
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SFntFieldAttribute : Attribute
    {
        public string Count { get; set; }

        public SFntFieldAttribute(string count)
        {
            Count = count;
        }
        public SFntFieldAttribute(): this("") { }
    }
}
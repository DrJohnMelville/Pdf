# Code Generators

Roslyn generators analyze the code being compiled by the C# compiler and produce additional code which is included in the build of the module being compiled.  Generators have the potential not only to reduce boilerplate code, but also to eliminate subtle bugs that arise when code differs only slightly from the boilerplate pattern.  In Melville.Pdf I reused the Melville.INPC.Generators class from prior projects to provide some of the boilerplate code.  Melville.INPC.Generators supports the following generators.

## [AutoNotify]
AutoNotify implements the INotifyPropertyChanged implementation for a class.  For example, given the following class,

````c#
public partial class Foo
{
    [AutoNotify] private bool a;
    [AutoNotify] private string b;
    [AutoNotify] private string C => $"{!A}";
}
````

the following code is generated.

````c#
public partial class Foo: Melville.INPC.IExternalNotifyPropertyChanged 
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    void Melville.INPC.IExternalNotifyPropertyChanged.OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    public bool A
    {
        get => this.a;
        set
        {
            this.a = value;
            ((Melville.INPC.IExternalNotifyPropertyChanged)this).OnPropertyChanged("A");
            ((Melville.INPC.IExternalNotifyPropertyChanged)this).OnPropertyChanged("C");
        }
    }
    public string B
    {
        get => this.b;
        set
        {
            this.b = value;
            ((Melville.INPC.IExternalNotifyPropertyChanged)this).OnPropertyChanged("B");
        }
    }
}
````
The generator has implemented INotifyPropertyChanged, via a private interface that allows the generator to properly implement hierarchies of auto generated types.  Notice that in the case of property C the analyzer inspects the code of the property and correctly infers that changes to property A might also change property C. 

If any of the following methods exist in the class containing the generated property they will be called at appropriate points in the generated code.

| Method                                        | Effect                             |
|-----------------------------------------------|------------------------------------|
| bool AGetFilter(bool val)                     | Called in the get accessor for A   |
 | bool ASetFilter(bool val)                     | Called in the A setter prior to storing the value in the backing field |
| bool OnAChanged()                             | Called when the value of A is set  |
| bool OnAChanged(bool newValue)                | Called when the value of A is set  |
| bool OnAChanged(bool oldValue, bool newValue) | Called when the value of A is set  |

## StaticSingleton
This is a very simple generator.  Frequently I use classes that have no state. and exist only to provide an implementation to a given interface.  This generator creates a static singleton member and a private
constructor to ensure than only one instance of the class exists.  For example:

````c#
[StaticSingleton()]
public partial class Foo
{
    
}
````
generates
````c#
public partial class Foo 
{
    public static readonly Foo Instance = new();
    private Foo() {}
}
````

You can name the generated field by passing the name to the StaticSingleton attribute. 

## FromConstructor
90% of constructors simpley store their parameters into fields or properties.  This leads to a lot of boilerplate constructors, and changeing the type of a field makes you change the constructor as well.  The FromConstructor generates the constructors automatically.  Given the declarations:

````c#
public class Base
{
    public Base(string s) { }

}
public partial class Impl:Base
{
    [FromConstructor] private readonly int a;
    [FromConstructor] public float B { get; set; }
}
````
The following code is generated:
````c#
public partial class Impl 
{
    public Impl(string s, int a, float b): base(s)
    {
        this.a = a;
        this.B = b;
        OnConstructed();
    }
    partial void OnConstructed();
}
````
The generator found the constructor on the base class and all the FromConstructor members of the current class and generated the appropriate constructor.  If the base class has multiple constructors, the child class will have multiple corresponding constructors.  A class that does not declare fields of its own but would like "inherited" constructors can just decorate the class declaration itself with the [FromConstructor] attribute.

The generator also generates a partial OnConstructed method that is called after all the fields are initialized.  If this partial method is not implemented in another part of the class, the call will be eliminated by the compiler.

## DelegateTo
In object oriented design we learn that there are two ways to reuse and modify an implementation.  We can inherit from a base class, or we can contain a "base" class inside a "child" and delegate operations to the contained class.  Every object-oriented language, by definition, has syntax for the inheritance, but few have specific syntax support for containment and delegation.  When applied to a field, property, or method with no arguments, DelegateTo generates forwarding methods to delegate all possible operations to that member.  For example, these declarations:

````c#
public interface IParent
{
    void OpA();
    double OpB(int x, string y);
    void OpC(DateTime x);
}

public partial class Impl : IParent
{
    [DelegateTo] private IParent Target() => null!;
    public void OpC(DateTime x) => throw new NotImplementedException();
}
````
cause the following code to be generated.
````c#
public partial class Impl 
{
    public void OpA() => this.Target().OpA();
    public double OpB(int x, string y) => this.Target().OpB(x, y);
}
````
Notice that the generator recognizes that the class already has an implementation of OpC and does not generate a delegating member for OpC.  (The effect of the code above is to throw an exception for every operation defined in IParent.  A more interesting implementation of Target() would lead to a more useful class.)

## GenerateDp 
The WPF framework makes extensive use of dependency properties -- enriched properties that support change notification, visual tree inheritance, binding, and other useful properties.  Defining dependency properties, however, requires significant boilerplate code.  GenerateDP allows the elimination of much of the boilerplate.  For example:

````c#
[GenerateDP(typeof(int), "IntProp", Attached = true, Default = 10, Nullable = true)]
public partial class Impl 
{
}
````
generates
````c#
public partial class Impl 
{
    // IntProp Dependency Property Implementation
    public static readonly System.Windows.DependencyProperty IntPropProperty = 
        System.Windows.DependencyProperty.RegisterAttached(
        "IntProp", typeof(int?), typeof(Melville.Generators.INPC.Test.IntegrationTests.Impl),
        new System.Windows.FrameworkPropertyMetadata(10));
    
    public static int? GetIntProp(System.Windows.DependencyObject obj) =>
        (int?)obj.GetValue(Melville.Generators.INPC.Test.IntegrationTests.Impl.IntPropProperty);
    public static void SetIntProp(System.Windows.DependencyObject obj, int? value) =>
        obj.SetValue(Melville.Generators.INPC.Test.IntegrationTests.Impl.IntPropProperty, value);
    
}
````

Alternatively, dependency properties can be defined based on other code elements such as:
````c#
public partial class Impl: DependencyObject 
{
    [GenerateDP]
    private void OnIntPropChanged(int newValue) { }
}
````

which generates
````c#
public partial class Impl 
{
    // IntProp Dependency Property Implementation
    public static readonly System.Windows.DependencyProperty IntPropProperty = 
        System.Windows.DependencyProperty.Register(
        "IntProp", typeof(int), typeof(Melville.Generators.INPC.Test.IntegrationTests.Impl),
        new System.Windows.FrameworkPropertyMetadata(default(int), 
            (i,j)=>((Melville.Generators.INPC.Test.IntegrationTests.Impl)i).OnIntPropChanged((int)(j.NewValue))));
    
    public int IntProp
    {
        get => (int)this.GetValue(Melville.Generators.INPC.Test.IntegrationTests.Impl.IntPropProperty);
        set => this.SetValue(Melville.Generators.INPC.Test.IntegrationTests.Impl.IntPropProperty, value);
    }
    
}
````
Here the property name is derived from the OnXXXChanged naming convention and the type from the type of the first argument to the method.  Other code elements that the attribute could be attached to include: 
````c#
public partial class Impl: DependencyObject 
{
    [GenerateDP] 
    private void OnPropAChanged(int oldValue, int newValue) { }
    [GenerateDP(typeof(int))] 
    private static void OnPropBChanged(Impl target, DependencyPropertyChangedEventArgs args) { }
    [GenerateDP(typeof(int))] 
    private static void OnPropCChanged(DependencyObject target, DependencyPropertyChangedEventArgs args) { }
    [GenerateDP()] 
    private static void OnPropDChanged(DependencyObject target, double newValue) { }
    [GenerateDP()] 
    private static void OnPropEChanged(DependencyObject target, double oldValue, double newValue) { }
    [GenerateDP(typeof(string))] 
    private static void OnPropFChanged() { }
    [GenerateDP]
    public static DependencyProperty NovelProperty =
        System.Windows.DependencyProperty.RegisterAttached("Novel", typeof(int), typeof(Impl), new FrameworkPropertyMetadata(0));
}
````

## MacroItem / MacroCode
This generator is inspired by the C macro system.  (I know, much maligned, but I really liked it.)  The macro generator is useful when defining types that are essentially strongly typed property bags.  The system is also used in places to declare common operations over multiple number types, but this may go away as I adopt C# 11.  A "Property Bag" example includes the GraphicsState class, which defines 26 properties all of which have default values, but must be copied individually for the clone operation.  The following code:

````c#
public abstract partial  class GraphicsState: IGraphicsState, IDisposable
{
    [MacroItem("Matrix3x2", "TransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "InitialTransformMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "TextMatrix", "Matrix3x2.Identity")]
    [MacroItem("Matrix3x2", "TextLineMatrix", "Matrix3x2.Identity")]
    [MacroItem("double", "LineWidth", "1.0")]
    [MacroItem("double", "MiterLimit", "10.0")]
    [MacroItem("LineJoinStyle", "LineJoinStyle", "LineJoinStyle.Miter")]
    [MacroItem("LineCap", "LineCap", "LineCap.Butt")]
    [MacroItem("double", "DashPhase", "0.0")]
    [MacroItem("double", "FlatnessTolerance", "0.0")]
    [MacroItem("double[]", "DashArray", "Array.Empty<double>()")]
    [MacroItem("RenderIntentName", "RenderIntent", "RenderIntentName.RelativeColoriMetric")]
    [MacroItem("IColorSpace", "StrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("IColorSpace", "NonstrokeColorSpace", "DeviceGray.Instance")]
    [MacroItem("DeviceColor", "StrokeColor", "DeviceColor.Black")]
    [MacroItem("DeviceColor", "NonstrokeColor", "DeviceColor.Black")]

    // Text Properties
    [MacroItem("double", "CharacterSpacing", "0.0")]
    [MacroItem("double", "WordSpacing", "0.0")]
    [MacroItem("double", "TextLeading", "0.0")]
    [MacroItem("double", "TextRise", "0.0")]
    [MacroItem("double", "HorizontalTextScale", "100.0")]
    [MacroItem("TextRendering", "TextRender", "TextRendering.Fill")]
    [MacroItem("IRealizedFont", "Typeface", "NullRealizedFont.Instance")]
    [MacroItem("double", "FontSize", "0.0")]
    
    //PageSizes
    [MacroItem("double", "PageWidth", "1")]
    [MacroItem("double", "PageHeight", "1")]

    // code
    [MacroCode("public ~0~ ~1~ {get; private set;} = ~2~;")]
    [MacroCode("    ~1~ = other.~1~;", Prefix = "public virtual void CopyFrom(GraphicsState other){", Postfix = "}")]
    public void SaveGraphicsState() { }
}
````
generates all the declarations and copy operations.
````c#
public abstract partial class GraphicsState 
{
    public Matrix3x2 TransformMatrix {get; private set;} = Matrix3x2.Identity;
    public Matrix3x2 InitialTransformMatrix {get; private set;} = Matrix3x2.Identity;
    public Matrix3x2 TextMatrix {get; private set;} = Matrix3x2.Identity;
    public Matrix3x2 TextLineMatrix {get; private set;} = Matrix3x2.Identity;
    public double LineWidth {get; private set;} = 1.0;
    public double MiterLimit {get; private set;} = 10.0;
    public LineJoinStyle LineJoinStyle {get; private set;} = LineJoinStyle.Miter;
    public LineCap LineCap {get; private set;} = LineCap.Butt;
    public double DashPhase {get; private set;} = 0.0;
    public double FlatnessTolerance {get; private set;} = 0.0;
    public double[] DashArray {get; private set;} = Array.Empty<double>();
    public RenderIntentName RenderIntent {get; private set;} = RenderIntentName.RelativeColoriMetric;
    public IColorSpace StrokeColorSpace {get; private set;} = DeviceGray.Instance;
    public IColorSpace NonstrokeColorSpace {get; private set;} = DeviceGray.Instance;
    public DeviceColor StrokeColor {get; private set;} = DeviceColor.Black;
    public DeviceColor NonstrokeColor {get; private set;} = DeviceColor.Black;
    public double CharacterSpacing {get; private set;} = 0.0;
    public double WordSpacing {get; private set;} = 0.0;
    public double TextLeading {get; private set;} = 0.0;
    public double TextRise {get; private set;} = 0.0;
    public double HorizontalTextScale {get; private set;} = 100.0;
    public TextRendering TextRender {get; private set;} = TextRendering.Fill;
    public IRealizedFont Typeface {get; private set;} = NullRealizedFont.Instance;
    public double FontSize {get; private set;} = 0.0;
    public double PageWidth {get; private set;} = 1;
    public double PageHeight {get; private set;} = 1;
    public virtual void CopyFrom(GraphicsState other){
        TransformMatrix = other.TransformMatrix;
        InitialTransformMatrix = other.InitialTransformMatrix;
        TextMatrix = other.TextMatrix;
        TextLineMatrix = other.TextLineMatrix;
        LineWidth = other.LineWidth;
        MiterLimit = other.MiterLimit;
        LineJoinStyle = other.LineJoinStyle;
        LineCap = other.LineCap;
        DashPhase = other.DashPhase;
        FlatnessTolerance = other.FlatnessTolerance;
        DashArray = other.DashArray;
        RenderIntent = other.RenderIntent;
        StrokeColorSpace = other.StrokeColorSpace;
        NonstrokeColorSpace = other.NonstrokeColorSpace;
        StrokeColor = other.StrokeColor;
        NonstrokeColor = other.NonstrokeColor;
        CharacterSpacing = other.CharacterSpacing;
        WordSpacing = other.WordSpacing;
        TextLeading = other.TextLeading;
        TextRise = other.TextRise;
        HorizontalTextScale = other.HorizontalTextScale;
        TextRender = other.TextRender;
        Typeface = other.Typeface;
        FontSize = other.FontSize;
        PageWidth = other.PageWidth;
        PageHeight = other.PageHeight;
    }
}
````
Notice that [MacroItems] define items that are fed into the [MacroCode] instances.  Within the [MacroCode] parameter ```````~0~, ~1~, ~2~,``````` and etc are replaced with the zero based n'th argument to MacroItem.  Notice that each [MacroCode] interacts with each [MacroItem] which allows both the definitions and the CopyFrom method to be created from the same set of [MarcoItem]s.

The code generation algorithm (in pseudocode) might look something like

````
foreach codeItem in MacroCodes
     Emit codeItem.Prefix
     foreach innerItem in MacroItems
          Substitute parameters into codeItem and emit the result
      Emit codeItem.PostFix
````

Each member of a class which might contain [MacroItem] and [MacroCode] attributes is considered independently, so a class could have multiple groups of generated members.  Unlike the other generators, however, The [MacroItem] and [MacroCode] properties do not inspect the members that they happen to decorate.
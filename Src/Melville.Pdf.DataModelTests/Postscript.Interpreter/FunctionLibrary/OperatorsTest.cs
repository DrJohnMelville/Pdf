using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.FunctionLibrary;

public class OperatorsTest
{
    [Theory]
    [InlineData("true", "01: true")]
    [InlineData("false", "01: false")]
    [InlineData("null", "01: null")]
    public void TestSystemTokens(string code, string result) => 
        RunTestOn(code, result, new PostscriptEngine(
            PostscriptOperatorCollections.Empty().WithSystemTokens()));

    [Theory]
    [InlineData("4 5 pop", "01: 4")]
    [InlineData("4 5 exch", "01: 4\r\n02: 5")]
    [InlineData("4 dup", "01: 4\r\n02: 4")]
    [InlineData("1 2 3 2 copy", """
        01: 3
        02: 2
        03: 3
        04: 2
        05: 1
        """)]
    [InlineData("1 4 3 1 index", """
        01: 4
        02: 3
        03: 4
        04: 1
        """)]
    [InlineData("1 2 3 4 5   3 1 roll", """
        01: 4
        02: 3
        03: 5
        04: 2
        05: 1
        """)]
    [InlineData("1 2 3 4 5   3 2 roll", """
        01: 3
        02: 5
        03: 4
        04: 2
        05: 1
        """)]
    [InlineData("1 2 3 4 5   3 -1 roll", """
        01: 3
        02: 5
        03: 4
        04: 2
        05: 1
        """)]
    [InlineData("1 2 3 4 5 3 -1 clear", "")]
    [InlineData("1 2 3 4 5 count", """
        01: 5
        02: 5
        03: 4
        04: 3
        05: 2
        06: 1
        """)]
    [InlineData("1 mark", """
        01: <Mark Object>
        02: 1
        """)]
    [InlineData("1 mark 2 3 4 counttomark", """
        01: 3
        02: 4
        03: 3
        04: 2
        05: <Mark Object>
        06: 1
        """)]
    [InlineData("1 mark 2 3 4 cleartomark", "01: 1")]
    public void WithStackOperators(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(
            PostscriptOperatorCollections.Empty().WithStackOperators()));

    [Theory]
    [InlineData("1 2 add", "01: 3")]
    [InlineData("1.5 2 add", "01: 3.5")]
    [InlineData("1 2.5 add", "01: 3.5")]
    [InlineData("1.25 2.25 add", "01: 3.5")]
    [InlineData("8 4 div", "01: 2")]
    [InlineData("9 4 div", "01: 2.25")]
    [InlineData("8 4 idiv", "01: 2")]
    [InlineData("9 4 idiv", "01: 2")]
    [InlineData("9 4 mul", "01: 36")]
    [InlineData("9 4 mod", "01: 1")]
    [InlineData("9 4 sub", "01: 5")]
    [InlineData("9 abs", "01: 9")]
    [InlineData("9.5 abs", "01: 9.5")]
    [InlineData("-9 abs", "01: 9")]
    [InlineData("-9.5 abs", "01: 9.5")]
    [InlineData("7 neg", "01: -7")]
    [InlineData("-7 neg", "01: 7")]
    [InlineData("9.1 ceiling", "01: 10")]
    [InlineData("-9.1 ceiling", "01: -9")]
    [InlineData("9.1 floor", "01: 9")]
    [InlineData("-9.1 floor", "01: -10")]
    [InlineData("9.4 round", "01: 9")]
    [InlineData("9.5 round", "01: 10")]
    [InlineData("9.5 truncate", "01: 9")]
    [InlineData("-9.5 truncate", "01: -9")]
    [InlineData("9 sqrt", "01: 3")]
    [InlineData("10 10 atan", "01: 45")]
    [InlineData("0 sin", "01: 0")]
    [InlineData("90 sin", "01: 1")]
    [InlineData("90 cos", "01: 6.123233995736766E-17")]
    [InlineData("0 cos", "01: 1")]
    [InlineData("10 2 exp", "01: 100")]
    [InlineData("100 ln", "01: 4.605170185988092")]
    [InlineData("234 srand rand rand rrand", """
        01: 1927566503
        02: 1927566503
        03: 11295414
        """)]
    public void WithMathOperators(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(
            PostscriptOperatorCollections.Empty().WithMathOperators()));

    [Theory]
    [InlineData("3 string dup currentfile exch readstring JDM", "01: true\r\n02: (JDM)\r\n03: (JDM)")]
    [InlineData("3 string dup currentfile exch readstring JD", "01: false\r\n02: (JD)\r\n03: (JD\u0000)")]
    [InlineData("3 string dup currentfile exch readstring ", "01: false\r\n02: ()\r\n03: (\u0000\u0000\u0000)")]
    [InlineData("3 string dup currentfile exch readstring", "01: false\r\n02: ()\r\n03: (\u0000\u0000\u0000)")]
    [InlineData("2 string dup 0 102 put dup 1 103 put", "01: (fg)")]
    [InlineData("(Hello) length", "01: 5")]
    [InlineData("(Hello) 1 get", "01: 101")]
    [InlineData("(Hello) dup 1 102 put", "01: (Hfllo)")]
    [InlineData("(Hello)  1 3 getinterval", "01: (ell)")]
    [InlineData("(Hello) dup 1 (ome) putinterval", "01: (Homeo)")]
    [InlineData("(Hello) dup (Oreo) exch copy", "01: (Oreo)\r\n02: (Oreoo)")]
    [InlineData("(Abc) {} forall", "01: 99\r\n02: 98\r\n03: 65")]
    [InlineData("(baabc) (ba) anchorsearch", "01: true\r\n02: (ba)\r\n03: (abc)")]
    [InlineData("(baabc) (Aa) anchorsearch", "01: false\r\n02: (baabc)")]
    [InlineData("(baabc) (ba) search", "01: true\r\n02: ()\r\n03: (ba)\r\n04: (abc)")]
    [InlineData("(baabc) (Aa) search", "01: false\r\n02: (baabc)")]
    [InlineData("(Hellobaabc) (ba) search", "01: true\r\n02: (Hello)\r\n03: (ba)\r\n04: (abc)")]
    [InlineData("(Hellobaabc) (Aa) search", "01: false\r\n02: (Hellobaabc)")]
    [InlineData("(123 x y) token", "01: true\r\n02: 123\r\n03: ( x y)")]
    [InlineData("(123) token", "01: true\r\n02: 123\r\n03: ()")]
    [InlineData("((\\()) token", "01: false")]
    public void WithStringOperators(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(
            PostscriptOperatorCollections.Empty().WithBaseLanguage()));
    
    [Theory]
    [InlineData("1 2 3 2 packedarray", "01: [2 3]\r\n02: 1")]
    [InlineData("true setpacking currentpacking", "01: true")]
    [InlineData("currentpacking", "01: false")]
    [InlineData("3 array", "01: [null null null]")]
    [InlineData("23[1    2 \r\n 3]", "01: [1 2 3]\r\n02: 23")]
    [InlineData("[1 2 3] length", "01: 3")]
    [InlineData("[1 2 3] 1 get ", "01: 2")]
    [InlineData("[1 2 3] dup 2 /true put", "01: [1 2 /true]")]
    [InlineData("[1 2 3] 1 2 getinterval", "01: [2 3]")]
    [InlineData("[1 2 3] dup 1 [8 9] putinterval", "01: [1 8 9]")]
    [InlineData("1 2 3 4 count array astore", "01: [1 2 3 4]")]
    [InlineData("1 [2 3 4 5] aload ", """
        01: [2 3 4 5]
        02: 5
        03: 4
        04: 3
        05: 2
        06: 1
        """)]
    [InlineData("[1 2 3] 5 array dup 3 1 roll copy", """
        01: [1 2 3]
        02: [1 2 3 null null]
        """)]
    public void WithArrayOperators(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(PostscriptOperatorCollections.Empty()
            .WithStackOperators().WithArrayOperators().WithSystemTokens()));

    [Theory]
    [InlineData("/add cvx", "01: add")]
    [InlineData("/add cvx cvlit", "01: /add")]
    [InlineData("5 2 /mul cvx exec", "01: 10")]
    [InlineData("5 xcheck", "01: false")]
    [InlineData("(string) xcheck", "01: false")]
    [InlineData("1.2 xcheck", "01: false")]
    [InlineData("true xcheck", "01: false")]
    [InlineData("/Name xcheck", "01: false")]
    [InlineData("/Name cvx xcheck", "01: true")]
    [InlineData("{add} xcheck", "01: true")]
    [InlineData("1 executeonly", "01: 1")]
    [InlineData("1 readonly", "01: 1")]
    [InlineData("1 noaccess", "01: 1")]
    [InlineData("1 rcheck", "01: true")]
    [InlineData("1 wcheck", "01: true")]
    [InlineData("123.4 cvi", "01: 123")]
    [InlineData("123 cvr", "01: 123")]
    [InlineData("(123.4) cvi", "01: 123")]
    [InlineData("(Hello) cvn", "01: /Hello")]
    [InlineData("(Hello) cvx cvn", "01: Hello")]
    [InlineData("/Hello cvs", "01: (/Hello)")]
    [InlineData("(TESTING) dup 123 10 3 -1 roll cvrs", "01: (123)\r\n02: (123TING)")]
    [InlineData("(TESTING) dup -123 10 3 -1 roll cvrs", "01: (-123)\r\n02: (-123ING)")]
    [InlineData("(TESTING) dup 123.4 10 3 -1 roll cvrs", "01: (123.4)\r\n02: (123.4NG)")]
    [InlineData("(TESTING) dup 123 16 3 -1 roll cvrs", "01: (7B)\r\n02: (7BSTING)")]
    [InlineData("(TESTINGX) dup -123 16 3 -1 roll cvrs", "01: (FFFFFF85)\r\n02: (FFFFFF85)")]
    [InlineData("(TESTING) dup 123.4 16 3 -1 roll cvrs", "01: (7B)\r\n02: (7BSTING)")]
    [InlineData("(TESTING) dup 0 16 3 -1 roll cvrs", "01: (0)\r\n02: (0ESTING)")]
    [InlineData("0 [1 2 3] {add} forall", "01: 6")]
    public void ExecutionAndConversion(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(PostscriptOperatorCollections.Empty()
            .WithConversionOperators().WithControlOperators().WithMathOperators()
            .WithSystemTokens().WithArrayOperators().WithStackOperators()));

    [Theory]
    [InlineData("10 exec", "01: 10")]
    [InlineData("/start cvx start", "01: start")]
    [InlineData("{2 3 add} exec", "01: 5")]
    [InlineData("2 { 3 add} exec", "01: 5")]
    [InlineData("(2 3 add) cvx exec", "01: 5")]
    [InlineData("true {1} if", "01: 1")]
    [InlineData("false {1} if", "")]
    [InlineData("true {1} {2} ifelse", "01: 1")]
    [InlineData("false {1} {2} ifelse", "01: 2")]
    [InlineData("0 1 1 4 {add} for", "01: 10")]
    [InlineData("1 2 6 {} for", "01: 5\r\n02: 3\r\n03: 1")]
    [InlineData("3 -.5 1 { } for", "01: 1\r\n02: 1.5\r\n03: 2\r\n04: 2.5\r\n05: 3")]
    [InlineData("4 {1} repeat", "01: 1\r\n02: 1\r\n03: 1\r\n04: 1")]
    [InlineData("4 {1 exit} repeat", "01: 1")]
    [InlineData("{ [1] { exit} forall 2 exit} loop 3", "01: 3\r\n02: 2\r\n03: 1")]
    [InlineData("{ 1 {1 exit} repeat 2 exit} loop 3", "01: 3\r\n02: 2\r\n03: 1")]
    [InlineData("{ 1 1 1 { exit} for 2 exit} loop 3", "01: 3\r\n02: 2\r\n03: 1")]
    [InlineData("4 true false false false { {exit} if} loop", "01: 4")]
    [InlineData("1 {4 stop 5} stopped 6", "01: 6\r\n02: true\r\n03: 4\r\n04: 1")]
    [InlineData("1 {4 5} stopped 6", "01: 6\r\n02: false\r\n03: 5\r\n04: 4\r\n05: 1")]
    [InlineData("{countexecstack 1 pop} exec 1 pop", "01: 2")]
    [InlineData("{countexecstack array execstack stop} stopped 1 pop",
        "01: true\r\n02: [Synchronous CodeSource Stop Context [countexecstack array execstack stop]]")]

    [InlineData("1 { 2 { 3 quit} exec 4 } exec 5", "01: 3\r\n02: 2\r\n03: 1")]
    public void ControlOperators(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(PostscriptOperatorCollections.Empty()
            .WithConversionOperators().WithControlOperators().WithMathOperators()
            .WithSystemTokens().WithArrayOperators().WithStackOperators()));

    [Theory]
    [InlineData("1 1 ne", "01: false")]
    [InlineData("1 2 ne", "01: true")]
    [InlineData("1 1 eq", "01: true")]
    [InlineData("1.0 1 eq", "01: true")]
    [InlineData("1.1 1 eq", "01: false")]
    [InlineData("1 2 eq", "01: false")]
    [InlineData("/H1 /H1 eq", "01: true")]
    [InlineData("/H1 /H2 eq", "01: false")]
    [InlineData("(H1) dup pop /H1 eq", "01: true")]
    [InlineData("/H1 /H1 cvx eq", "01: true")]
    [InlineData("[1 2 3] dup eq", "01: true")]
    [InlineData("[1 2 3] [1 2 3] eq", "01: false")]
    [InlineData("true not", "01: false")]
    [InlineData("false not", "01: true")]
    [InlineData("0 not", "01: -1")]
    [InlineData("2 not", "01: -3")]

    [InlineData("2 1 ge", "01: true")]
    [InlineData("1 1 ge", "01: true")]
    [InlineData("0 1 ge", "01: false")]
    [InlineData("2 1 gt", "01: true")]
    [InlineData("1 1 gt", "01: false")]
    [InlineData("0 1 gt", "01: false")]
    [InlineData("2 1 le", "01: false")]
    [InlineData("1 1 le", "01: true")]
    [InlineData("0 1 le", "01: true")]
    [InlineData("2 1 lt", "01: false")]
    [InlineData("1 1 lt", "01: false")]
    [InlineData("0 1 lt", "01: true")]
    [InlineData("1.1 1 gt", "01: true")]
    [InlineData("/B /A gt", "01: true")]

    [InlineData("3 5 and", "01: 1")]
    [InlineData("false false and", "01: false")]
    [InlineData("false true and", "01: false")]
    [InlineData("true false and", "01: false")]
    [InlineData("true true and", "01: true")]
    [InlineData("3 5 or", "01: 7")]
    [InlineData("false false or", "01: false")]
    [InlineData("false true or", "01: true")]
    [InlineData("true false or", "01: true")]
    [InlineData("true true or", "01: true")]
    [InlineData("3 5 xor", "01: 6")]
    [InlineData("false false xor", "01: false")]
    [InlineData("false true xor", "01: true")]
    [InlineData("true false xor", "01: true")]
    [InlineData("true true xor", "01: false")]

    [InlineData("4 2 bitshift", "01: 16")]
    [InlineData("4 -2 bitshift", "01: 1")]
    public void RelationalAndBitwise(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(PostscriptOperatorCollections.Empty()
            .WithRelationalAndBitwiseOperators()
            .WithSystemTokens()
            .WithStackOperators()
            .WithArrayOperators()
            .WithConversionOperators()));

    [Theory]
    [InlineData("10 dict", "01: <<\r\n>>")]
    [InlineData("10 dict length", "01: 0")]
    [InlineData("10 dict maxlength", "01: 10")]
    [InlineData("25 dict length", "01: 0")]
    [InlineData("25 dict maxlength", "01: 0")]
    [InlineData("2 dict dup begin 1 1 5 {cvs cvn /Hi def} for end length", "01: 5")]
    [InlineData("<</A 1 /B 2>>", "01: <<\r\n    /A: 1\r\n    /B: 2\r\n>>")]
    [InlineData("/A 1 def /A load /A load", "01: 1\r\n02: 1")]
    [InlineData("/A 1 def 10 dict begin /A load /A 2 def /A load", "01: 2\r\n02: 1")]
    [InlineData("/A 1 def 10 dict begin /A 2 def /A load end /A load ", "01: 1\r\n02: 2")]
    [InlineData("5 dict begin /A 10 def A", "01: 10")]
    [InlineData("/A 5 def 5 dict begin /A 10 store A /A 12 def A end A", "01: 10\r\n02: 12\r\n03: 10")]
    [InlineData("5 dict dup /A 12 put /A get", "01: 12")]
    [InlineData("5 dict dup /A 12 put /A known", "01: true")]
    [InlineData("5 dict dup /A 12 put /B known", "01: false")]
    [InlineData("5 dict dup /A 12 put dup /A undef /A known", "01: false")]
    [InlineData("25 dict dup /A 12 put dup /A undef /A known", "01: false")]
    [InlineData("/NotFound where", "01: false")]
    [InlineData("5 dict begin /A 10 def /A where", "01: true\r\n02: <<\r\n    /A: 10\r\n>>")]
    [InlineData("<</A 10 /B 20>> <</B 15 /C 30>> copy", "01: <<\r\n    /B: 20\r\n    /C: 30\r\n    /A: 10\r\n>>")]
    [InlineData("25 dict dup /A 10 put dup /B 20 put <</B 15 /C 30>> copy", "01: <<\r\n    /B: 20\r\n    /C: 30\r\n    /A: 10\r\n>>")]
    [InlineData("0 <</A 10 /B 20 /C 30>> {exch pop add} forall", "01: 60")]
    [InlineData("5 dict begin /A 3 def currentdict", "01: <<\r\n    /A: 3\r\n>>")]
    [InlineData("systemdict /A 1 put A", "01: 1")]
    [InlineData("systemdict /A 1 put globaldict /A 2 put A", "01: 2")]
    [InlineData("systemdict /A 1 put globaldict /A 2 put userdict /A 3 put A", "01: 3")]
    [InlineData("errordict", "01: <<\r\n>>")]
    [InlineData("$error", "01: <<\r\n>>")]
    [InlineData("statusdict", "01: <<\r\n>>")]
    [InlineData("countdictstack", "01: 3")]
    [InlineData("10 dict begin countdictstack", "01: 4")]
    [InlineData("10 dict begin countdictstack array dictstack [ exch { length } forall ]",
        "01: [111 0 0 0]")]
    [InlineData("10 dict begin 25 dict begin countdictstack cleardictstack countdictstack",
        "01: 3\r\n02: 5")]
    [InlineData("/Meth {dup 100000 ge {exit} {1 add Meth} ifelse} def 1 Meth", "01: 100000")]
    public void Dictionary(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(PostscriptOperatorCollections.Empty()
            .WithSystemTokens()
            .WithArrayOperators()
            .WithControlOperators()
            .WithDictionaryOperators()
            .WithStackOperators()
            .WithMathOperators()
            .WithRelationalAndBitwiseOperators()
            .WithConversionOperators()));

    [Theory]
    [InlineData("/Key 12345 /Cat defineresource", "01: 12345")]
    [InlineData("/Key 12345 /Cat defineresource pop /Key /Cat findresource", "01: 12345")]
    [InlineData("/Key 12345 /Cat defineresource pop /Key /Cat undefineresource /Key /Cat resourcestatus", 
        "01: false")]
    [InlineData("/Key 12345 /Cat defineresource pop /Key /Cat resourcestatus", 
        "01: true\r\n02: -1\r\n03: 1")]
    public void Reources(string code, string result) =>
        RunTestOn(code, result, new PostscriptEngine(PostscriptOperatorCollections.Empty()
            .WithSystemTokens()
            .WithResourceOperators()
            .WithStackOperators()));

    private static void RunTestOn(string code, string result, PostscriptEngine engine)
    {
        engine.Execute(code);
        Assert.Equal(result, engine.OperandStack.ToString());
    }
}
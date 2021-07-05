using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
  public static class AAssert
  {
    /// <summary>Do not call this method.</summary>
    [Obsolete("This is an override of Object.Equals(). Call Assert.Equal() instead.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new static bool Equals(object a, object b) =>
      throw new InvalidOperationException("Assert.Equals should not be used");

    /// <summary>Do not call this method.</summary>
    [Obsolete("This is an override of Object.ReferenceEquals(). Call Assert.Same() instead.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new static bool ReferenceEquals(object a, object b) =>
      throw new InvalidOperationException("Assert.ReferenceEquals should not be used");

    public static void False([DoesNotReturnIf(true)]bool condition) => Assert.False(condition);

    public static void False([DoesNotReturnIf(true)]bool? condition) => Assert.False(condition);

    public static void False([DoesNotReturnIf(true)]bool condition, string userMessage) => 
      Assert.False(condition, userMessage);

    public static void False([DoesNotReturnIf(true)] bool? condition, string userMessage) =>
      Assert.False(condition, userMessage);

    public static void True([DoesNotReturnIf(false)]bool condition) => Assert.True(new bool?(condition));

    public static void True([DoesNotReturnIf(false)]bool? condition) => Assert.True(condition);

    public static void True([DoesNotReturnIf(false)]bool condition, string userMessage) => 
      Assert.True(condition, userMessage);

    public static void True([DoesNotReturnIf(false)] bool? condition, string userMessage) =>
      Assert.True(condition, userMessage);

    [DoesNotReturn]
    public static void Fail(string message) => AAssert.True(false, message);
  }
}
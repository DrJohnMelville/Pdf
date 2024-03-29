﻿using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public abstract class ParserTest
{
    protected Mock<IContentStreamOperations> Target { get; } = new();

    protected ValueTask ParseStringAsync(
        string s, IContentStreamOperations? target = null)
    {
        var sut = new ContentStreamParser(target ?? Target.Object);
        return sut.ParseAsync(PipeReaderFromString(s));
    }

    private static PipeReader PipeReaderFromString(string s) =>
        PipeReader.Create(new MemoryStream(s.AsExtendedAsciiBytes()));

    protected async Task TestInputAsync(
        string input, params Expression<Action<IContentStreamOperations>>[] actions)
    {
        await ParseStringAsync(input);
        foreach (var action in actions)
        {
            Target.Verify(action);
        }
        Target.VerifyNoOtherCalls();
    }

    protected async Task TestInputAsync(
        string input, MockBase mock)
    {
        await ParseStringAsync(input, (IContentStreamOperations)mock);
        mock.Verify();
    }

    public class MockBase
    {
        private bool called = false;
        protected void SetCalled() => called = true;
        public void Verify() => Assert.True(called);
    }

}
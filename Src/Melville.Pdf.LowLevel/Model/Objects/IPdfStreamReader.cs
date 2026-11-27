using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// Represents the byte content from a PDF Stream object.  PDF streams content all need to
/// ne disposed because they rent from the main PDF reader.  Many filters use parsing buffers that
/// also need to be disposed.  
/// </summary>
public interface IPdfStreamReader : IAsyncReader, ISyncReader, IDisposable
{
}
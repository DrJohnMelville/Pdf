using System;
using System.Collections.Generic;
using Melville.FileSystem;
using Melville.MVVM.Wpf.MvvmDialogs;

namespace Melville.Pdf.LowLevelReader.CompositionRoot
{
    internal class FakeOpenAdapter : IOpenSaveFile
    {
        private readonly string path;
        public FakeOpenAdapter(string path)
        {
            this.path = path;
        }

        public IDirectory? GetDirectory(string? dir = null) => throw new NotImplementedException();

        public string? GetDirectoryString(string? dir = null) => throw new NotImplementedException();

        public IFile? GetSaveFile(
            IDirectory? defaultDirectory, string ext, string filter, string title, string? name = null) => 
            throw new NotImplementedException();

        public string? GetSaveFileName(
            string? defaultDir, string ext, string filter, string title, string? name = null) => 
            throw new NotImplementedException();

        public string? GetLoadFileName(string? defaultDir, string ext, string filter, string title) => path;

        public IFile? GetLoadFile(IDirectory? defaultDir, string ext, string filter, string title) => 
            new FileSystemFile(path);

        public IEnumerable<IFile> GetLoadFiles(
            IDirectory? defaultDir, string ext, string filter, string title, bool oneFileOnly = false) =>
            new IFile[] {new FileSystemFile(path)};

        public IEnumerable<string> GetLoadFileNames(
            string? defaultDir, string ext, string filter, string title, bool oneFileOnly = false) =>
            new[] {path};

        public string ImageFileFilter => throw new NotImplementedException();
    }
}
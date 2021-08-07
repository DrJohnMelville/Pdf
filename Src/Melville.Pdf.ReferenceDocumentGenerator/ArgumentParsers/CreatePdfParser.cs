﻿using System.IO;
using System.Threading.Tasks;

namespace Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers
{
    public abstract class CreatePdfParser: IArgumentParser
    {
        public string Prefix { get; }

        protected CreatePdfParser(string prefix)
        {
            Prefix = prefix;
        }

        public async ValueTask<IArgumentParser?> ParseArgumentAsync(string argument, IRootParser root)
        {
            await using (var targetStream = root.Target.CreateTargetStream())
            {
                await WritePdf(targetStream);
            }
            root.Target.View();
            return null;
        }

        protected abstract ValueTask WritePdf(Stream target);
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Enumeration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{
    public sealed class ReportMetadata(string documentExtension, string mimeType, string fileName, double fileSize)
    {
        public required string DocumentExtension { get; init; } = documentExtension;
        public required string MimeType { get; init; } = mimeType;
        public required string FileName { get; init; } = fileName;
        public string Encoding { get; init; } = "UTF-8";
        public required double FileSize { get; init; } = fileSize;
        public required string DatabaseIdentifier { get; init; } = Guid.NewGuid().ToString();
        public required DateTime CreationDate { get; init; } = DateTime.Now;
        public required string Author { get; init; }
        public required string BLOB_URI { get; init; }
    }
}

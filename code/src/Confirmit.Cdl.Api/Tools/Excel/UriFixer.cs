using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Confirmit.Cdl.Api.Tools.Excel
{
    /// <summary>
    /// Original idea:
    /// http://www.ericwhite.com/blog/handling-invalid-hyperlinks-openxmlpackageexception-in-the-open-xml-sdk/
    /// </summary>
    public static class UriFixer
    {
        private const string FakeUri = "about:blank";

        public static void FixInvalidUris(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Update, true))
            {
                foreach (var entry in archive.Entries.ToList().Where(e => e.Name.EndsWith(".rels")))
                {
                    UpdateEntryIfNeeded(archive, entry);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
        }

        private static void UpdateEntryIfNeeded(ZipArchive archive, ZipArchiveEntry entry)
        {
            XNamespace relNs = "http://schemas.openxmlformats.org/package/2006/relationships";
            var needToReplace = false;
            XDocument entryXDoc;
            using (var entryStream = entry.Open())
            {
                try
                {
                    entryXDoc = XDocument.Load(entryStream);
                    if (entryXDoc.Root == null || entryXDoc.Root.Name.Namespace != relNs)
                        return;

                    var urisToCheck = entryXDoc
                        .Descendants(relNs + "Relationship")
                        .Where(r => r.Attribute("TargetMode")?.Value == "External")
                        .ToList();

                    foreach (var rel in urisToCheck.Where(
                        r => !Uri.TryCreate(r.Attribute("Target")?.Value, UriKind.Absolute, out _)))
                    {
                        rel.SetAttributeValue("Target", FakeUri);
                        needToReplace = true;
                    }
                }
                catch (XmlException)
                {
                    return;
                }
            }

            if (needToReplace)
                ReplaceEntry(archive, entryXDoc, entry);
        }

        private static void ReplaceEntry(ZipArchive archive, XDocument entryXDoc, ZipArchiveEntry entry)
        {
            var fullName = entry.FullName;
            entry.Delete();
            var newEntry = archive.CreateEntry(fullName);
            using var streamWriter = new StreamWriter(newEntry.Open());
            using var xmlWriter = XmlWriter.Create(streamWriter);
            entryXDoc.WriteTo(xmlWriter);
        }
    }
}

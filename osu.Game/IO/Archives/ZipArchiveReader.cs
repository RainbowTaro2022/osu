﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace osu.Game.IO.Archives
{
    public sealed class ZipArchiveReader : ArchiveReader
    {
        /// <summary>
        /// List of substrings that indicate a file should be ignored during the import process
        /// (usually due to representing no useful data and being autogenerated by the OS).
        /// </summary>
        private static readonly string[] filename_ignore_list =
        {
            // Mac-specific
            "__MACOSX",
            ".DS_Store",
            // Windows-specific
            "Thumbs.db"
        };

        private readonly Stream archiveStream;
        private readonly ZipArchive archive;

        public ZipArchiveReader(Stream archiveStream, string name = null)
            : base(name)
        {
            this.archiveStream = archiveStream;
            archive = ZipArchive.Open(archiveStream);
        }

        public override Stream GetStream(string name)
        {
            ZipArchiveEntry entry = archive.Entries.SingleOrDefault(e => e.Key == name);
            if (entry == null)
                throw new FileNotFoundException();

            // allow seeking
            MemoryStream copy = new MemoryStream();

            using (Stream s = entry.OpenEntryStream())
                s.CopyTo(copy);

            copy.Position = 0;

            return copy;
        }

        public override void Dispose()
        {
            archive.Dispose();
            archiveStream.Dispose();
        }

        private static bool canBeIgnored(IEntry entry) => filename_ignore_list.Any(ignoredName => entry.Key.IndexOf(ignoredName, StringComparison.InvariantCultureIgnoreCase) >= 0);

        public override IEnumerable<string> Filenames => archive.Entries.Where(e => !canBeIgnored(e)).Select(e => e.Key).ToArray();

        public override Stream GetUnderlyingStream() => archiveStream;
    }
}

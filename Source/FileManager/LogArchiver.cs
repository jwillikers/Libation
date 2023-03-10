﻿using Dinah.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace FileManager
{
	public class LogArchiver : IDisposable
	{
		public Encoding Encoding { get; set; }
		public string FileName { get; }
		private readonly ZipArchive archive;

		public LogArchiver(string filename) : this(filename, Encoding.UTF8) { }
		public LogArchiver(string filename, Encoding encoding)
		{
			FileName = ArgumentValidator.EnsureNotNull(filename, nameof(filename));
			Encoding = ArgumentValidator.EnsureNotNull(encoding, nameof(encoding));
			archive = new ZipArchive(File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), ZipArchiveMode.Update, false, Encoding);
		}

		public void DeleteOlderThan(DateTime cutoffDate)
			=> DeleteEntries(archive.Entries.Where(e => e.LastWriteTime < cutoffDate).ToList());

		public void DeleteOldestN(int quantity)
			=> DeleteEntries(archive.Entries.OrderBy(e => e.LastWriteTime).Take(quantity).ToList());
		
		public void DeleteAllButNewestN(int quantity)
			=> DeleteEntries(archive.Entries.OrderByDescending(e => e.LastWriteTime).Skip(quantity).ToList());
		
		private void DeleteEntries(List<ZipArchiveEntry> entries)
		{
			foreach (var e in entries)
				e.Delete();
		}

		public void AddFile(string name, JObject contents, string comment = null)
			=> AddFile(name, Encoding.GetBytes(contents.ToString(Newtonsoft.Json.Formatting.Indented)), comment);

		public void AddFile(string name, string contents, string comment = null)
			=> AddFile(name, Encoding.GetBytes(contents), comment);

		private readonly object lockOob = new();

		public void AddFile(string name, ReadOnlySpan<byte> contents, string comment = null)
		{
			ArgumentValidator.EnsureNotNull(name, nameof(name));

			name = ReplacementCharacters.Barebones.ReplaceFilenameChars(name);

			lock (lockOob)
			{
				var entry = archive.CreateEntry(name, CompressionLevel.SmallestSize);

				entry.Comment = comment;
				using var entryStream = entry.Open();
				entryStream.Write(contents);
			}
		}

		public void Dispose() => archive.Dispose();
	}
}
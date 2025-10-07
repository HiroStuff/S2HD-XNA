using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace S2HD.Shared.Data
{
	public static class DataService
	{
		private static string _baseDataRoot;
		private static bool _folderMode;
		private static DatArchive _archive;

		public static void Init(string baseDataRoot, bool folderMode)
		{
			_baseDataRoot = baseDataRoot;
			_folderMode = folderMode;
			_archive = null;
			if (!folderMode)
			{
				string datPath = Path.Combine(baseDataRoot, "sonicorca.dat");
				_archive = new DatArchive(datPath);
			}
		}

		public static Stream OpenRead(string relativePath)
		{
			if (_folderMode)
			{
				return File.OpenRead(Path.Combine(_baseDataRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
			}
			if (_archive == null)
				throw new InvalidOperationException("DataService not initialized with archive.");
			string keyPath = relativePath;
			if (Path.IsPathRooted(relativePath))
			{
				string baseNorm = _baseDataRoot.Replace('\\','/');
				string relNorm = relativePath.Replace('\\','/');
				if (relNorm.StartsWith(baseNorm, StringComparison.OrdinalIgnoreCase))
				{
					keyPath = relNorm.Substring(baseNorm.Length).TrimStart('/');
				}
			}
			return _archive.Open(keyPath);
		}

		private sealed class DatArchive
		{
			private readonly string _path;
			private readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

			public DatArchive(string path)
			{
				_path = path;
				Index();
			}

			public Stream Open(string relPath)
			{
				string key = ToKey(relPath);
				if (!_entries.TryGetValue(key, out var e))
					throw new FileNotFoundException($"Resource not found in archive: {relPath}");
				string sourcePath = e.ExternalPath ?? _path;
				var fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				fs.Position = e.Offset;
				var slice = new Substream(fs, e.Size, leaveOpen: false);
				if (e.Compressed)
				{
					return new GZipStream(slice, CompressionMode.Decompress, leaveOpen: false);
				}
				return slice;
			}

			private void Index()
			{
				using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true))
				{
					int magic = br.ReadInt32();
					byte version = br.ReadByte();
					long tableSize = br.ReadInt64();
					long dataOffset = 4 + 1 + 8 + tableSize;
					ScanNode(br, _entries, dataOffset, fs, null);
				}
			}

			private static void ScanNode(BinaryReader br, Dictionary<string, Entry> map, long dataOffset, Stream stream, string fullPath)
			{
				byte flags = br.ReadByte();
				bool hasChildren = (flags & 0x02) != 0;
				bool hasResource = (flags & 0x04) != 0;
				bool external = (flags & 0x08) != 0;
				bool compressed = (flags & 0x10) != 0;
				ushort childCount = hasChildren ? br.ReadUInt16() : (ushort)0;
				string key = ReadNullTerminatedString(br);
				string newPath = string.IsNullOrEmpty(fullPath) ? key : (fullPath + "/" + key);
				if (hasResource)
				{
					ushort identifier = br.ReadUInt16();
					string path = external ? ReadNullTerminatedString(br) : null;
					uint offset = br.ReadUInt32();
					uint size = br.ReadUInt32();
					long absOffset = external ? offset : (dataOffset + offset);
					map[newPath] = new Entry { ExternalPath = path, Offset = absOffset, Size = (int)size, Compressed = !external && compressed };
				}
				for (int i = 0; i < childCount; i++)
				{
					ScanNode(br, map, dataOffset, stream, newPath);
				}
			}

			private static string ReadNullTerminatedString(BinaryReader br)
			{
				List<byte> bytes = new List<byte>();
				byte b;
				while ((b = br.ReadByte()) != 0)
					bytes.Add(b);
				return Encoding.ASCII.GetString(bytes.ToArray());
			}

			private static string ToKey(string relPath)
			{
				string p = relPath.Replace('\\', '/');
				if (p.StartsWith("/")) p = p.Substring(1);
				int dot = p.LastIndexOf('.');
				if (dot >= 0) p = p.Substring(0, dot);
				return p.ToUpperInvariant();
			}

			private sealed class Entry
			{
				public string ExternalPath;
				public long Offset;
				public int Size;
				public bool Compressed;
			}

			private sealed class Substream : Stream
			{
				private readonly Stream _base;
				private readonly long _start;
				private readonly long _length;
				private readonly bool _leaveOpen;
				private long _position;

				public Substream(Stream @base, long length, bool leaveOpen)
				{
					_base = @base; _start = @base.Position; _length = length; _leaveOpen = leaveOpen; _position = 0;
				}

				public override bool CanRead => true;
				public override bool CanSeek => true;
				public override bool CanWrite => false;
				public override long Length => _length;
				public override long Position { get => _position; set => Seek(value, SeekOrigin.Begin); }
				public override void Flush() { }
				public override int Read(byte[] buffer, int offset, int count)
				{
					long remaining = _length - _position;
					if (remaining <= 0) return 0;
					if (count > remaining) count = (int)remaining;
					_base.Position = _start + _position;
					int read = _base.Read(buffer, offset, count);
					_position += read;
					return read;
				}
				public override long Seek(long offset, SeekOrigin origin)
				{
					long newPos = origin == SeekOrigin.Begin ? offset : origin == SeekOrigin.Current ? _position + offset : _length + offset;
					newPos = Math.Max(0, Math.Min(_length, newPos));
					_position = newPos; return _position;
				}
				public override void SetLength(long value) => throw new NotSupportedException();
				public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
				protected override void Dispose(bool disposing)
				{
					if (disposing && !_leaveOpen) _base.Dispose();
					base.Dispose(disposing);
				}
			}
		}
	}
}



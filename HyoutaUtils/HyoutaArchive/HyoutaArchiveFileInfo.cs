using HyoutaPluginBase;
using HyoutaPluginBase.FileContainer;
using HyoutaUtils.Checksum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public class HyoutaArchiveFileInfo : IFile {
		// the actual data of this file
		// this stream may be compressed, and it may be a bps patch that needs to be applied first
		// if you just want the file data and don't care about the internal state use DataStream instead
		public DuplicatableStream? Data;

		public byte[]? DummyContent;
		public string? Filename;

		// if CompressionInfo is not null but StreamIsCompressed is false, then the file is intended to be compressed
		// within the on-disk container but has already been decompressed (or not yet compressed) in memory
		// if packing with such a file info, we need to compress the data before writing it into the archive
		// if StreamIsCompressed is true on packing we can assume the data was already compressed and can just write it as-is
		public Compression.IHyoutaArchiveCompressionInfo? CompressionInfo;
		public bool StreamIsCompressed = false;

		// just like CompressionInfo above
		// if a stream is both compressed and a patch:
		// unpacking needs to first decompress and then patch-apply, packing is the other way around
		public HyoutaArchiveBpsPatchInfo? BpsPatchInfo;
		public bool StreamIsBpsPatch = false;

		public CRC32? crc32;
		public MD5? md5;
		public SHA1? sha1;

		#region IFile implementation
		public DuplicatableStream DataStream {
			get {
				DuplicatableStream? s = Data;
				if (s == null) {
					throw new NullReferenceException();
				}
				if (CompressionInfo != null && StreamIsCompressed) {
					s = CompressionInfo.Decompress(s.Duplicate());
				}
				if (BpsPatchInfo != null && StreamIsBpsPatch) {
					s = HyoutaArchiveBps.ApplyPatch(BpsPatchInfo, s.Duplicate());
				}
				return s.Duplicate();
			}
		}

		public bool IsFile => true;
		public bool IsContainer => false;
		public IFile AsFile => this;
		public IContainer? AsContainer => null;
		private bool disposedValue;
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (Data != null) {
						Data.Dispose();
					}
				}
				disposedValue = true;
			}
		}
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}

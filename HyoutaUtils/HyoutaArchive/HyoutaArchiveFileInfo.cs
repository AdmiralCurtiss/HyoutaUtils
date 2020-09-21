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
		public DuplicatableStream Data;

		public byte[] DummyContent;
		public string Filename;
		public HyoutaArchiveCompressionInfo Compression;
		public HyoutaArchiveBpsPatchInfo BpsPatch;
		public CRC32? crc32;
		public MD5? md5;
		public SHA1? sha1;

		#region IFile implementation
		public DuplicatableStream DataStream => Data.Duplicate();
		public bool IsFile => true;
		public bool IsContainer => false;
		public IFile AsFile => this;
		public IContainer AsContainer => null;
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

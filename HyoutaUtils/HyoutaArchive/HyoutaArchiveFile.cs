using HyoutaPluginBase;
using HyoutaPluginBase.FileContainer;
using HyoutaUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyoutaUtils.HyoutaArchive {
	public class HyoutaArchiveFile : IContainer {
		private List<HyoutaArchiveChunk> Chunks;
		private long[] FileCountOffsets;

		public long Filecount {
			get {
				long c = 0;
				foreach (HyoutaArchiveChunk chunk in Chunks) {
					c += chunk.Filecount;
				}
				return c;
			}
		}

		public HyoutaArchiveFile(DuplicatableStream stream) {
			ulong totalChunkLengths;
			var firstChunk = new HyoutaArchiveChunk(stream, out totalChunkLengths);

			Chunks = new List<HyoutaArchiveChunk>();
			Chunks.Add(firstChunk);
			while (totalChunkLengths < (ulong)stream.Length) {
				ulong tmp;
				Chunks.Add(new HyoutaArchiveChunk(new PartialStream(stream, (long)totalChunkLengths, (long)((ulong)stream.Length - totalChunkLengths)), out tmp));
				totalChunkLengths += tmp;
			}

			Chunks.TrimExcess();
			FileCountOffsets = new long[Chunks.Count + 1];
			FileCountOffsets[0] = 0;
			for (int i = 0; i < Chunks.Count; ++i) {
				FileCountOffsets[i + 1] = FileCountOffsets[i] + Chunks[i].Filecount;
			}
		}

		public bool IsFile => false;
		public bool IsContainer => true;
		public IFile AsFile => null;
		public IContainer AsContainer => this;

		public INode GetChildByIndex(long index) {
			// is this right???
			for (int i = 1; i < FileCountOffsets.Length; ++i) {
				if (index < FileCountOffsets[i]) {
					return Chunks[i - 1].GetChildByIndex(index - FileCountOffsets[i - 1]);
				}
			}
			return null;
		}

		public INode GetChildByName(string name) {
			foreach (HyoutaArchiveChunk chunk in Chunks) {
				INode child = chunk.GetChildByName(name);
				if (child != null) {
					return child;
				}
			}
			return null;
		}

		public IEnumerable<string> GetChildNames() {
			foreach (HyoutaArchiveChunk chunk in Chunks) {
				foreach (string s in chunk.GetChildNames()) {
					yield return s;
				}
			}
		}

		private bool disposedValue;
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					for (int i = 0; i < Chunks.Count; ++i) {
						Chunks[i].Dispose();
					}
				}
				disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

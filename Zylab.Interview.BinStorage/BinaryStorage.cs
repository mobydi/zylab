using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zylab.Interview.BinStorage {
    public class BinaryStorage : IBinaryStorage {

		Dictionary<string, Int64> index = new Dictionary<string, Int64> ();
		readonly FileStream storageSream;
		readonly object writeLock = new object();

        public BinaryStorage(StorageConfiguration configuration) {

			storageFile = Path.Combine (configuration.WorkingFolder, "storage.bin");
			if (!File.Exists (storageFile))
				File.Create (storageFile).Close ();
			storageSream = new FileStream(storageFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None,
				        bufferSize: 4096, useAsync: true);
        }

        public void Add(string key, Stream data) {
			if (index.ContainsKey (key))
				throw new Exception ();
		
			Int64 positionToWrite;
			lock (writeLock) {
				positionToWrite = storageSream.Length;
				storageSream.Seek (data.Length, SeekOrigin.End);
			}
        }

        public Stream Get(string key) {
            throw new NotImplementedException();
        }

        public bool Contains(string key) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}

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
		readonly string storageFile;

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

			using (var writeStream = new FileStream (storageFile, FileMode.Open, FileAccess.Write)) {
				writeStream.Seek (positionToWrite, SeekOrigin.Begin);
				data.CopyTo (writeStream);
			}

			index.Add (key, positionToWrite);
        }

        public Stream Get(string key) {
			if (!index.ContainsKey (key))
				throw new Exception ();

			var positionToRead = index [key];
			var readStream = new WindowStream(new FileStream (storageFile, FileMode.Open, FileAccess.Read), positionToRead, 0);
			return readStream;
        }

        public bool Contains(string key) {
			return index.ContainsKey (key);
        }

        public void Dispose() {
			storageSream.Dispose ();
        }

    }
}

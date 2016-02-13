using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zylab.Interview.BinStorage {
    public class BinaryStorage : IBinaryStorage {

		Dictionary<string, Int64> index = new Dictionary<string, Int64> ();

        public BinaryStorage(StorageConfiguration configuration) {

			string storageFile = Path.Combine (configuration.WorkingFolder, "storage.bin");
			if (!File.Exists (storageFile))
				File.Create (storageFile).Close ();
        }

        public void Add(string key, Stream data) {
			if (index.ContainsKey (key))
				throw new Exception ();

			//increase file size

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

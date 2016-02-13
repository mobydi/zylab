﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zylab.Interview.BinStorage {
    public class BinaryStorage : IBinaryStorage {

		Dictionary<string, Data> index = new Dictionary<string, Data> ();
		readonly FileStream storageSream;
		readonly object writeLock = new object();
		readonly string storageFile;

        public BinaryStorage(StorageConfiguration configuration) {

			storageFile = Path.Combine (configuration.WorkingFolder, "storage.bin");
			storageSream = new FileStream(storageFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None,
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

			index.Add (key, new Data {Position = positionToWrite, Length = data.Length, MD5 = "MD5"});
        }

        public Stream Get(string key) {

			Data data;
			if (!index.TryGetValue (key, out data))
				throw new Exception ();

			var readStream = new WindowStream(
				new FileStream (storageFile, FileMode.Open, FileAccess.Read), 
				data.Position, data.Length);
			return readStream;
        }

        public bool Contains(string key) {
			return index.ContainsKey (key);
        }

        public void Dispose() {
			storageSream.Dispose ();
        }

    }

	public struct Data
	{
		public Int64 Position { get; set;}
		public Int64 Length { get; set; }
		public string MD5 { get; set; }
	}
}

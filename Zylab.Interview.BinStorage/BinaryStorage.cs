﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Zylab.Interview.BinStorage
{
    public class BinaryStorage : IBinaryStorage {

		static Dictionary<string, Data> index = new Dictionary<string, Data> ();
		readonly FileStream storageSream;
		readonly object writeLock = new object();
		readonly string storageFile;

        public BinaryStorage(StorageConfiguration configuration) {

			storageFile = Path.Combine (configuration.WorkingFolder, "storage.bin");
			storageSream = new FileStream(storageFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096);
        }

        public void Add(string key, Stream data) {
			if (string.IsNullOrEmpty (key))
				throw new ArgumentNullException ();

			if (data == null)
				throw new ArgumentNullException ();

			if (index.ContainsKey (key))
				throw new ArgumentException ();
		
			Int64 positionToWrite;
			lock (writeLock) {
				positionToWrite = storageSream.Length;
				storageSream.Seek (data.Length, SeekOrigin.End);
			}

            byte[] md5;
			using (var writeStream = new FileStream (storageFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) {
				writeStream.Seek (positionToWrite, SeekOrigin.Begin);
				md5 = data.CopyToWithMD5 (writeStream);
                writeStream.Flush(true);
			}
            lock(writeLock)
			    index.Add (key, new Data {Position = positionToWrite, Length = data.Length, MD5 = md5});
        }

        public Stream Get(string key) {
			if (string.IsNullOrEmpty (key))
				throw new ArgumentNullException ();
			
			Data data;
			if (!index.TryGetValue (key, out data))
				throw new KeyNotFoundException ();

			var readStream = new WindowStream(
				new FileStream (storageFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), 
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
		public byte[] MD5 { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using CSharpTest.Net.Synchronization;

namespace Zylab.Interview.BinStorage
{
    public class BinaryStorage : IBinaryStorage {
		private const int cacheSize = 200 * 1024 * 1024; //200MB
		private const int writeBufferSize = 81920;
		private const int estimatedKeySize = 50;
		private readonly int estimatedDataSize = DataSerializer.GetEstimatedSize();

		private readonly BPlusTree<string, Data> index;
		private readonly FileStream storageStream;
		private readonly object seekLock = new object();
		private readonly string storageFile;
		private readonly ConcurrentDictionary<string, object> currentWritings = new ConcurrentDictionary<string, object>();

        public BinaryStorage(StorageConfiguration configuration) {
			var options = new BPlusTree<string, Data>.OptionsV2 (PrimitiveSerializer.String, new DataSerializer());
			options.CreateFile = CreatePolicy.IfNeeded;
			options.FileName = Path.Combine (configuration.WorkingFolder, "index.bin");
            options.CallLevelLock = new ReaderWriterLocking();
			options.CachePolicy = CachePolicy.Recent;
			options.CacheKeepAliveMaximumHistory = cacheSize / (estimatedKeySize + estimatedDataSize);
			options.CalcBTreeOrder (estimatedKeySize, estimatedDataSize);
			index = new BPlusTree<string, Data> (options);

			storageFile = Path.Combine (configuration.WorkingFolder, "storage.bin");
			storageStream = new FileStream(storageFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.WriteThrough);
        }

        public void Add(string key, Stream data) {
			if (string.IsNullOrEmpty (key))
				throw new ArgumentNullException ();

			if (data == null)
				throw new ArgumentNullException ();
			
			try {
				if (!currentWritings.TryAdd (key, null))
					throw new ArgumentException ();

				if (index.ContainsKey (key))
					throw new ArgumentException ();
			
				Int64 positionToWrite;
				lock (seekLock) {
					positionToWrite = storageStream.Length;
					storageStream.SetLength(storageStream.Length + data.Length);
				}
		       
		        byte[] md5;
		        using (var writeStream = new FileStream (storageFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, writeBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.WriteThrough)) {
		            writeStream.Seek (positionToWrite, SeekOrigin.Begin);
		            md5 = data.CopyToWithMD5 (writeStream, writeBufferSize);
		            writeStream.Flush(true);
		        }

				index.Add (key, new Data { Position = positionToWrite, Length = data.Length, MD5 = md5 });
			}
			finally {
				object value;
				currentWritings.TryRemove (key, out value);
			}
        }

        public Stream Get(string key) {
			if (string.IsNullOrEmpty (key))
				throw new ArgumentNullException ();
			
			Data data;
			if (!index.TryGetValue (key, out data))
				throw new KeyNotFoundException ();

			var readStream = new WindowStream(
				new FileStream (storageFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, writeBufferSize, FileOptions.SequentialScan), 
				data.Position, data.Length);
			return readStream;
        }

        public bool Contains(string key) {
			return index.ContainsKey (key);
        }

        public void Dispose() {
			storageStream.Dispose ();
			index.Dispose ();
        }
    }
}
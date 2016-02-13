﻿using System;
using System.Collections.Generic;
using System.IO;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using CSharpTest.Net.Synchronization;
using System.Runtime.InteropServices;

namespace Zylab.Interview.BinStorage
{
    public class BinaryStorage : IBinaryStorage {
		readonly BPlusTree<string, Data> index;
		readonly FileStream storageStream;
		readonly object seekLock = new object();
		readonly string storageFile;

        const int writeBufferSize = 81920;

        public BinaryStorage(StorageConfiguration configuration) {
			var options = new BPlusTree<string, Data>.OptionsV2 (PrimitiveSerializer.String, new DataSerializer());
			options.CreateFile = CreatePolicy.IfNeeded;
			options.FileName = Path.Combine (configuration.WorkingFolder, "index.bin");
            options.CallLevelLock = new ReaderWriterLocking();
			options.CachePolicy = CachePolicy.Recent;
			options.CacheKeepAliveMaximumHistory = 125000; // ~50Mb of cached objects;
			options.CalcBTreeOrder (50, 8+8+16);
			index = new BPlusTree<string, Data> (options);

			storageFile = Path.Combine (configuration.WorkingFolder, "storage.bin");
			storageStream = new FileStream(storageFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.WriteThrough);
        }

        public void Add(string key, Stream data) {
			if (string.IsNullOrEmpty (key))
				throw new ArgumentNullException ();

			if (data == null)
				throw new ArgumentNullException ();

            //TODO: lock here!!!
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

        public Stream Get(string key) {
			if (string.IsNullOrEmpty (key))
				throw new ArgumentNullException ();
			
			Data data;
			if (!index.TryGetValue (key, out data))
				throw new KeyNotFoundException ();

			var readStream = new WindowStream(
				new FileStream (storageFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 8192, FileOptions.SequentialScan), 
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

	struct Data
	{
		public Int64 Position { get; set;}
		public Int64 Length { get; set; }
		public byte[] MD5 { get; set; }
	}


	class DataSerializer : ISerializer<Data>
	{
		#region ISerializer implementation
		public void WriteTo (Data value, Stream stream)
		{
			PrimitiveSerializer.Int64.WriteTo (value.Position, stream);
			PrimitiveSerializer.Int64.WriteTo (value.Length, stream);
			stream.Write (value.MD5, 0, value.MD5.Length);
		}
		public Data ReadFrom (Stream stream)
		{
			var pos = PrimitiveSerializer.Int64.ReadFrom(stream);
			var len = PrimitiveSerializer.Int64.ReadFrom(stream);
			var md5 = new byte[16];
			stream.Read (md5, 0, md5.Length);

			return new Data{ Position = pos, Length = len, MD5 = md5 };
		}
		#endregion
	}
}

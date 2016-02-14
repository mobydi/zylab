using System;
using CSharpTest.Net.Serialization;
using System.IO;

namespace Zylab.Interview.BinStorage
{
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

		public static int GetEstimatedSize() {
			return 8 + 8 + 16; //int64 + int64 + byte[16]
		}
	}
}


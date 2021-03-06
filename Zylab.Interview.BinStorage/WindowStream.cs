﻿using System;
using System.IO;

namespace Zylab.Interview.BinStorage
{
	public class WindowStream : Stream
	{
		#region implemented abstract members of Stream

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (stream.Position + count> end)
				count = (int)(end - stream.Position);
			return stream.Read (buffer, offset, count);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return false;
			}
		}

		public override long Length {
			get {
				return length;
			}
		}

		public override long Position {
			get {
				return this.Position - start;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		#endregion

		readonly FileStream stream;
		readonly Int64 start;
		readonly Int64 length;
		readonly Int64 end;

		public WindowStream (FileStream stream, Int64 start, Int64 length)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			
			this.length = length;
			this.start = start;
			this.stream = stream;
			this.end = start + length;

			stream.Seek (start, SeekOrigin.Begin);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				stream.Dispose ();

			base.Dispose (disposing);
		}
	}
}


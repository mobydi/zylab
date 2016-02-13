using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Zylab.Interview.BinStorage
{
    public static class StreamExtention
    {
        public static byte[] CopyToWithMD5(this Stream source, Stream destination, int bufferSize = 81920)
        {
            using (MD5 md5Hasher = MD5.Create())
            {
                byte[][] buffer = new byte[2][];
                buffer[0] = new byte[bufferSize/2];
                buffer[1] = new byte[bufferSize/2];
                int current_read_buffer = 0;

                int readBytes;
                Task writeOperation = null;
                while ((readBytes = source.Read(buffer[current_read_buffer], 0, bufferSize/2)) > 0)
                {
                    if(writeOperation != null)
                    {
                        writeOperation.Wait();
                    }
					writeOperation = destination.WriteAsync(buffer[current_read_buffer], 0, readBytes);
                    md5Hasher.TransformBlock(buffer[current_read_buffer], 0, readBytes, buffer[current_read_buffer], 0);
                    current_read_buffer = current_read_buffer == 0 ? 1 : 0;
                }

                md5Hasher.TransformFinalBlock(new byte[0], 0, 0);
                if (writeOperation != null)
                {
                    writeOperation.Wait();
                }

                return md5Hasher.Hash;
            }
        }
    }
}
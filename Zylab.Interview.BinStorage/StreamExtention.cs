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
                var halfBuffer = bufferSize / 2;
                byte[][] buffer = new byte[2][];
                buffer[0] = new byte[halfBuffer];
                buffer[1] = new byte[halfBuffer];
                int current_read_buffer = 0;

                int readBytes;
                Task asyncTask = null;
                while ((readBytes = source.Read(buffer[current_read_buffer], 0, halfBuffer)) > 0)
                {
                    if(asyncTask != null)
                    {
                        asyncTask.Wait();
                    }
					var writeOperation = destination.WriteAsync(buffer[current_read_buffer], 0, readBytes);
                    var md5Operation = Task.Run(() => md5Hasher.TransformBlock(buffer[current_read_buffer], 0, readBytes, buffer[current_read_buffer], 0));
                    asyncTask = Task.WhenAll(writeOperation, md5Operation);
                    current_read_buffer = current_read_buffer == 0 ? 1 : 0;
                }
              
                if (asyncTask != null)
                {
                    asyncTask.Wait();
                }
        
                md5Hasher.TransformFinalBlock(new byte[0], 0, 0);
                return md5Hasher.Hash;
            }
        }
    }
}
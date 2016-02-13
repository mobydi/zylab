using System.IO;
using System.Security.Cryptography;

namespace Zylab.Interview.BinStorage
{
    public static class StreamExtention
    {
        public static byte[] CopyToWithMD5(this Stream source, Stream destination, int bufferSize = 81920)
        {
            using (MD5 md5Hasher = MD5.Create())
            {
                byte[] buffer = new byte[bufferSize];
                int readBytes;

                while ((readBytes = source.Read(buffer, 0, bufferSize)) > 0)
                {
                    destination.Write(buffer, 0, readBytes);
                    md5Hasher.TransformBlock(buffer, 0, readBytes, buffer, 0);
                }

                md5Hasher.TransformFinalBlock(new byte[0], 0, 0);

                return md5Hasher.Hash;
            }
        }
    }
}
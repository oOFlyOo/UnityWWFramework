
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace WWFramework.Zip
{
    public static class SharpZipLibHelper
    {
        private const int BuffSize = 1024;

        static SharpZipLibHelper()
        {
            ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.UTF8.CodePage;
        }


        public static int Compress(byte[] input, byte[] buffer, ref byte[] result, int level = Deflater.BEST_COMPRESSION)
        {
            var deflater = new Deflater();
            deflater.SetLevel(level);
            deflater.SetInput(input);
            deflater.Finish();

            buffer = buffer ?? new byte[BuffSize];
            MemoryStream ms;
            if (result != null)
            {
                ms = new MemoryStream(result);
            }
            else
            {
                ms = new MemoryStream(input.Length);
            }

            var length = 0;
            while (!deflater.IsFinished)
            {
                var count = deflater.Deflate(buffer);
                ms.Write(buffer, 0, count);
                length += count;
            }

            if (result == null)
            {
                result = ms.ToArray();
            }
            ms.Dispose();

            return length;
        }


        public static int UnCompress(byte[] input, byte[] buffer, ref byte[] result)
        {
            var inflater = new Inflater();
            inflater.SetInput(input);

            buffer = buffer ?? new byte[BuffSize];
            MemoryStream ms;
            if (result != null)
            {
                ms = new MemoryStream(result);
            }
            else
            {
                ms = new MemoryStream(input.Length);
            }

            var length = 0;
            while (!inflater.IsFinished)
            {
                var count = inflater.Inflate(buffer);
                ms.Write(buffer, 0, count);
                length += count;
            }

            if (result == null)
            {
                result = ms.ToArray();
            }
            ms.Dispose();

            return length;
        }
    }
}
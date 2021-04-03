using BenchmarkDotNet.Attributes;
using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BenchmarkDotNetSample
{
    [ShortRunJob]
    public class SearchBinPatternFromFile
    {
        [Params(10_000, 1_000_000, 100_000_000)]
        public int SIZE;
        private static readonly string _tempFilePath = @"temp.bin";
        private static readonly byte[] _pattern = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        [GlobalSetup]
        public void GlobalSetup()
        {
            var bs = GetBinaryArray();
            WriteBinaryToFile(_tempFilePath, bs);
        }

        #region Setup
        private byte[] GetBinaryArray()
        {
            var data = new byte[SIZE];
            var offset = data.Length - _pattern.Length - 1;
            for (int i = 0; i < _pattern.Length; ++i)
            {
                data[offset + i] = _pattern[i];
            }
            return data;
        }

        private static void WriteBinaryToFile(string path, byte[] data)
        {
            if (File.Exists(path)) File.Delete(path);

            using var fs = new FileStream(path, FileMode.Create);
            using var sw = new BinaryWriter(fs);
            sw.Write(data);
        }
        #endregion

        [Benchmark(Baseline = true)]
        public async Task Buf1024Async()
        {
            int size = 1024;

            using var fs = new FileStream(_tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var sourceBytes = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                var sourceMemory = new Memory<byte>(sourceBytes);
                var remainSize = fs.Length;

                while (remainSize > 0)
                {
                    var readSize = await fs.ReadAsync(sourceMemory);

                    for (var i = 0; i < readSize; ++i)
                    {
                        if (sourceBytes[i] == _pattern[0])
                        {
                            for (var j = 1; j < _pattern.Length; ++j)
                            {
                                if (sourceBytes[i + j] != _pattern[j])
                                    goto OUTER_FOR_END;
                            }
                            return; // fs.Length - remainSize + i;
                        }
                        OUTER_FOR_END: ;
                    }

                    remainSize -= readSize;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sourceBytes, clearArray: true);
            }
            //return -1;
        }

        [Benchmark]
        public async Task Buf1024Async2()
        {
            int size = 1024;

            using var fs = new FileStream(_tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var sourceBytes = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                var sourceMemory = new Memory<byte>(sourceBytes);
                var remainSize = fs.Length;
                var headByte = _pattern[0];     // ★スタックにコピー

                while (remainSize > 0)
                {
                    var readSize = await fs.ReadAsync(sourceMemory);

                    for (var i = 0; i < readSize; ++i)
                    {
                        if (sourceBytes[i] == headByte)     // ★スタックと比較 _pattern[0]
                        {
                            for (var j = 1; j < _pattern.Length; ++j)
                            {
                                if (sourceBytes[i + j] != _pattern[j])
                                    goto OUTER_FOR_END;
                            }
                            return; // fs.Length - remainSize + i;
                        }
                        OUTER_FOR_END:;
                    }

                    remainSize -= readSize;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sourceBytes, clearArray: true);
            }
            //return -1;
        }

        [Benchmark]
        public async Task Buf4096Async()
        {
            int size = 1024 * 4;

            using var fs = new FileStream(_tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var sourceBytes = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                var sourceMemory = new Memory<byte>(sourceBytes);
                var remainSize = fs.Length;

                while (remainSize > 0)
                {
                    var readSize = await fs.ReadAsync(sourceMemory);

                    for (var i = 0; i < readSize; ++i)
                    {
                        if (sourceBytes[i] == _pattern[0])
                        {
                            for (var j = 1; j < _pattern.Length; ++j)
                            {
                                if (sourceBytes[i + j] != _pattern[j])
                                    goto OUTER_FOR_END;
                            }
                            return; // fs.Length - remainSize + i;
                        }
                        OUTER_FOR_END:;
                    }

                    remainSize -= readSize;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sourceBytes, clearArray: true);
            }
            //return -1;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }
    }
}

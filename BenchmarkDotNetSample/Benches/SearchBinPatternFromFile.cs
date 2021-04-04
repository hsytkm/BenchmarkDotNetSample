using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BenchmarkDotNetSample
{
#if false   // 規模が大きくなったので別プロジェクトに移行した → SearchByteArrayPattern
    public class SearchBinPatternFromFileConfig : ManualConfig
    {
        public SearchBinPatternFromFileConfig()
        {
            AddExporter(MarkdownExporter.GitHub);
            AddDiagnoser(MemoryDiagnoser.Default);

            // ShortRunは LaunchCount=1  TargetCount=3 WarmupCount = 3 のショートカット
            AddJob(Job.ShortRun);
        }
    }

    [Config(typeof(SearchBinPatternFromFileConfig))]
    public class SearchBinPatternFromFile
    {
        [Params(10_000, 1_000_000, 100_000_000)]
        public int SIZE;

        private static readonly byte[] _pattern = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        private Stream? _stream;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var bs = GetBinaryArray();
            _stream = new MemoryStream(bs, writable: false);

            byte[] GetBinaryArray()
            {
                var data = new byte[SIZE];
                var offset = data.Length - _pattern.Length - 1;
                for (int i = 0; i < _pattern.Length; ++i)
                {
                    data[offset + i] = _pattern[i];
                }
                return data;
            }
        }

        [Benchmark(Baseline = true)]
        public async ValueTask CopyAllToBufferAsync()
        {
            // ファイルの内容を byte[] に全コピー（バカだけど割と高速）
            // https://stackoverflow.com/questions/283456/byte-array-pattern-search
            var fs = _stream!;
            var sourceBuffer = new byte[fs.Length];

            fs.Position = 0;
            _ = await fs.ReadAsync(sourceBuffer);
            _ = Search(sourceBuffer, _pattern);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static int Search(in ReadOnlySpan<byte> source, in ReadOnlySpan<byte> pattern)
            {
                var outerLoopEnd = source.Length - pattern.Length;
                var innerLoopEnd = pattern.Length;

                for (var i = 0; i <= outerLoopEnd; ++i)
                {
                    for (var j = 0; j < innerLoopEnd; ++j)
                    {
                        if (source[i + j] != pattern[j])
                            goto OUTER_LOOP_END;
                    }
                    return i;

                    OUTER_LOOP_END:;
                }
                return -1;
            }
        }

        [Benchmark]
        public async ValueTask SplitSearchAsync()
        {
            // ファイルの内容を分割して全コピー。 コピー先は Rent してくる。
            _ = await SearchBytesPatternUsingRent(_stream!, _pattern, 0x4000);   // =16KByte

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static async ValueTask<long> SearchBytesPatternUsingRent(Stream stream, byte[] patternBytes, int bufferSize = 1024)
            {
                stream.Position = 0;

                var patternLength = patternBytes.Length;
                var sourceBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                var patternBuffer = ArrayPool<byte>.Shared.Rent(patternLength);
                try
                {
                    var headByte = patternBytes[0];     // ★高速化：スタックにコピー
                    int readSize = 0;

                    for (var remainSize = stream.Length; remainSize >= patternLength; remainSize -= readSize)
                    {
                        readSize = await stream.ReadAsync(sourceBuffer);

                        for (var index = 0; index < readSize; ++index)
                        {
                            if (sourceBuffer[index] != headByte)    // ★高速化：スタックと比較 patternBytes[0]
                                continue;

                            var sourceRestSize = readSize - index;
                            if (sourceRestSize >= patternLength)
                            {
                                // 読み出し済み分で判定できる場合
                                if (IsHit(sourceBuffer, index, 1, patternBytes))
                                    return stream.Length - remainSize + index;
                            }
                            else
                            {
                                // 分割読み出し済み分で判定できない場合は追加で読み込む
                                var positionBuf = stream.Position;
                                stream.Position = index;
                                var size = stream.Read(patternBuffer, 0, patternLength);

                                if (size == patternLength)
                                {
                                    if (IsHit(patternBuffer, 0, 1, patternBytes))
                                        return stream.Length - remainSize + index;
                                }
                                stream.Position = positionBuf;
                            }
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(sourceBuffer, clearArray: true);
                    ArrayPool<byte>.Shared.Return(patternBuffer, clearArray: true);
                }
                return -1;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool IsHit(byte[] sourceBytes, int sourceIndex, int offset, byte[] patternBytes)
                {
                    var patternLength = patternBytes.Length - offset;
                    var sourceSpan = new ReadOnlySpan<byte>(sourceBytes, sourceIndex + offset, patternLength);
                    var patternSpan = new ReadOnlySpan<byte>(patternBytes, offset, patternLength);
                    return IsHitImpl(sourceSpan, patternSpan);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static bool IsHitImpl(in ReadOnlySpan<byte> sourceBytes, in ReadOnlySpan<byte> patternBytes)
                {
                    for (var i = 0; i < sourceBytes.Length; ++i)
                    {
                        if (sourceBytes[i] != patternBytes[i])
                            return false;
                    }
                    return true;
                }
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (_stream is not null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
    }
#endif
}

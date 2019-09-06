using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkDotNetSample
{
#if false

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i5-3210M CPU 2.50GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.801
  [Host]     : .NET Core 2.1.12 (CoreCLR 4.6.27817.01, CoreFX 4.6.27818.01), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.1.12 (CoreCLR 4.6.27817.01, CoreFX 4.6.27818.01), 64bit RyuJIT


|           Method |      Mean |     Error |    StdDev |
|----------------- |----------:|----------:|----------:|
|     UseArrayFor1 |  7.398 us | 0.1131 us | 0.1003 us |
|     UseArrayFor2 |  6.889 us | 0.0758 us | 0.0633 us |
| UseArrayForEach1 |  6.827 us | 0.0712 us | 0.0595 us |
| UseArrayForEach2 |  6.845 us | 0.0838 us | 0.0743 us |
|      UseListFor1 | 65.702 us | 1.2253 us | 1.2034 us |
|      UseListFor2 | 93.203 us | 0.6636 us | 0.5883 us |
|  UseListForEach1 | 86.452 us | 1.5587 us | 1.3817 us |
|         UseSpan1 |  6.878 us | 0.0590 us | 0.0552 us |
|         UseSpan2 | 10.573 us | 0.1047 us | 0.0979 us |

#endif

    public class SomeTest
    {
        private readonly int[] _array;
        private readonly IList<int> _list;
        private ReadOnlySpan<int> ArraySpan => _array;

        public SomeTest()
        {
            _array = Enumerable.Range(0, 10000).ToArray();
            _list = _array.ToList();
        }

        /// <summary>
        /// 早いけど最速ではない
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseArrayFor1()
        {
            var accum = 0;

            for (int i = 0; i < _array.Length; i++)
            {
                accum += _array[i];
            }
            return accum;
        }

        /// <summary>
        /// 同率三位
        /// ループ外でローカル変数にコピーした方がちょい早く最速クラス
        /// Lengthのチェックが必要なくなったおかげと思われる。IL見てないから知らんけど
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseArrayFor2()
        {
            var accum = 0;
            var array = _array;
            for (int i = 0; i < array.Length; i++)
            {
                accum += array[i];
            }
            return accum;
        }

        /// <summary>
        /// 一位
        /// 一応最速(UseArrayFor2とほぼ同等で有意差じゃないかも)
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseArrayForEach1()
        {
            var accum = 0;

            foreach (var i in _array)
            {
                accum += i;
            }
            return accum;
        }

        /// <summary>
        /// 二位
        /// UseArrayForEach1よりローカルコピーある分だけちょい遅いっぽい
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseArrayForEach2()
        {
            var accum = 0;

            var array = _array;
            foreach (var i in array)
            {
                accum += i;
            }
            return accum;
        }

        /// <summary>
        /// 遅いほうの三位
        /// リストは遅い
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseListFor1()
        {
            var accum = 0;
            var list = _list;

            for (int i = 0; i < list.Count; i++)
            {
                accum += list[i];
            }
            return accum;
        }

        /// <summary>
        /// ダントツワースト！
        /// 実装は一番楽！
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseListFor2()
        {
            return _list.Sum();
        }

        /// <summary>
        /// 遅いほうの二位
        /// リストは遅い
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseListForEach1()
        {
            var accum = 0;

            foreach (var i in _list)
            {
                accum += i;
            }
            return accum;
        }

        /// <summary>
        /// 同率三位
        /// UseArrayFor2と同等
        /// Spanで受けるのが一番応用性が高いと思う
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseSpan1()
        {
            var accum = 0;

            foreach (var i in ArraySpan)
            {
                accum += i;
            }
            return accum;
        }

        /// <summary>
        /// stackコピーしてみたけど、コピーコストのせいで遅い
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public int UseSpan2()
        {
            var accum = 0;

            Span<int> stack = stackalloc int[ArraySpan.Length];
            ArraySpan.CopyTo(stack);

            foreach (var i in stack)
            {
                accum += i;
            }
            return accum;
        }

    }
}

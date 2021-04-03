using BenchmarkDotNet.Attributes;
using System;
using System.Globalization;
using System.Linq;

// 16進数文字列 を 数値に変換 → int.Parse() が高速
namespace BenchmarkDotNetSample
{
#if false
    [ShortRunJob]
    public class StrHexConvert
    {
        private string[] _sourceArray;

        [GlobalSetup]
        public void Setup()
        {
            _sourceArray = Enumerable.Range(0, Int16.MaxValue).Select(x => x.ToString("x4")).ToArray();
        }

        [Benchmark(Baseline = true)]
        public void UseConvert()
        {
            long sum = 0;
            foreach (var s in _sourceArray)
            {
                sum += Convert.ToInt32(s, 16);
            }
        }

        [Benchmark]
        public void UseParse()
        {
            long sum = 0;
            foreach (var s in _sourceArray)
            {
                sum += int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
        }

        [Benchmark]
        public void UseTryParse()
        {
            long sum = 0;
            foreach (var s in _sourceArray)
            {
                if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var i))
                    sum += i;
            }
        }

    }
#endif
}

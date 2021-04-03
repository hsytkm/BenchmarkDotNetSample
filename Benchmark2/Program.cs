using BenchmarkDotNet.Running;
using System;

namespace Benchmark2
{
    // http://engineering.grani.jp/entry/2017/07/28/145035
    class Program
    {
        static void Main(string[] args)
        {
            // Switcherは複数ベンチマークを作りたい場合ベンリ。
            var switcher = new BenchmarkSwitcher(
                new[]
                {
                    typeof(DictionaryBenchmark),
                });

            // 今回は一個だけなのでSwitcherは不要ですが。
            args = new string[] { "0" };

            switcher.Run(args);
        }
    }

}

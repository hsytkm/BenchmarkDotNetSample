using BenchmarkDotNet.Attributes;
using System;
using System.Globalization;
using System.Linq;

namespace BenchmarkDotNetSample
{
    [ShortRunJob]
    public class SomeTest
    {
        [Params(10, 100, 1000)]
        public int N;

        private int[] data;

        [GlobalSetup]
        public void GlobalSetup()
        {
            data = new int[N]; // executed once per each N value
        }

        [Benchmark(Baseline = true)]
        public void Bench0()
        {
        }

        [Benchmark]
        public void Bench1()
        {
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
        }
    }
}

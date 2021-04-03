using BenchmarkDotNet.Attributes;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Benchmark2
{
    [Config(typeof(BenchmarkConfig))]
    public class DictionaryBenchmark
    {
        private string key;
        private byte[] keybs;
        private readonly Dictionary<string, int> dictStringKey = new();
        private readonly Dictionary<byte[], int> dictByteArrayKey = new();

        [GlobalSetup]
        public void Setup()
        {
            var max = 3000;

            for (int i = 0; i < max; i++)
            {
                var g = Guid.NewGuid().ToString("N");
                dictStringKey.Add(g, i);
                dictByteArrayKey.Add(Encoding.UTF8.GetBytes(g), i);

                if (i == max / 2)
                {
                    key = g;
                    keybs = Encoding.UTF8.GetBytes(g);
                }
            }
        }

        [Benchmark(Baseline = true)]
        public int DictionaryStringKey()
        {
            dictStringKey.TryGetValue(key, out var x);
            return x;
        }

        [Benchmark]
        public int DictionaryByteArrayKey()
        {
            dictByteArrayKey.TryGetValue(keybs, out var x);
            return x;
        }

    }
}

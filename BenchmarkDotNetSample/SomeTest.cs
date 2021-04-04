using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using System;

namespace BenchmarkDotNetSample
{
    public class SomeTestConfig : ManualConfig
    {
        public SomeTestConfig()
        {
            AddExporter(MarkdownExporter.GitHub);
            AddDiagnoser(MemoryDiagnoser.Default);

            // ShortRunは LaunchCount=1  TargetCount=3 WarmupCount = 3 のショートカット
            AddJob(Job.ShortRun);
        }
    }

    [Config(typeof(SomeTestConfig))]
    public class SomeTest
    {
        [Params(10, 100, 1000)]
        public int N;

        private int[]? data;

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

using BenchmarkDotNet.Running;
using System;

namespace BenchmarkDotNetSample
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SomeTest>();
        }
    }
}

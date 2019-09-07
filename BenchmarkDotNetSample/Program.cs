using BenchmarkDotNet.Running;
using System;

namespace BenchmarkDotNetSample
{
    /*
     * https://qiita.com/Tokeiya/items/30d8a76163622a4b5be1
     * 制約
     * 　引数は取れない
     * 　publicメソッド必須
     */

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SomeTest>();
        }
    }
}

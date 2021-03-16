using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace XUnitTestProject1
{
    public static class MyParam
    {
        public const int SIZE = 213;
    }

    [StructLayout(LayoutKind.Sequential, Size = MyParam.SIZE)]
    unsafe struct MyStruct
    {
        public fixed byte fixedBuffer[MyParam.SIZE];
    }


    public class MyTest
    {
        private MyStruct _data;
        private readonly int _answer;

        public MyTest()
        {
            unsafe
            {
                for (var i = 0; i < MyParam.SIZE; ++i)
                {
                    var x = (i % 256);
                    _data.fixedBuffer[i] = (byte)x;
                    _answer += x;
                }
            }
        }

        [Fact]
        public byte[] UseMarshal()
        {
            var bs = MyClass.ToByteArray_UseMarshal(_data);

            Assert.Equal(_answer, bs.Select(x => (int)x).Sum());
            return bs;
        }

        [Fact]
        public byte[] UseGCHandle()
        {
            var bs = MyClass.ToByteArray_UseGCHandle(_data);

            Assert.Equal(_answer, bs.Select(x => (int)x).Sum());
            return bs;
        }

        [Fact]
        public byte[] UseUnsafe()
        {
            var bs = MyClass.ToByteArray_UseUnsafe(_data);

            Assert.Equal(_answer, bs.Select(x => (int)x).Sum());
            return bs;
        }

        [Fact]
        public void Use2()
        {
            var bs0 = MyClass.ToByteArray_UseMarshal(_data);
            var bs1 = MyClass.ToByteArray_UseGCHandle(_data);

            Assert.Equal(bs0, bs1);
        }
    }
}

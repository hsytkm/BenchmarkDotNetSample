using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.InteropServices;

// メモリを確保して、構造体コピー → UseUnsafe が高速
namespace BenchmarkDotNetSample
{
#if true
    [ShortRunJob]
    public class StructAllocCopy
    {
        private MyStructTestStruct _data;

        [GlobalSetup]
        public void Setup()
        {
            unsafe
            {
                for (var i = 0; i < MyStructTestParam.SIZE; ++i)
                {
                    _data.fixedBuffer[i] = (byte)(i % 256);
                }
            }
        }

        [Benchmark(Baseline = true)]
        public byte[] UseMarshal()
        {
            return ToByteArray(_data);

            static byte[] ToByteArray<T>(T data) where T : struct
            {
                var size = Marshal.SizeOf(data);
                var bs = new byte[size];

                IntPtr intPtr = IntPtr.Zero;
                try
                {
                    intPtr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(data, intPtr, false);
                    Marshal.Copy(intPtr, bs, 0, size);
                }
                finally
                {
                    if (intPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(intPtr);
                }
                return bs;
            }
        }

        [Benchmark]
        public byte[] UseGCHandle()
        {
            return ToByteArray(_data);

            static byte[] ToByteArray<T>(T data) where T : struct
            {
                var size = Marshal.SizeOf<T>();
                var bs = new byte[size];

                GCHandle gch = default;
                try
                {
                    gch = GCHandle.Alloc(bs, GCHandleType.Pinned);
                    Marshal.StructureToPtr(data, gch.AddrOfPinnedObject(), false);
                }
                finally
                {
                    if (gch.IsAllocated)
                        gch.Free();
                }
                return bs;
            }
        }

        [Benchmark]
        public byte[] UseUnsafe()
        {
            return ToByteArray(_data);

            static byte[] ToByteArray<T>(T data) where T : unmanaged
            {
                var size = Marshal.SizeOf<T>();
                var bs = new byte[size];

                unsafe
                {
                    fixed (byte* p = bs)
                    {
                        *(T*)p = data;
                    }
                }
                return bs;
            }
        }

    }

    static class MyStructTestParam
    {
        public const int SIZE = 256;
    }

    [StructLayout(LayoutKind.Sequential, Size = MyStructTestParam.SIZE)]
    unsafe struct MyStructTestStruct
    {
        public fixed byte fixedBuffer[MyStructTestParam.SIZE];
    }
#endif
}

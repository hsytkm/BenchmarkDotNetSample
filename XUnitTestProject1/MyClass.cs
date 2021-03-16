using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XUnitTestProject1
{
    class MyClass
    {
        public static byte[] ToByteArray_UseMarshal<T>(T data)
            where T : struct
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

        public static byte[] ToByteArray_UseGCHandle<T>(T data)
            where T : struct
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

        public static byte[] ToByteArray_UseUnsafe<T>(T data)
            where T : unmanaged
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

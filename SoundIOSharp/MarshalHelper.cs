//
// MarshalHelper.cs
//
//      Copyright (c) 2018, Cezary Rojewski
//
// This program is free software; you can redistribute it and/or modify it
// under the terms and conditions of the MIT Licence.
//
// This program is distributed in the hope it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.
//

using System;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    /// <summary>
    /// Internal Marshal helper class which purpose is code size reduction.
    /// </summary>
    internal static class MarshalHelper
    {
        internal static int OffsetOf<T>(string fieldName)
        {
            return (int)Marshal.OffsetOf(typeof(T), fieldName);
        }

        internal static byte ReadByteField<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            return Marshal.ReadByte(ptr, OffsetOf<T>(fieldName));
        }

        internal static int ReadInt32Field<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            return Marshal.ReadInt32(ptr, OffsetOf<T>(fieldName));
        }

        internal static long ReadInt64Field<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            return Marshal.ReadInt64(ptr, OffsetOf<T>(fieldName));
        }

        internal static IntPtr ReadIntPtrField<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            return Marshal.ReadIntPtr(ptr, OffsetOf<T>(fieldName));
        }

        internal static float ReadFloatField<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            byte[] buffer = BitConverter.GetBytes(ReadInt32Field<T>(ptr, fieldName));
            return BitConverter.ToSingle(buffer, 0);
        }

        internal static double ReadDoubleField<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            return BitConverter.Int64BitsToDouble(ReadInt64Field<T>(ptr, fieldName));
        }

        internal static string ReadStringAnsiField<T>(IntPtr ptr, string fieldName)
            where T : struct
        {
            return Marshal.PtrToStringAnsi(ReadIntPtrField<T>(ptr, fieldName));
        }

        internal static void WriteInt32Field<T>(IntPtr ptr, string fieldName, int value)
            where T : struct
        {
            Marshal.WriteInt32(ptr, OffsetOf<T>(fieldName), value);
        }

        internal static void WriteInt64Field<T>(IntPtr ptr, string fieldName, long value)
            where T : struct
        {
            Marshal.WriteInt64(ptr, OffsetOf<T>(fieldName), value);
        }

        internal static void WriteIntPtrField<T>(IntPtr ptr, string fieldName, IntPtr value)
            where T : struct
        {
            Marshal.WriteIntPtr(ptr, OffsetOf<T>(fieldName), value);
        }

        internal static void WriteDoubleField<T>(IntPtr ptr, string fieldName, double value)
            where T : struct
        {
            WriteInt64Field<T>(ptr, fieldName, BitConverter.DoubleToInt64Bits(value));
        }

        internal static void WriteStringAnsiField<T>(IntPtr ptr, string fieldName, string value)
            where T : struct
        {
            WriteIntPtrField<T>(ptr, fieldName, Marshal.StringToHGlobalAnsi(value));
        }

        internal static byte[] StructureToBytes<T>(T str)
            where T : struct
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            GCHandle h = default(GCHandle);

            try
            {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);
                Marshal.StructureToPtr(str, h.AddrOfPinnedObject(), false);
            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }

            return arr;
        }
    }
}

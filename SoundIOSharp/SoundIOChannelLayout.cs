//
// SoundIOChannelLayout.cs
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
    /// Channel mapping, specifies where each channel is physically located.
    /// </summary>
    public class SoundIOChannelLayout : IEquatable<SoundIOChannelLayout>
    {
        private IntPtr handle;
        private SoundIoChannelId[] channels;

        internal SoundIOChannelLayout(IntPtr layout)
        {
            IntPtr ptr;
            int offset, size;

            handle = layout;

            channels = new SoundIoChannelId[24];
            offset = (int)Marshal.OffsetOf(typeof(SoundIoChannelLayout), "Channels");
            ptr = handle + offset;
            size = sizeof(int);
            for (int n = 0; n < 24; n++)
            {
                channels[n] = (SoundIoChannelId)Marshal.ReadInt32(ptr);
                ptr += size;
            }
        }

        /// <summary>
        /// Pointer to the underlying native object.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return handle;
            }
        }

        /// <summary>
        /// Name assigned to channel layout.
        /// </summary>
        public string Name
        {
            get
            {
                return MarshalHelper.ReadStringAnsiField<SoundIoChannelLayout>(handle, "Name");
            }
        }

        /// <summary>
        /// Number of channels contained in channel layout.
        /// </summary>
        public int ChannelCount
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoChannelLayout>(handle, "ChannelCount");
            }
        }

        /// <summary>
        /// Sequence of channel ids <see cref="SoundIoChannelLayout"/> consists of.
        /// </summary>
        public SoundIoChannelId[] Channels
        {
            get
            {
                return channels;
            }
        }

        /// <summary>
        /// Converts string representation of enumerated constant to an equivalent enumerated object.
        /// Returns <see cref="SoundIoChannelId.Invalid"/> if value does not correspond to any channel id.
        /// </summary>
        /// <param name="value">String value to parse.</param>
        /// <returns><see cref="SoundIoChannelId"/> object whose value is represented by value.</returns>
        public static SoundIoChannelId ParseChannelId(string value)
        {
            return NativeMethods.SoundIoParseChannelId(value, value.Length);
        }

        /// <summary>
        /// Whether this channel layout equals another, specified <see cref="SoundIOChannelLayout"/> instance.
        /// </summary>
        /// <param name="other"><see cref="SoundIOChannelLayout"/> instance to compare to.</param>
        /// <returns>Value indicating whether layouts are equal.</returns>
        public bool Equals(SoundIOChannelLayout other)
        {
            return NativeMethods.SoundIoChannelLayoutEqual(handle, other.handle);
        }

        /// <summary>
        /// Returns number of builtin channel layouts.
        /// </summary>
        public static int BuiltinLayoutCount
        {
            get
            {
                return NativeMethods.SoundIoChannelLayoutBuiltinCount();
            }
        }

        /// <summary>
        /// Retrieves builtin <see cref="SoundIOChannelLayout"/> instance at specified index or null if not found.
        /// </summary>
        /// <param name="index">Index of channel layout to retrieve.</param>
        /// <returns>Instance of <see cref="SoundIOChannelLayout"/> or null if not found.</returns>
        public static SoundIOChannelLayout GetBuiltin(int index)
        {
            if (index < 0 || index >= BuiltinLayoutCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            IntPtr ptr = NativeMethods.SoundIoChannelLayoutGetBuiltin(index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIOChannelLayout(ptr);
        }

        /// <summary>
        /// Retrieves default <see cref="SoundIOChannelLayout"/> instance for specified
        /// channel count or null if not found.
        /// </summary>
        /// <param name="channelCount">Number of channels for which to retrieve channel layout.</param>
        /// <returns>Instance of <see cref="SoundIOChannelLayout"/> or null if not found.</returns>
        public static SoundIOChannelLayout GetDefault(int channelCount)
        {
            IntPtr ptr = NativeMethods.SoundIoChannelLayoutGetDefault(channelCount);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIOChannelLayout(ptr);
        }

        /// <summary>
        /// Searches for <see cref="SoundIoChannelId"/> object within <see cref="SoundIoChannelLayout"/>
        /// and returns zero-based index of element found.
        /// </summary>
        /// <param name="channel">Channel id to search for.</param>
        /// <returns>Zero-based index of object or -1 if not found.</returns>
        public int FindChannel(SoundIoChannelId channel)
        {
            return NativeMethods.SoundIoChannelLayoutFindChannel(handle, channel);
        }

        /// <summary>
        /// Populates the <see cref="Name"/> field of layout if it matches a builtin one.
        /// </summary>
        /// <returns>Value indicating whether match has been found.</returns>
        public bool DetectBuiltin()
        {
            return NativeMethods.SoundIoChannelLayoutDetectBuiltin(handle);
        }
    }
}

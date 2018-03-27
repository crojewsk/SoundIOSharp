//
// SoundIOChannelAreas.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    /// <summary>
    /// Represents data block for single channel.
    /// </summary>
    public class SoundIOChannelArea
    {
        private IntPtr handle;

        internal SoundIOChannelArea(IntPtr area)
        {
            handle = area;
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
        /// Base address of underlying buffer.
        /// </summary>
        public IntPtr Pointer
        {
            get
            {
                return MarshalHelper.ReadIntPtrField<SoundIoChannelArea>(handle, "Ptr");
            }
            set
            {
                MarshalHelper.WriteIntPtrField<SoundIoChannelArea>(handle, "Ptr", value);
            }
        }

        /// <summary>
        /// Offset in bytes from the beginning of one sample to the beginning of the next sample.
        /// </summary>
        public int Step
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoChannelArea>(handle, "Step");
            }
        }

        /// <summary>
        /// Advances <see cref="Pointer"/> by <see cref="Step"/> bytes.
        /// </summary>
        public void AdvancePointer()
        {
            Pointer += Step;
        }
    }

    /// <summary>
    /// Represents data block for entire channel layout.
    /// </summary>
    public class SoundIOChannelAreas : IEnumerable<SoundIOChannelArea>
    {
        private IntPtr handle;
        private SoundIOChannelArea[] areas;

        internal SoundIOChannelAreas(IntPtr head, int channelCount, int frameCount)
        {
            int size = Marshal.SizeOf(typeof(SoundIoChannelArea));
            handle = head;
            areas = new SoundIOChannelArea[channelCount];
            for (int n = 0; n < channelCount; n++)
            {
                areas[n] = new SoundIOChannelArea(head + (n * size));
            }

            FrameCount = frameCount;
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
        /// Number of channels for this data block.
        /// </summary>
        public int Count
        {
            get
            {
                return areas.Length;
            }
        }

        /// <summary>
        /// Number of frames for this data block
        /// </summary>
        public int FrameCount { get; internal set; }

        /// <summary>
        /// Returns data block for specified channel index.
        /// </summary>
        /// <param name="index">Index of channel for which to return data block.</param>
        /// <returns>Data block for specified channel.</returns>
        public SoundIOChannelArea this[int index]
        {
            get
            {
                return areas[index];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SoundIOChannelAreas"/>.
        /// </summary>
        /// <returns>Enumerator for iterating over channel areas.</returns>
        public IEnumerator<SoundIOChannelArea> GetEnumerator()
        {
            return new ChannelAreasEnumerator(areas);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Enumerates through the elements of <see cref="SoundIOChannelAreas"/>.
        /// </summary>
        public struct ChannelAreasEnumerator : IEnumerator<SoundIOChannelArea>
        {
            SoundIOChannelArea[] areas;
            int memberIndex;

            public ChannelAreasEnumerator(SoundIOChannelArea[] arr)
            {
                areas = arr;
                memberIndex = -1;
            }

            public bool MoveNext()
            {
                memberIndex++;
                return (memberIndex < areas.Length);
            }

            public void Reset()
            {
                memberIndex = -1;
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public SoundIOChannelArea Current
            {
                get
                {
                    try
                    {
                        return areas[memberIndex];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }
    }
}

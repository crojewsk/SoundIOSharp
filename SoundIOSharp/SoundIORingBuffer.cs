//
// SoundIORingBuffer.cs
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

namespace SoundIOSharp
{
    /// <summary>
    /// A ring buffer is a single-reader single-writer lock-free fixed-size queue.
    /// </summary>
    public class SoundIORingBuffer : IDisposable
    {
        private IntPtr handle;

        /// <summary>
        /// Initializes new instance of <see cref="SoundIORingBuffer"/> class with specified capacity.
        /// </summary>
        /// <param name="soundIo">Io unit for which to initialize buffer.</param>
        /// <param name="requestedCapacity">
        /// Minimum required capacity in bytes. Actual capacity might be greater than the requested one.
        /// </param>
        public SoundIORingBuffer(SoundIO soundIo, int requestedCapacity)
        {
            handle = NativeMethods.SoundIoRingBufferCreate(soundIo.Handle, requestedCapacity);
        }

        ~SoundIORingBuffer()
        {
            Dispose(false);
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
        /// Actual buffer capacity.
        /// </summary>
        public int Capacity
        {
            get
            {
                return NativeMethods.SoundIoRingBufferCapacity(handle);
            }
        }

        /// <summary>
        /// Write pointer of the ring buffer.
        /// </summary>
        public IntPtr WritePointer
        {
            get
            {
                return NativeMethods.SoundIoRingBufferWritePtr(handle);
            }
        }

        /// <summary>
        /// Advances write pointer by specified amount of bytes.
        /// </summary>
        /// <param name="count">Number of bytes to advance pointer by.</param>
        public void AdvanceWritePointer(int count)
        {
            NativeMethods.SoundIoRingBufferAdvanceWritePtr(handle, count);
        }

        /// <summary>
        /// Read pointer of the ring buffer.
        /// </summary>
        public IntPtr ReadPointer
        {
            get
            {
                return NativeMethods.SoundIoRingBufferReadPtr(handle);
            }
        }

        /// <summary>
        /// Advances read pointer by specified amount of bytes.
        /// </summary>
        /// <param name="count">Number of bytes to advance pointer by.</param>
        public void AdvanceReadPointer(int count)
        {
            NativeMethods.SoundIoRingBufferAdvanceReadPtr(handle, count);
        }

        /// <summary>
        /// Number of bytes used, ready for reading.
        /// </summary>
        public int FillCount
        {
            get
            {
                return NativeMethods.SoundIoRingBufferFillCount(handle);
            }
        }

        /// <summary>
        /// Number of bytes free, ready for writing.
        /// </summary>
        public int FreeCount
        {
            get
            {
                return NativeMethods.SoundIoRingBufferFreeCount(handle);
            }
        }

        /// <summary>
        /// Clears the ring buffer.
        /// </summary>
        public void Clear()
        {
            NativeMethods.SoundIoRingBufferClear(handle);
        }

        /// <summary>
        /// Releases all resources used by <see cref="SoundIORingBuffer"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Disposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    // Release managed resources
                }

                if (handle != IntPtr.Zero)
                {
                    NativeMethods.SoundIoRingBufferDestroy(handle);
                }

                Disposed = true;
            }
        }
    }
}

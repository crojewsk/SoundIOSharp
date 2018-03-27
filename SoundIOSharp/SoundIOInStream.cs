//
// SoundIOInStream.cs
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
    /// Transfer unit, responsible for streaming data from recording device.
    /// </summary>
    public class SoundIOInStream : IDisposable
    {
        private IntPtr handle;
        private Action<SoundIOInStream, int, int> onReadCallback;
        private Action<SoundIOInStream> onOverflowCallback;
        private Action<SoundIOInStream, SoundIoError> onErrorCallback;

        /// <summary>
        /// Initializes new instance of <see cref="SoundIOInStream"/> class with specified sound device.
        /// </summary>
        /// <param name="device">Recording device to stream with.</param>
        /// <remarks>Increments ref count for specified device.</remarks>
        public SoundIOInStream(SoundIODevice device)
        {
            handle = NativeMethods.SoundIoInstreamCreate(device.Handle);
            Device = device;
        }

        ~SoundIOInStream()
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
        /// Recording device.
        /// </summary>
        public SoundIODevice Device { get; private set; }

        /// <summary>
        /// Stream wave format.
        /// </summary>
        public SoundIoFormat Format
        {
            get
            {
                return (SoundIoFormat)MarshalHelper.ReadInt32Field<SoundIoInStream>(handle, "Format");
            }
            set
            {
                MarshalHelper.WriteInt32Field<SoundIoInStream>(handle, "Format", (int)value);
            }
        }

        /// <summary>
        /// Stream sample rate.
        /// </summary>
        public int SampleRate
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoInStream>(handle, "SampleRate");
            }
            set
            {
                MarshalHelper.WriteInt32Field<SoundIoInStream>(handle, "SampleRate", value);
            }
        }

        /// <summary>
        /// Stream channel layout.
        /// </summary>
        public SoundIOChannelLayout Layout
        {
            get
            {
                int offset = (int)Marshal.OffsetOf(typeof(SoundIoInStream), "Layout");
                return new SoundIOChannelLayout(handle + offset);
            }
            set
            {
                SoundIoChannelLayout toWrite;
                if (value != null)
                {
                    toWrite = (SoundIoChannelLayout)Marshal.PtrToStructure(value.Handle, typeof(SoundIoChannelLayout));
                }
                else
                {
                    toWrite = default(SoundIoChannelLayout);
                }

                int offset = (int)Marshal.OffsetOf(typeof(SoundIoInStream), "Layout");
                int size = Marshal.SizeOf(typeof(SoundIoChannelLayout));
                byte[] buffer = MarshalHelper.StructureToBytes(toWrite);
                Marshal.Copy(buffer, 0, handle + offset, size);
            }
        }

        /// <summary>
        /// Ignoring hardware latency, this is the number of seconds it takes for a
        /// captured sample to become available for reading.
        /// </summary>
        public double SoftwareLatency
        {
            get
            {
                return MarshalHelper.ReadDoubleField<SoundIoInStream>(handle, "SoftwareLatency");
            }
            set
            {
                MarshalHelper.WriteDoubleField<SoundIoInStream>(handle, "SoftwareLatency", value);
            }
        }

        /// <summary>
        /// Optional, user specific data.
        /// </summary>
        public IntPtr UserData
        {
            get
            {
                return MarshalHelper.ReadIntPtrField<SoundIoInStream>(handle, "UserData");
            }
            set
            {
                MarshalHelper.WriteIntPtrField<SoundIoInStream>(handle, "UserData", value);
            }
        }

        /// <summary>
        /// Context for calling BeingRead and EndRead as many times as necessary to read number of frames
        /// between the provided minimum and maximum. Returning from callback without having read
        /// at least the required minimum, causes the frames to be dropped.
        /// </summary>
        /// <remarks>
        /// The code in the supplied function must be suitable for real-time execution. That means that
        /// it cannot call functions that might block for a long time. This includes all I/O functions
        /// (disk, TTY, network), malloc, free, printf, pthread_mutex_lock, sleep, wait, poll, select etc.
        /// </remarks>
        public Action<SoundIOInStream, int, int> OnReadCallback
        {
            get
            {
                return onReadCallback;
            }
            set
            {
                onReadCallback = value;
                SoundIoInStream.ReadCallback readCallback = null;
                if (value != null)
                {
                    readCallback = (s, fmin, fmax) => onReadCallback(this, fmin, fmax);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(readCallback);
                MarshalHelper.WriteIntPtrField<SoundIoInStream>(handle, "OnReadCallback", ptr);
            }
        }

        /// <summary>
        /// Optional callback. Called when the sound device buffer is full,
        /// yet there is more captured audio to put in it.
        /// </summary>
        /// <remarks>Called from the OnReadCallback thread context.</remarks>
        public Action<SoundIOInStream> OnOverflowCallback
        {
            get
            {
                return onOverflowCallback;
            }
            set
            {
                onOverflowCallback = value;
                SoundIoInStream.OverflowCallback overflowCallback = null;
                if (value != null)
                {
                    overflowCallback = (s) => onOverflowCallback(this);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(overflowCallback);
                MarshalHelper.WriteIntPtrField<SoundIoInStream>(handle, "OnOverflowCallback", ptr);
            }
        }

        /// <summary>
        /// Optional callback. Provided error is always <see cref="SoundIoError.Streaming"/> which
        /// states that the stream is in an invalid state and must be destroyed.
        /// </summary>
        /// <remarks>Called from the OnReadCallback thread context.</remarks>
        public Action<SoundIOInStream, SoundIoError> OnErrorCallback
        {
            get
            {
                return onErrorCallback;
            }
            set
            {
                onErrorCallback = value;
                SoundIoInStream.ErrorCallback errorCallback = null;
                if (value != null)
                {
                    errorCallback = (s, err) => onErrorCallback(this, err);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(errorCallback);
                MarshalHelper.WriteIntPtrField<SoundIoInStream>(handle, "OnErrorCallback", ptr);
            }
        }

        /// <summary>
        /// Name of the stream. Optional, backend dependant. Defaults to "SoundIoInStream".
        /// </summary>
        public string Name
        {
            get
            {
                return MarshalHelper.ReadStringAnsiField<SoundIoInStream>(handle, "Name");
            }
            set
            {
                MarshalHelper.WriteStringAnsiField<SoundIoInStream>(handle, "Name", value);
            }
        }

        /// <summary>
        /// Indicates that the data received by the stream will be passed on or made
        /// available to another stream. Optional.
        /// </summary>
        public bool NonTerminalHint
        {
            get
            {
                return MarshalHelper.ReadByteField<SoundIoInStream>(handle, "NonTerminalHint") != 0;
            }
        }

        /// <summary>
        /// Number of bytes used by the stream per frame.
        /// </summary>
        public int BytesPerFrame
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoInStream>(handle, "BytesPerFrame");
            }
        }

        /// <summary>
        /// Number of bytes used by the sample per frame.
        /// </summary>
        public int BytesPerSample
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoInStream>(handle, "BytesPerSample");
            }
        }

        /// <summary>
        /// Result of setting the channel layout. Possible error codes are: <see cref="SoundIoError.IncompatibleDevice"/>.
        /// </summary>
        public SoundIoError LayoutError
        {
            get
            {
                return (SoundIoError)MarshalHelper.ReadInt32Field<SoundIoInStream>(handle, "LayoutError");
            }
        }

        /// <summary>
        /// Opens the stream for recording.
        /// </summary>
        /// <returns>Error code indicating operation result.</returns>
        public SoundIoError Open()
        {
            return NativeMethods.SoundIoInstreamOpen(handle);
        }

        /// <summary>
        /// Starts recording.
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError Start()
        {
            return NativeMethods.SoundIoInstreamStart(handle);
        }

        /// <summary>
        /// Call this function when you are ready to begin reading from the device buffer.
        /// </summary>
        /// <param name="areas">The memory area data can be read from.</param>
        /// <param name="frameCount">Positive number of frames to read, returns the number of frames that will be actually read.</param>
        /// <returns>Value indicating operation success.</returns>
        /// <remarks>
        /// Call this function only from the OnReadCallback thread context. When done reading from areas, call EndRead.
        /// </remarks>
        public SoundIoError BeginRead(out SoundIOChannelAreas areas, ref int frameCount)
        {
            IntPtr head;
            SoundIoError result = NativeMethods.SoundIoInstreamBeginRead(handle, out head, ref frameCount);

            if (head == IntPtr.Zero)
            {
                areas = null;
            }
            else
            {
                areas = new SoundIOChannelAreas(head, Layout.ChannelCount, frameCount);
            }

            return result;
        }

        /// <summary>
        /// Drops all of the frames from when BeginRead was called. Call only after a successful BeginRead.
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        /// <remarks>Call this function only from the OnReadCallback thread context.</remarks>
        public SoundIoError EndRead()
        {
            return NativeMethods.SoundIoInstreamEndRead(handle);
        }

        /// <summary>
        /// Pauses recording stream.
        /// </summary>
        /// <param name="pause">Whether to pause or unpause the stream.</param>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError Pause(bool pause)
        {
            return NativeMethods.SoundIoInstreamPause(handle, pause);
        }

        /// <summary>
        /// Number of seconds that the next frame of sound being
        /// captured will take to arrive in the buffer, plus the amount of time that is
        /// represented in the buffer. This includes both software and hardware latency.
        /// </summary>
        /// <param name="latency">Value to set result latency to.</param>
        /// <returns>Value indicating operation success.</returns>
        /// <remarks>Call this function only from within OnReadCallback.</remarks>
        public SoundIoError GetLatency(out double latency)
        {
            return NativeMethods.SoundIoInstreamGetLatency(handle, out latency);
        }

        /// <summary>
        /// Releases all resources used by the stream.
        /// </summary>
        /// <remarks>Decrements ref count for the attached device.</remarks>
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
                    // Internaly calls unref on attached device
                    NativeMethods.SoundIoInstreamDestroy(handle);
                }

                Disposed = true;
            }
        }
    }
}

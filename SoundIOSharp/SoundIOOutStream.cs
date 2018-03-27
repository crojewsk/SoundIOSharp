//
// SoundIOOutStream.cs
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
    /// Transfer unit, responsible for streaming data over to render device.
    /// </summary>
    public class SoundIOOutStream : IDisposable
    {
        private IntPtr handle;
        private Action<SoundIOOutStream, int, int> onWriteCallback;
        private Action<SoundIOOutStream> onUnderflowCallback;
        private Action<SoundIOOutStream, SoundIoError> onErrorCallback;

        /// <summary>
        /// Initializes new instance of <see cref="SoundIOOutStream"/> class with specified sound device.
        /// </summary>
        /// <param name="device">Render device to stream with.</param>
        /// <remarks>Increments ref count for specified device.</remarks>
        public SoundIOOutStream(SoundIODevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            handle = NativeMethods.SoundIoOutstreamCreate(device.Handle);
            Device = device;
        }

        ~SoundIOOutStream()
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
        /// Render device.
        /// </summary>
        public SoundIODevice Device { get; private set; }

        /// <summary>
        /// Stream wave format.
        /// </summary>
        public SoundIoFormat Format
        {
            get
            {
                return (SoundIoFormat)MarshalHelper.ReadInt32Field<SoundIoOutStream>(handle, "Format");
            }
            set
            {
                MarshalHelper.WriteInt32Field<SoundIoOutStream>(handle, "Format", (int)value);
            }
        }

        /// <summary>
        /// Stream sample rate.
        /// </summary>
        public int SampleRate
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoOutStream>(handle, "SampleRate");
            }
            set
            {
                MarshalHelper.WriteInt32Field<SoundIoOutStream>(handle, "SampleRate", value);
            }
        }

        /// <summary>
        /// Stream channel layout.
        /// </summary>
        public SoundIOChannelLayout Layout
        {
            get
            {
                int offset = (int)Marshal.OffsetOf(typeof(SoundIoOutStream), "Layout");
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

                int offset = (int)Marshal.OffsetOf(typeof(SoundIoOutStream), "Layout");
                int size = Marshal.SizeOf(typeof(SoundIoChannelLayout));
                byte[] buffer = MarshalHelper.StructureToBytes(toWrite);
                Marshal.Copy(buffer, 0, handle + offset, size);
            }
        }

        /// <summary>
        /// Ignoring hardware latency, this is the number of seconds it takes for
        /// the last sample in a full buffer to be played.
        /// </summary>
        public double SoftwareLatency
        {
            get
            {
                return MarshalHelper.ReadDoubleField<SoundIoOutStream>(handle, "SoftwareLatency");
            }
            set
            {
                MarshalHelper.WriteDoubleField<SoundIoOutStream>(handle, "SoftwareLatency", value);
            }
        }

        /// <summary>
        /// Stream volume in range 0.0 - 1.0. Backend dependent.
        /// </summary>
        public float Volume
        {
            get
            {
                return MarshalHelper.ReadFloatField<SoundIoOutStream>(handle, "Volume");
            }
        }

        /// <summary>
        /// Optional, user specific data.
        /// </summary>
        public IntPtr UserData
        {
            get
            {
                return MarshalHelper.ReadIntPtrField<SoundIoOutStream>(handle, "UserData");
            }
            set
            {
                MarshalHelper.WriteIntPtrField<SoundIoOutStream>(handle, "UserData", value);
            }
        }

        /// <summary>
        /// Context for calling BeginWrite and EndWrite as many times as necessary to write
        /// the number of frames between the provided minimum and maximum. To prevent buffer underflow,
        /// it is best to write as many frames as available.
        /// </summary>
        /// <remarks>
        /// The code in the supplied function must be suitable for real-time execution. That means that
        /// it cannot call functions that might block for a long time. This includes all I/O functions
        /// (disk, TTY, network), malloc, free, printf, pthread_mutex_lock, sleep, wait, poll, select etc.
        /// </remarks>
        public Action<SoundIOOutStream, int, int> OnWriteCallback
        {
            get
            {
                return onWriteCallback;
            }
            set
            {
                onWriteCallback = value;
                SoundIoOutStream.WriteCallback writeCallback = null;
                if (value != null)
                {
                    writeCallback = (s, min, max) => onWriteCallback(this, min, max);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(writeCallback);
                MarshalHelper.WriteIntPtrField<SoundIoOutStream>(handle, "OnWriteCallback", ptr);
            }
        }

        /// <summary>
        /// Optional callback. Called when the sound device runs out of buffered audio data to play.
        /// After this occurs, the outstream waits until the buffer is full to resume playback.
        /// </summary>
        /// <remarks>Called from the OnWriteCallback thread context.</remarks>
        public Action<SoundIOOutStream> OnUnderflowCallback
        {
            get
            {
                return onUnderflowCallback;
            }
            set
            {
                onUnderflowCallback = value;
                SoundIoOutStream.UnderflowCallback underflowCallback = null;
                if (value != null)
                {
                    underflowCallback = (s) => onUnderflowCallback(this);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(underflowCallback);
                MarshalHelper.WriteIntPtrField<SoundIoOutStream>(handle, "OnUnderflowCallback", ptr);
            }
        }

        /// <summary>
        /// Optional callback. Provided error is always <see cref="SoundIoError.Streaming"/> which
        /// states that the stream is in an invalid state and must be destroyed.
        /// </summary>
        /// <remarks>Called from the OnWriteCallback thread context.</remarks></remarks>
        public Action<SoundIOOutStream, SoundIoError> OnErrorCallback
        {
            get
            {
                return onErrorCallback;
            }
            set
            {
                onErrorCallback = value;
                SoundIoOutStream.ErrorCallback errorCallback = null;
                if (value != null)
                {
                    errorCallback = (s, err) => onErrorCallback(this, err);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(errorCallback);
                MarshalHelper.WriteIntPtrField<SoundIoOutStream>(handle, "OnErrorCallback", ptr);
            }
        }

        /// <summary>
        /// Name of the stream. Optional, backend dependant. Defaults to "SoundIoOutStream".
        /// </summary>
        public string Name
        {
            get
            {
                return MarshalHelper.ReadStringAnsiField<SoundIoOutStream>(handle, "Name");
            }
            set
            {
                MarshalHelper.WriteStringAnsiField<SoundIoOutStream>(handle, "Name", value);
            }
        }

        /// <summary>
        /// Indicates that the output stream data originates from an input stream. Optional.
        /// </summary>
        public bool NonTerminalHint
        {
            get
            {
                return MarshalHelper.ReadByteField<SoundIoOutStream>(handle, "NonTerminalHint") != 0;
            }
        }

        /// <summary>
        /// Number of bytes used by the stream per frame.
        /// </summary>
        public int BytesPerFrame
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoOutStream>(handle, "BytesPerFrame");
            }
        }

        /// <summary>
        /// Number of bytes used by the sample per frame.
        /// </summary>
        public int BytesPerSample
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoOutStream>(handle, "BytesPerSample");
            }
        }

        /// <summary>
        /// Result of setting the channel layout. Possible error codes are: <see cref="SoundIoError.IncompatibleDevice"/>.
        /// </summary>
        public SoundIoError LayoutError
        {
            get
            {
                return (SoundIoError)MarshalHelper.ReadInt32Field<SoundIoOutStream>(handle, "LayoutError");
            }
        }

        /// <summary>
        /// Opens the stream for playback.
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError Open()
        {
            return NativeMethods.SoundIoOutstreamOpen(handle);
        }

        /// <summary>
        /// Starts playback.
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError Start()
        {
            return NativeMethods.SoundIoOutstreamStart(handle);
        }

        /// <summary>
        /// Call this function when you are ready to begin writing to the device buffer.
        /// </summary>
        /// <param name="areas">The memory area data can be written to, one per channel.</param>
        /// <param name="frameCount">Positive number of frames to write, returns the number of frames that will be actually written.</param>
        /// <returns>Value indicating operation success.</returns>
        /// <remarks>
        /// Call this function only from the OnWriteCallback thread context. When done writing to areas, call EndWrite.
        /// </remarks>
        public SoundIoError BeginWrite(out SoundIOChannelAreas areas, ref int frameCount)
        {
            IntPtr head;
            SoundIoError result = NativeMethods.SoundIoOutstreamBeginWrite(handle, out head, ref frameCount);
            
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
        /// Commits the write that you began with BeginWrite.
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        /// <remarks>Call this function only from the OnWriteCallback thread context.</remarks>
        public SoundIoError EndWrite()
        {
            return NativeMethods.SoundIoOutstreamEndWrite(handle);
        }

        /// <summary>
        /// Clears the output stream buffer.
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        /// <remarks>
        /// Can be called from any thread, regardless of whether the outstream is paused or not.
        /// Returns <see cref="SoundIoError.IncompatibleBackend"/> if not supported by given backend.
        /// </remarks>
        public SoundIoError ClearBuffer()
        {
            return NativeMethods.SoundIoOutstreamClearBuffer(handle);
        }

        /// <summary>
        /// Pauses playback stream.
        /// </summary>
        /// <param name="pause">Whether to pause or unpause the stream.</param>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError Pause(bool pause)
        {
            return NativeMethods.SoundIoOutstreamPause(handle, pause);
        }

        /// <summary>
        /// Number of seconds that the next frame written after the last frame written with EndWrite
        /// will take to become audible. This includes both software and hardware latency.
        /// </summary>
        /// <remarks>Call this function only from within OnWriteCallback.</remarks>
        public SoundIoError GetLatency(out double latency)
        {
            return NativeMethods.SoundIoOutstreamGetLatency(handle, out latency);
        }

        /// <summary>
        /// Sets the volume for stream, range 0.0 - 1.0.
        /// </summary>
        /// <param name="volume">Volume to set.</param>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError SetVolume(double volume)
        {
            return NativeMethods.SoundIoOutstreamSetVolume(handle, volume);
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
                    NativeMethods.SoundIoOutstreamDestroy(handle);
                }

                Disposed = true;
            }
        }
    }
}

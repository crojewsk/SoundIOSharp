//
// SoundIO.cs
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
    /// Provides information about input and output devices and backend sound interfaces.
    /// </summary>
    public class SoundIO : IEnumerable<SoundIODevice>, IDisposable
    {
        private IntPtr handle;
        private static Version version;
        private Action<SoundIO> onDevicesChange;
        private Action<SoundIO, SoundIoError> onBackendDisconnect;
        private Action<SoundIO> onEventsSignal;
        private Action onEmitRtPrioWarning;
        private Action<string> onJackInfoCallback;
        private Action<string> onJackErrorCallback;

        static SoundIO()
        {
            int major = NativeMethods.SoundIoVersionMajor();
            int minor = NativeMethods.SoundIoVersionMinor();
            int patch = NativeMethods.SoundIoVersionPatch();
            version = new Version(major, minor, patch);
        }

        /// <summary>
        /// Initializes new instance of <see cref="SoundIO"/> class.
        /// </summary>
        public SoundIO()
        {
            handle = NativeMethods.SoundIoCreate();
        }

        ~SoundIO()
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
        /// Optional, user specific data.
        /// </summary>
        public IntPtr UserData
        {
            get
            {
                return Marshal.ReadIntPtr(handle);
            }
            set
            {
                Marshal.WriteIntPtr(handle, value);
            }
        }

        /// <summary>
        /// Optional callback. Called when the list of devices change.
        /// </summary>
        /// <remarks>Called only during a call to FlushEvents or WaitEvents.</remarks>
        public Action<SoundIO> OnDevicesChange
        {
            get
            {
                return onDevicesChange;
            }
            set
            {
                onDevicesChange = value;
                SoundIo.DevicesChange devicesChange = null;
                if (value != null)
                {
                    devicesChange = sio => onDevicesChange(this);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(devicesChange);
                MarshalHelper.WriteIntPtrField<SoundIo>(handle, "OnDevicesChange", ptr);
            }
        }

        /// <summary>
        /// Optional callback. Called when the backend disconnects. For example, when the JACK
        /// server shuts down. When this happens, listing devices and opening streams will
        /// always fail with <see cref="SoundIoError.BackendDisconnected"/>.
        /// </summary>
        /// <remarks>Called only during a call to FlushEvents or WaitEvents.</remarks>
        public Action<SoundIO, SoundIoError> OnBackendDisconnect
        {
            get
            {
                return onBackendDisconnect;
            }
            set
            {
                onBackendDisconnect = value;
                SoundIo.BackendDisconnect backendDisconnect = null;
                if (value != null)
                {
                    backendDisconnect = (sio, err) => onBackendDisconnect(this, err);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(backendDisconnect);
                MarshalHelper.WriteIntPtrField<SoundIo>(handle, "OnBackendDisconnect", ptr);
            }
        }

        /// <summary>
        /// Optional callback. Called from an unknown thread that should not used to call any
        /// soundio functions. Can be used to signal a condition variable to wake up.
        /// </summary>
        /// <remarks>Called when WaitEvents would be woken up.</remarks>
        public Action<SoundIO> OnEventsSignal
        {
            get
            {
                return onEventsSignal;
            }
            set
            {
                onEventsSignal = value;
                SoundIo.DevicesChange eventsSignal = null;
                if (value != null)
                {
                    eventsSignal = sio => onEventsSignal(this);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(eventsSignal);
                MarshalHelper.WriteIntPtrField<SoundIo>(handle, "OnEventsSignal", ptr);
            }
        }

        /// <summary>
        /// Backend sound interface currently connected to.
        /// </summary>
        public SoundIoBackend CurrentBackend
        {
            get
            {
                return (SoundIoBackend)MarshalHelper.ReadInt32Field<SoundIo>(handle, "CurrentBackend");
            }
        }

        /// <summary>
        /// Optional, application name. Backend dependant.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return MarshalHelper.ReadStringAnsiField<SoundIo>(handle, "AppName");
            }
            set
            {
                MarshalHelper.WriteStringAnsiField<SoundIo>(handle, "AppName", value);
            }
        }

        /// <summary>
        /// Optional callback. Real time priority warning.
        /// This callback is fired when making thread real-time priority failed.
        /// </summary>
        public Action OnEmitRtPrioWarning
        {
            get
            {
                return onEmitRtPrioWarning;
            }
            set
            {
                onEmitRtPrioWarning = value;
                SoundIo.EmitRtPrioWarning emitRtPrioWarning = null;
                if (value != null)
                {
                    emitRtPrioWarning = () => onEmitRtPrioWarning();
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(emitRtPrioWarning);
                MarshalHelper.WriteIntPtrField<SoundIo>(handle, "OnEmitRtPrioWarning", ptr);
            }
        }

        /// <summary>
        /// Optional JACK (backend) info callback.
        /// </summary>
        public Action<string> OnJackInfo
        {
            get
            {
                return onJackInfoCallback;
            }
            set
            {
                onJackInfoCallback = value;
                SoundIo.JackInfoCallback emitRtPrioWarning = null;
                if (value != null)
                {
                    emitRtPrioWarning = msg => onJackInfoCallback(msg);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(emitRtPrioWarning);
                MarshalHelper.WriteIntPtrField<SoundIo>(handle, "OnJackInfoCallback", ptr);
            }
        }

        /// <summary>
        /// Optional JACK (backend) error callback.
        /// </summary>
        public Action<string> OnJackError
        {
            get
            {
                return onJackErrorCallback;
            }
            set
            {
                onJackErrorCallback = value;
                SoundIo.JackErrorCallback jackErrorCallback = null;
                if (value != null)
                {
                    jackErrorCallback = msg => onJackErrorCallback(msg);
                }

                IntPtr ptr = Marshal.GetFunctionPointerForDelegate(jackErrorCallback);
                MarshalHelper.WriteIntPtrField<SoundIo>(handle, "OnJackErrorCallback", ptr);
            }
        }

        /// <summary>
        /// String representation of libsoundio library version.
        /// </summary>
        public static string VersionString
        {
            get
            {
                IntPtr ptr = NativeMethods.SoundIoVersionString();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <summary>
        /// Version of libsoundio library used.
        /// </summary>
        public static Version Version
        {
            get
            {
                return version;
            }
        }

        /// <summary>
        /// Connects to the first available backend.
        /// Order of connection is defined by <see cref="SoundIoBackend"/> (ignoring None).
        /// </summary>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError Connect()
        {
            return NativeMethods.SoundIoConnect(handle);
        }

        /// <summary>
        /// Attempts to connect to the specified backend.
        /// </summary>
        /// <param name="backend">Sound interface to connect to.</param>
        /// <returns>Value indicating operation success.</returns>
        public SoundIoError ConnectBackend(SoundIoBackend backend)
        {
            return NativeMethods.SoundIoConnectBackend(handle, backend);
        }

        /// <summary>
        /// Disconnects from currently connected sound interface.
        /// </summary>
        /// <remarks>Decrements ref count for all associated devices.</remarks>
        public void Disconnect()
        {
            NativeMethods.SoundIoDisconnect(handle);
        }

        /// <summary>
        /// Returns the number of available sound interfaces in the system.
        /// </summary>
        /// <returns>number of sound interfaces present in the system.</returns>
        public int BackendCount()
        {
            return NativeMethods.SoundIoBackendCount(handle);
        }

        /// <summary>
        /// Retrieves available backend at specified index.
        /// </summary>
        /// <param name="index">Index of backend to retrieve.</param>
        /// <returns>Available backend at specified index.</returns>
        public SoundIoBackend GetBackend(int index)
        {
            return NativeMethods.SoundIoGetBackend(handle, index);
        }

        /// <summary>
        /// Whether specified sound interface is available in the system.
        /// </summary>
        /// <param name="backend">Backend to validate availability for.</param>
        /// <returns>Value indicating if sound interface is present in the system.</returns>
        public static bool HaveBackend(SoundIoBackend backend)
        {
            return NativeMethods.SoundIoHaveBackend(backend);
        }

        /// <summary>
        /// Atomically update information for all connected devices. Calling this function may
        /// trigger OnDeviceChange and OnBackendDisconnect callbacks.
        /// </summary>
        public void FlushEvents()
        {
            NativeMethods.SoundIoFlushEvents(handle);
        }

        /// <summary>
        /// Calls FlushEvents then blocks until another event is ready or when Wakeup function is called manually.
        /// </summary>
        public void WaitEvents()
        {
            NativeMethods.SoundIoWaitEvents(handle);
        }

        /// <summary>
        /// Stops WaitEvents from blocking the feed.
        /// </summary>
        public void Wakeup()
        {
            NativeMethods.SoundIoWakeup(handle);
        }

        /// <summary>
        /// Updates information about sound devices. Due to libsoundio builtin auto-rescan feature,
        /// use this only when in specific circumstances e.g.: when probe fails for ALSA device.
        /// </summary>
        /// <remarks>
        /// FlushEvents or WaitEvents still needs to be called in order to trigger OnDevicesChange callback.
        /// Can be called from any thread context except for SoundIOOutStream::OnWriteCallback and
        /// SoundIOInStream::OnReadCallback.
        /// </remarks>
        public void ForceDeviceRescan()
        {
            NativeMethods.SoundIoForceDeviceScan(handle);
        }

        /// <summary>
        /// Gets the number of available active input devices.
        /// </summary>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Number of available active input devices.</returns>
        /// <remarks>Returns -1 if FlushEvents was never called.</remarks>
        public int GetInputDeviceCount(bool flush = false)
        {
            if (flush)
            {
                FlushEvents();
            }

            return NativeMethods.SoundIoInputDeviceCount(handle);
        }

        /// <summary>
        /// Gets the number of available active output devices.
        /// </summary>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Number of available active output devices.</returns>
        /// <remarks>Returns -1 if FlushEvents was never called.</remarks>
        public int GetOutputDeviceCount(bool flush = false)
        {
            if (flush)
            {
                FlushEvents();
            }

            return NativeMethods.SoundIoOutputDeviceCount(handle);
        }

        /// <summary>
        /// Gets the input device at specified position. Increments ref count for the returned device.
        /// </summary>
        /// <param name="index">Index of device to retrieve.</param>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Input sound device at specified position.</returns>
        /// <remarks>
        /// Returns null if FlushEvents was never called.
        /// Increments ref count for the returned device. Release device when no longer needed.
        /// </remarks>
        public SoundIODevice GetInputDevice(int index, bool flush = false)
        {
            if (flush)
            {
                FlushEvents();
            }

            IntPtr ptr = NativeMethods.SoundIoGetInputDevice(handle, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIODevice(ptr);
        }

        /// <summary>
        /// Gets the output device at specified position. Increments ref count for the returned device.
        /// </summary>
        /// <param name="index">Index of device to retrieve.</param>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Output sound device at specified position.</returns>
        /// <remarks>
        /// Returns null if FlushEvents was never called.
        /// Increments ref count for the returned device. Release device when no longer needed.
        /// </remarks>
        public SoundIODevice GetOutputDevice(int index, bool flush = false)
        {
            if (flush)
            {
                FlushEvents();
            }

            IntPtr ptr = NativeMethods.SoundIoGetOutputDevice(handle, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIODevice(ptr);
        }

        /// <summary>
        /// Gets the zero-based index of default input device or -1 if not found.
        /// </summary>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Zero-based index of default input device.</returns>
        /// <remarks>Returns -1 if FlushEvents was never called.</remarks>
        public int GetDefaultInputDeviceIndex(bool flush = false)
        {
            if (flush)
            {
                FlushEvents();
            }

            return NativeMethods.SoundIoDefaultInputDeviceIndex(handle);
        }

        /// <summary>
        /// Gets the zero-based index of default output device or -1 if not found.
        /// </summary>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Zero-based index of default output device.</returns>
        /// <remarks>Returns -1 if FlushEvents was never called.</remarks>
        public int GetDefaultOutputDeviceIndex(bool flush = false)
        {
            if (flush)
            {
                FlushEvents();
            }

            return NativeMethods.SoundIoDefaultOutputDeviceIndex(handle);
        }

        /// <summary>
        /// Gets the default input device. Increments ref count for the returned device.
        /// </summary>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Default input sound device.</returns>
        /// <remarks>
        /// Returns null if FlushEvents was never called.
        /// Increments ref count for the returned device. Release device when no longer needed.
        /// </remarks>
        public SoundIODevice GetDefaultInputDevice(bool flush = false)
        {
            int index = GetDefaultInputDeviceIndex(flush);
            IntPtr ptr = NativeMethods.SoundIoGetInputDevice(handle, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIODevice(ptr);
        }

        /// <summary>
        /// Gets the default output device.
        /// </summary>
        /// <param name="flush">Whether to flush before retrieval.</param>
        /// <returns>Default output sound device.</returns>
        /// <remarks>
        /// Returns null if FlushEvents was never called.
        /// Increments ref count for the returned device. Release device when no longer needed.
        /// </remarks>
        public SoundIODevice GetDefaultOutputDevice(bool flush = false)
        {
            int index = GetDefaultOutputDeviceIndex(flush);
            IntPtr ptr = NativeMethods.SoundIoGetOutputDevice(handle, index);
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIODevice(ptr);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="SoundIO"/>.
        /// </summary>
        /// <returns>Enumerator for iterating over channel areas.</returns>
        public IEnumerator<SoundIODevice> GetEnumerator()
        {
            return new DeviceEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Enumerates through the elements of <see cref="SoundIO"/>.
        /// </summary>
        public struct DeviceEnumerator : IEnumerator<SoundIODevice>
        {
            SoundIODevice[] devices;
            int memberIndex;

            public DeviceEnumerator(SoundIO io)
            {
                int inputCount, outputCount, n = 0;

                memberIndex = -1;
                inputCount = io.GetInputDeviceCount();
                outputCount = io.GetOutputDeviceCount();
                devices = new SoundIODevice[inputCount + outputCount];

                for (int i = 0; i < inputCount; i++)
                {
                    devices[n++] = io.GetInputDevice(i);
                }

                for (int i = 0; i < outputCount; i++)
                {
                    devices[n++] = io.GetOutputDevice(i);
                }
            }

            public bool MoveNext()
            {
                memberIndex++;
                return (memberIndex < devices.Length);
            }

            public void Reset()
            {
                memberIndex = -1;
            }

            public void Dispose()
            {
                foreach (var device in devices)
                {
                    if (device.RefCount > 0)
                    {
                        device.Release();
                    }
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public SoundIODevice Current
            {
                get
                {
                    try
                    {
                        return devices[memberIndex];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }

        /// <summary>
        /// Releases all resources used by <see cref="SoundIO"/> instance.
        /// </summary>
        /// <remarks>Decrements ref count for all associated devices.</remarks>
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
                    // Internaly calls unref on all associated devices
                    NativeMethods.SoundIoDestroy(handle);
                }

                Disposed = true;
            }
        }
    }
}

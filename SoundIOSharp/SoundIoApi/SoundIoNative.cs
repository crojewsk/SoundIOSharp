//
// SoundIoNative.cs
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
    /// SoundIo error codes.
    /// </summary>
    public enum SoundIoError
    {
        None,
        NoMem,                // Out of memory.
        InitAudioBackend,     // The backend does not appear to be active or running.
        SystemResources,      // A system resource other than memory was not available.
        OpeningDevice,        // Attempted to open a device and failed.
        NoSuchDevice,
        Invalid,              // The programmer did not comply with the API.
        BackendUnavailable,   // libsoundio was compiled without support for that backend.
        Streaming,            // An open stream had an error that can only be recovered from by
                              // destroying the stream and creating it again.
        IncompatibleDevice,   // Attempted to use a device with parameters it cannot support.
        NoSuchClient,         // When JACK returns `JackNoSuchClient`
        IncompatibleBackend,  // Attempted to use parameters that the backend cannot support.     
        BackendDisconnected,  // Backend server shutdown or became inactive.
        Interrupted,
        Underflow,            // Buffer underrun occurred.
        EncodingString,       // Unable to convert to or from UTF-8 to the native string format.
    };

    /// <summary>
    /// Specifies where a channel is physically located.
    /// </summary>
    public enum SoundIoChannelId
    {
        Invalid,

        FrontLeft,       // First of the more commonly supported ids.
        FrontRight,
        FrontCenter,
        Lfe,
        BackLeft,
        BackRight,
        FrontLeftCenter,
        FrontRightCenter,
        BackCenter,
        SideLeft,
        SideRight,
        TopCenter,
        TopFrontLeft,
        TopFrontCenter,
        TopFrontRight,
        TopBackLeft,
        TopBackCenter,
        TopBackRight,    // Last of the more commonly supported ids.

        BackLeftCenter,  // First of the less commonly supported ids.
        BackRightCenter,
        FrontLeftWide,
        FrontRightWide,
        FrontLeftHigh,
        FrontCenterHigh,
        FrontRightHigh,
        TopFrontLeftCenter,
        TopFrontRightCenter,
        TopSideLeft,
        TopSideRight,
        LeftLfe,
        RightLfe,
        Lfe2,
        BottomCenter,
        BottomLeftCenter,
        BottomRightCenter,

        // Mid/side recording
        MsMid,
        MsSide,

        // first order ambisonic channels
        AmbisonicW,
        AmbisonicX,
        AmbisonicY,
        AmbisonicZ,

        // X-Y Recording
        XyX,
        XyY,

        HeadphonesLeft,    // First of the "other" channel ids
        HeadphonesRight,
        ClickTrack,
        ForeignLanguage,
        HearingImpaired,
        Narration,
        Haptic,
        DialogCentricMix,  // Last of the "other" channel ids

        Aux,
        Aux0,
        Aux1,
        Aux2,
        Aux3,
        Aux4,
        Aux5,
        Aux6,
        Aux7,
        Aux8,
        Aux9,
        Aux10,
        Aux11,
        Aux12,
        Aux13,
        Aux14,
        Aux15,
    };

    /// <summary>
    /// Built-in channel layouts for convenience.
    /// </summary>
    public enum SoundIoChannelLayoutId
    {
        Mono,
        Stereo,
        Id2Point1,
        Id3Point0,
        Id3Point0Back,
        Id3Point1,
        Id4Point0,
        Quad,
        QuadSide,
        Id4Point1,
        Id5Point0Back,
        Id5Point0Side,
        IdId5Point1,
        Id5Point1Back,
        Id6Point0Side,
        Id6Point0Front,
        Hexagonal,
        Id6Point1,
        Id6Point1Back,
        Id6Point1Front,
        Id7Point0,
        Id7Point0Front,
        Id7Point1,
        Id7Point1Wide,
        Id7Point1WideBack,
        Octagonal,
    };

    /// <summary>
    /// Audio backend interfaces.
    /// </summary>
    public enum SoundIoBackend
    {
        None,
        Jack,
        PulseAudio,
        Alsa,
        CoreAudio,
        Wasapi,
        Dummy,
    };

    /// <summary>
    /// Data flow.
    /// </summary>
    public enum SoundIoDeviceAim
    {
        Input,   // capture/recording
        Output,  // playback
    };

    /// <summary>
    /// Native Endian and Foreign Endian constants pointing
    /// to the respective SoundIoFormat values.
    /// </summary>
    public enum SoundIoFormat
    {
        Invalid,
        S8,         // Signed 8 bit
        U8,         // Unsigned 8 bit
        S16LE,      // Signed 16 bit Little Endian
        S16BE,      // Signed 16 bit Big Endian
        U16LE,      // Unsigned 16 bit Little Endian
        U16BE,      // Unsigned 16 bit Little Endian
        S24LE,      // Signed 24 bit Little Endian using low three bytes in 32-bit word
        S24BE,      // Signed 24 bit Big Endian using low three bytes in 32-bit word
        U24LE,      // Unsigned 24 bit Little Endian using low three bytes in 32-bit word
        U24BE,      // Unsigned 24 bit Big Endian using low three bytes in 32-bit word
        S32LE,      // Signed 32 bit Little Endian
        S32BE,      // Signed 32 bit Big Endian
        U32LE,      // Unsigned 32 bit Little Endian
        U32BE,      // Unsigned 32 bit Big Endian
        Float32LE,  // Float 32 bit Little Endian, Range -1.0 to 1.0
        Float32BE,  // Float 32 bit Big Endian, Range -1.0 to 1.0
        Float64LE,  // Float 64 bit Little Endian, Range -1.0 to 1.0
        Float64BE,  // Float 64 bit Big Endian, Range -1.0 to 1.0
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoChannelLayout
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public int ChannelCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public SoundIoChannelId[] Channels;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoSampleRateRange
    {
        public int Min;
        public int Max;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoChannelArea
    {
        public IntPtr Ptr;
        public int Step;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIo
    {
        public IntPtr UserData;

        public delegate void DevicesChange(IntPtr soundIo);
        public delegate void BackendDisconnect(IntPtr soundIo, SoundIoError error);
        public delegate void EventsSignal(IntPtr soundIo);

        public DevicesChange OnDevicesChange;
        public BackendDisconnect OnBackendDisconnect;
        public EventsSignal OnEventsSignal;

        public SoundIoBackend CurrentBackend;
        [MarshalAs(UnmanagedType.LPStr)]
        public string AppName;

        public delegate void EmitRtPrioWarning();
        public delegate void JackInfoCallback(string msg);
        public delegate void JackErrorCallback(string msg);

        public EmitRtPrioWarning OnEmitRtPrioWarning;
        public JackInfoCallback OnJackInfoCallback;
        public JackErrorCallback OnJackErrorCallback;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoDevice
    {
        public IntPtr SoundIo;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Id;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public SoundIoDeviceAim Aim;

        public IntPtr Layouts;
        public int LayoutCount;
        public SoundIoChannelLayout CurrentLayout;

        public IntPtr Formats;
        public int FormatCount;
        public SoundIoFormat CurrentFormat;

        public IntPtr SampleRates;
        public int SamleRateCount;
        public int SampleRateCurrent;

        public double SoftwareLatencyMin;
        public double SoftwareLatencyMax;
        public double SoftwareLatencyCurrent;

        public bool IsRaw;
        public int RefCount;

        public SoundIoError ProbeError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoOutStream
    {
        public IntPtr Device;

        public SoundIoFormat Format;
        public int SampleRate;
        public SoundIoChannelLayout Layout;
        public double SoftwareLatency;
        public float Volume;
        public IntPtr UserData;

        public delegate void WriteCallback(IntPtr stream, int frameCountMin, int frameCountMax);
        public delegate void UnderflowCallback(IntPtr stream);
        public delegate void ErrorCallback(IntPtr stream, SoundIoError err);

        public WriteCallback OnWriteCallback;
        public UnderflowCallback OnUnderflowCallback;
        public ErrorCallback OnErrorCallback;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public bool NonTerminalHint;
        public int BytesPerFrame;
        public int BytesPerSample;
        public SoundIoError LayoutError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoInStream
    {
        public SoundIODevice Device;

        public SoundIoFormat Format;
        public int SampleRate;
        public SoundIoChannelLayout Layout;
        public double SoftwareLatency;
        public IntPtr UserData;

        public delegate void ReadCallback(IntPtr stream, int frameCountMin, int frameCountMax);
        public delegate void OverflowCallback(IntPtr stream);
        public delegate void ErrorCallback(IntPtr stream, SoundIoError err);

        public ReadCallback OnReadCallback;
        public OverflowCallback OnOverflowCallback;
        public ErrorCallback OnErrorCallback;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        public bool NonTerminalHint;
        public int BytesPerFrame;
        public int BytesPerSample;
        public SoundIoError LayoutError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundIoRingBuffer
    {
    }

    /// <summary>
    /// Exposes methods native to libsoundio library.
    /// </summary>
    internal static class NativeMethods
    {
        internal const string LibraryName = "soundio";

        /**
         * General
         */

        [DllImport(LibraryName, EntryPoint = "soundio_version_string")]
        internal static extern IntPtr SoundIoVersionString();

        [DllImport(LibraryName, EntryPoint = "soundio_version_major")]
        internal static extern int SoundIoVersionMajor();

        [DllImport(LibraryName, EntryPoint = "soundio_version_minor")]
        internal static extern int SoundIoVersionMinor();

        [DllImport(LibraryName, EntryPoint = "soundio_version_patch")]
        internal static extern int SoundIoVersionPatch();

        [DllImport(LibraryName, EntryPoint = "soundio_create")]
        internal static extern IntPtr SoundIoCreate();

        [DllImport(LibraryName, EntryPoint = "soundio_destroy")]
        internal static extern void SoundIoDestroy(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_connect")]
        internal static extern SoundIoError SoundIoConnect(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_connect_backend")]
        internal static extern SoundIoError SoundIoConnectBackend(
            [In] IntPtr         soundIo,
            [In] SoundIoBackend backend
        );

        [DllImport(LibraryName, EntryPoint = "soundio_disconnect")]
        internal static extern void SoundIoDisconnect(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_strerror")]
        internal static extern IntPtr SoundIoStrError(
            [In] SoundIoError error
        );

        [DllImport(LibraryName, EntryPoint = "soundio_backend_name")]
        internal static extern IntPtr SoundIoBackendName(
            [In] SoundIoBackend backend
        );

        [DllImport(LibraryName, EntryPoint = "soundio_backend_count")]
        internal static extern int SoundIoBackendCount(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_get_backend")]
        internal static extern SoundIoBackend SoundIoGetBackend(
            [In] IntPtr soundIo,
            [In] int    index
        );

        [DllImport(LibraryName, EntryPoint = "soundio_have_backend")]
        internal static extern bool SoundIoHaveBackend(
            [In] SoundIoBackend backend
        );

        [DllImport(LibraryName, EntryPoint = "soundio_flush_events")]
        internal static extern int SoundIoFlushEvents(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_wait_events")]
        internal static extern int SoundIoWaitEvents(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_wakeup")]
        internal static extern int SoundIoWakeup(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_force_device_scan")]
        internal static extern int SoundIoForceDeviceScan(
            [In] IntPtr soundIo
        );

        /**
         * Channel Layouts
         */

        [DllImport(LibraryName, EntryPoint = "soundio_channel_layout_equal")]
        internal static extern bool SoundIoChannelLayoutEqual(
            [In, Out] IntPtr a,
            [In, Out] IntPtr b
        );

        [DllImport(LibraryName, EntryPoint = "soundio_get_channel_name")]
        internal static extern IntPtr SoundIoGetChannelName(
            [In] SoundIoChannelId id
        );

        [DllImport(LibraryName, EntryPoint = "soundio_parse_channel_id")]
        internal static extern SoundIoChannelId SoundIoParseChannelId(
            [MarshalAs(UnmanagedType.LPStr)]
            [In] string str,
            [In] int    strlen
        );

        [DllImport(LibraryName, EntryPoint = "soundio_channel_layout_builtin_count")]
        internal static extern int SoundIoChannelLayoutBuiltinCount();

        [DllImport(LibraryName, EntryPoint = "soundio_channel_layout_get_builtin")]
        internal static extern IntPtr SoundIoChannelLayoutGetBuiltin(
            [In] int index
        );

        [DllImport(LibraryName, EntryPoint = "soundio_channel_layout_get_default")]
        internal static extern IntPtr SoundIoChannelLayoutGetDefault(
            [In] int channelCount
        );

        [DllImport(LibraryName, EntryPoint = "soundio_channel_layout_find_channel")]
        internal static extern int SoundIoChannelLayoutFindChannel(
            [In] IntPtr           layout,
            [In] SoundIoChannelId channel
        );

        [DllImport(LibraryName, EntryPoint = "soundio_channel_layout_detect_builtin")]
        internal static extern bool SoundIoChannelLayoutDetectBuiltin(
            [In] IntPtr layout
        );

        [DllImport(LibraryName, EntryPoint = "soundio_best_matching_channel_layout")]
        internal static extern IntPtr SoundIoBestMatchingChannelLayout(
            [In] IntPtr preferredLayouts,
            [In] int    preferredLayoutCount,
            [In] IntPtr availableLayouts,
            [In] int    availableLayoutsCount);

        [DllImport(LibraryName, EntryPoint = "soundio_sort_channel_layouts")]
        internal static extern void SoundIoSortChannelLayouts(
            [In, Out] IntPtr layouts,
            [In]      int    layoutCount);

        /**
         * Sample Formats
         */

        [DllImport(LibraryName, EntryPoint = "soundio_get_bytes_per_sample")]
        internal static extern int SoundIoGetBytesPerSample(
            [In] SoundIoFormat format
        );

        [DllImport(LibraryName, EntryPoint = "soundio_format_string")]
        internal static extern IntPtr SoundIoFormatString(
            [In] SoundIoFormat format
        );

        /**
         * Devices
         */

        [DllImport(LibraryName, EntryPoint = "soundio_input_device_count")]
        internal static extern int SoundIoInputDeviceCount(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_output_device_count")]
        internal static extern int SoundIoOutputDeviceCount(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_get_input_device")]
        internal static extern IntPtr SoundIoGetInputDevice(
            [In] IntPtr soundIo,
            [In] int    index
        );

        [DllImport(LibraryName, EntryPoint = "soundio_get_output_device")]
        internal static extern IntPtr SoundIoGetOutputDevice(
            [In] IntPtr soundIo,
            [In] int    index
        );

        [DllImport(LibraryName, EntryPoint = "soundio_default_input_device_index")]
        internal static extern int SoundIoDefaultInputDeviceIndex(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_default_output_device_index")]
        internal static extern int SoundIoDefaultOutputDeviceIndex(
            [In] IntPtr soundIo
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_ref")]
        internal static extern void SoundIoDeviceRef(
            [In] IntPtr device
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_unref")]
        internal static extern void SoundIoDeviceUnref(
            [In] IntPtr device
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_equal")]
        internal static extern bool SoundIoDeviceEqual(
            [In] IntPtr a,
            [In] IntPtr b
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_sort_channel_layouts")]
        internal static extern void SoundIoDeviceSortChannelLayouts(
            [In] IntPtr device
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_supports_format")]
        internal static extern bool SoundIoDeviceSupportsFormat(
            [In] IntPtr        device,
            [In] SoundIoFormat format
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_supports_layout")]
        internal static extern bool SoundIoDeviceSupportsLayout(
            [In] IntPtr device,
            [In] IntPtr layout
        );

        [DllImport(LibraryName, EntryPoint = "soundio_device_supports_sample_rate")]
        internal static extern bool SoundIoDeviceSupportsSampleRate(
            [In] IntPtr device,
            [In] int    sampleRate
        );

       [DllImport(LibraryName, EntryPoint = "soundio_device_nearest_sample_rate")]
        internal static extern int SoundIoDeviceNearestSampleRate(
           [In] IntPtr device,
           [In] int    sampleRate
        );

        /**
         * Output Streams
         */

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_create")]
        internal static extern IntPtr SoundIoOutstreamCreate(
            [In] IntPtr device
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_destroy")]
        internal static extern void SoundIoOutstreamDestroy(
            [In] IntPtr outstream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_open")]
        internal static extern SoundIoError SoundIoOutstreamOpen(
            [In] IntPtr outstream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_start")]
        internal static extern SoundIoError SoundIoOutstreamStart(
            [In] IntPtr outstream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_begin_write")]
        internal static extern SoundIoError SoundIoOutstreamBeginWrite(
            [In]          IntPtr outstream,
            [Out]     out IntPtr areas,
            [In, Out] ref int    frameCount
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_end_write")]
        internal static extern SoundIoError SoundIoOutstreamEndWrite(
            [In] IntPtr outstream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_clear_buffer")]
        internal static extern SoundIoError SoundIoOutstreamClearBuffer(
            [In] IntPtr outstream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_pause")]
        internal static extern SoundIoError SoundIoOutstreamPause(
            [In] IntPtr outstream,
            [In] bool   pause
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_get_latency")]
        internal static extern SoundIoError SoundIoOutstreamGetLatency(
            [In]      IntPtr outstream,
            [Out] out double outLatency
        );

        [DllImport(LibraryName, EntryPoint = "soundio_outstream_set_volume")]
        internal static extern SoundIoError SoundIoOutstreamSetVolume(
            [In] IntPtr outstream,
            [In] double volume
        );

        /**
         * Input Streams
         */

        [DllImport(LibraryName, EntryPoint = "soundio_instream_create")]
        internal static extern IntPtr SoundIoInstreamCreate(
            [In] IntPtr device
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_destroy")]
        internal static extern void SoundIoInstreamDestroy(
            [In] IntPtr instream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_open")]
        internal static extern SoundIoError SoundIoInstreamOpen(
            [In] IntPtr instream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_start")]
        internal static extern SoundIoError SoundIoInstreamStart(
            [In] IntPtr instream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_begin_read")]
        internal static extern SoundIoError SoundIoInstreamBeginRead(
            [In]          IntPtr instream,
            [Out]     out IntPtr areas,
            [In, Out] ref int    frameCount
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_end_read")]
        internal static extern SoundIoError SoundIoInstreamEndRead(
            [In] IntPtr instream
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_pause")]
        internal static extern SoundIoError SoundIoInstreamPause(
            [In] IntPtr instream,
            [In] bool   pause
        );

        [DllImport(LibraryName, EntryPoint = "soundio_instream_get_latency")]
        internal static extern SoundIoError SoundIoInstreamGetLatency(
            [In]      IntPtr instream,
            [Out] out double outLatency
        );

        /**
         * Ring buffer
         */

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_create")]
        internal static extern IntPtr SoundIoRingBufferCreate(
            [In] IntPtr soundIo,
            [In] int    requestedCapacity
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_destroy")]
        internal static extern void SoundIoRingBufferDestroy(
            [In] IntPtr ringBuffer
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_capacity")]
        internal static extern int SoundIoRingBufferCapacity(
            [In] IntPtr ringBuffer
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_write_ptr")]
        internal static extern IntPtr SoundIoRingBufferWritePtr(
            [In] IntPtr ringBuffer
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_advance_write_ptr")]
        internal static extern void SoundIoRingBufferAdvanceWritePtr(
            [In] IntPtr ringBuffer,
            [In] int    count
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_read_ptr")]
        internal static extern IntPtr SoundIoRingBufferReadPtr(
            [In] IntPtr ringBuffer
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_advance_read_ptr")]
        internal static extern void SoundIoRingBufferAdvanceReadPtr(
            [In] IntPtr ringBuffer,
            [In] int    count
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_fill_count")]
        internal static extern int SoundIoRingBufferFillCount(
            [In] IntPtr ringBuffer
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_free_count")]
        internal static extern int SoundIoRingBufferFreeCount(
            [In] IntPtr ringBuffer
        );

        [DllImport(LibraryName, EntryPoint = "soundio_ring_buffer_clear")]
        internal static extern void SoundIoRingBufferClear(
            [In] IntPtr ringBuffer
        );
    }
}

//
// SoundIODevice.cs
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
    /// Media device, able to record and/or render data.
    /// </summary>
    public class SoundIODevice : IEquatable<SoundIODevice>
    {
        private IntPtr handle;
        private SoundIOChannelLayout[] layouts;
        private SoundIoFormat[] formats;
        private SoundIoSampleRateRange[] sampleRates;

        internal SoundIODevice(IntPtr device)
        {
            IntPtr ptr;
            int size;

            handle = device;

            Id = MarshalHelper.ReadStringAnsiField<SoundIoDevice>(handle, "Id");
            Name = MarshalHelper.ReadStringAnsiField<SoundIoDevice>(handle, "Name");
            Aim = (SoundIoDeviceAim)MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "Aim");

            LayoutCount = MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "LayoutCount");
            CurrentLayout = new SoundIOChannelLayout(handle + MarshalHelper.OffsetOf<SoundIoDevice>("CurrentLayout"));

            layouts = new SoundIOChannelLayout[LayoutCount];
            ptr = MarshalHelper.ReadIntPtrField<SoundIoDevice>(handle, "Layouts");
            size = Marshal.SizeOf(typeof(SoundIoChannelLayout));
            for (int n = 0; n < LayoutCount; n++)
            {
                layouts[n] = new SoundIOChannelLayout(ptr);
                ptr += size;
            }

            FormatCount = MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "FormatCount");
            CurrentFormat = (SoundIoFormat)MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "CurrentFormat");

            formats = new SoundIoFormat[FormatCount];
            ptr = MarshalHelper.ReadIntPtrField<SoundIoDevice>(handle, "Formats");
            size = sizeof(int);
            for (int n = 0; n < FormatCount; n++)
            {
                formats[n] = (SoundIoFormat)Marshal.ReadInt32(ptr);
                ptr += size;
            }

            SamleRateCount = MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "SamleRateCount");
            CurrentSampleRate = MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "SampleRateCurrent");

            sampleRates = new SoundIoSampleRateRange[SamleRateCount];
            ptr = MarshalHelper.ReadIntPtrField<SoundIoDevice>(handle, "SampleRates");
            size = Marshal.SizeOf(typeof(SoundIoSampleRateRange));
            for (int n = 0; n < SamleRateCount; n++)
            {
                sampleRates[n] = (SoundIoSampleRateRange)Marshal.PtrToStructure(ptr, typeof(SoundIoSampleRateRange));
                ptr += size;
            }

            IsRaw = MarshalHelper.ReadByteField<SoundIoDevice>(handle, "IsRaw") != 0;
            ProbeError = (SoundIoError)MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "ProbeError");
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
        /// String identifier of device.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Name of device.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Data flow direction.
        /// </summary>
        public readonly SoundIoDeviceAim Aim;

        /// <summary>
        /// Sequence of supported channel layouts.
        /// </summary>
        public SoundIOChannelLayout[] Layouts
        {
            get
            {
                return layouts;
            }
        }

        /// <summary>
        /// Amount of supported channel layouts.
        /// </summary>
        public readonly int LayoutCount;

        /// <summary>
        /// Currently selected <see cref="SoundIOChannelLayout"/> for the device.
        /// </summary>
        public readonly SoundIOChannelLayout CurrentLayout;

        /// <summary>
        /// Sequence of supported formats.
        /// </summary>
        public SoundIoFormat[] Formats
        {
            get
            {
                return formats;
            }
        }

        /// <summary>
        /// Amount of supported formats.
        /// </summary>
        public readonly int FormatCount;

        /// <summary>
        /// Currently selected <see cref="SoundIoFormat"/> for the device.
        /// </summary>
        public readonly SoundIoFormat CurrentFormat;

        /// <summary>
        /// Sequence of supported sample rates.
        /// </summary>
        public SoundIoSampleRateRange[] SampleRates
        {
            get
            {
                return sampleRates;
            }
        }

        /// <summary>
        /// Amount of supported sample rates.
        /// </summary>
        public readonly int SamleRateCount;

        /// <summary>
        /// Currently selected sample rate for the device.
        /// </summary>
        public readonly int CurrentSampleRate;

        /// <summary>
        /// Software latency minimum in seconds. If this value is unknown or
        /// irrelevant, it is set to 0.0.
        /// </summary>
        /// <remarks>For PulseAudio and WASAPI this value is unknown until you open a stream.</remarks>
        public double SoftwareLatencyMin
        {
            get
            {
                return BitConverter.Int64BitsToDouble(MarshalHelper.ReadInt64Field<SoundIoDevice>(handle, "SoftwareLatencyMin"));
            }
        }

        /// <summary>
        /// Software latency maximum in seconds. If this value is unknown or
        /// irrelevant, it is set to 0.0.
        /// </summary>
        /// <remarks>For PulseAudio and WASAPI this value is unknown until you open a stream.</remarks>
        public double SoftwareLatencyMax
        {
            get
            {
                return BitConverter.Int64BitsToDouble(MarshalHelper.ReadInt64Field<SoundIoDevice>(handle, "SoftwareLatencyMax"));
            }
        }

        /// <summary>
        /// Software latency in seconds. If this value is unknown or
        /// irrelevant, it is set to 0.0.
        /// </summary>
        /// <remarks>For PulseAudio and WASAPI this value is unknown until you open a stream.</remarks>
        public double SoftwareLatencyCurrent
        {
            get
            {
                return BitConverter.Int64BitsToDouble(MarshalHelper.ReadInt64Field<SoundIoDevice>(handle, "SoftwareLatencyCurrent"));
            }
        }

        /// <summary>
        /// Indicates if you are directly opening the hardware device and not
        /// going through a proxy such as dmix, PulseAudio, or JACK.
        /// </summary>
        /// <remarks>
        /// When you open a raw device, other applications on the computer are not able to
        /// simultaneously access the device. Raw devices do not perform automatic
        /// resampling and thus tend to have fewer formats available.
        /// </remarks>
        public readonly bool IsRaw;

        /// <summary>
        /// Current reference count for the <see cref="SoundIODevice"/>.
        /// </summary>
        public int RefCount
        {
            get
            {
                return MarshalHelper.ReadInt32Field<SoundIoDevice>(handle, "RefCount");
            }
        }

        /// <summary>
        /// Result of the device probe. If set to <see cref="SoundIoError.None"/>,
        /// all the fields of the device will be populated. Otherwise, information
        /// about formats, sample rates, and channel layouts might be missing.
        /// </summary>
        public readonly SoundIoError ProbeError;

        /// <summary>
        /// Iterates over preferred layouts and returns the first channel layout which matches
        /// one of the channel layouts in available layouts or null if none matches.
        /// </summary>
        /// <param name="devicePreferredLayouts">Preferred layouts to iterate over.</param>
        /// <param name="deviceAvailableLayouts">Range of layouts to match preferred layouts with.</param>
        /// <returns>First <see cref="SoundIOChannelLayout"/> matching criteria or null if none matches.</returns>
        public static SoundIOChannelLayout BestMatchingChannelLayout(SoundIODevice devicePreferredLayouts, SoundIODevice deviceAvailableLayouts)
        {
            if (devicePreferredLayouts == null || deviceAvailableLayouts == null)
            {
                throw new ArgumentNullException("devicePreferredLayouts and/or deviceAvailableLayouts");
            }

            IntPtr preferredLayouts = MarshalHelper.ReadIntPtrField<SoundIoDevice>(devicePreferredLayouts.handle, "Layouts");
            IntPtr availableLayouts = MarshalHelper.ReadIntPtrField<SoundIoDevice>(deviceAvailableLayouts.handle, "Layouts");

            IntPtr ptr = NativeMethods.SoundIoBestMatchingChannelLayout(
                preferredLayouts,
                devicePreferredLayouts.LayoutCount,
                availableLayouts,
                deviceAvailableLayouts.LayoutCount);

            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            return new SoundIOChannelLayout(ptr);
        }

        /// <summary>
        /// Increments the reference count for a <see cref="SoundIODevice"/> object.
        /// </summary>
        public void AddRef()
        {
            NativeMethods.SoundIoDeviceRef(handle);
        }

        /// <summary>
        /// Decrements the reference count for a <see cref="SoundIODevice"/> object.
        /// </summary>
        public void Release()
        {
            NativeMethods.SoundIoDeviceUnref(handle);
        }

        /// <summary>
        /// Whether this device equals another, specified <see cref="SoundIODevice"/> instance.
        /// </summary>
        /// <param name="other"><see cref="SoundIODevice"/> instance to compare to.</param>
        /// <returns>Value indicating whether devices are equal.</returns>
        public bool Equals(SoundIODevice other)
        {
            return NativeMethods.SoundIoDeviceEqual(handle, other.handle);
        }

        /// <summary>
        /// Sorts channel layouts by channel count in descending order.
        /// </summary>
        public void SortChannelLayouts()
        {
            NativeMethods.SoundIoDeviceSortChannelLayouts(handle);
        }

        /// <summary>
        /// Whether specified <see cref="SoundIoFormat"/> is supported by the device.
        /// </summary>
        /// <param name="format">Format to validate support for.</param>
        /// <returns>Value indicating whether format is supported.</returns>
        public bool SupportsFormat(SoundIoFormat format)
        {
            return NativeMethods.SoundIoDeviceSupportsFormat(handle, format);
        }

        /// <summary>
        /// Whether specified <see cref="SoundIOChannelLayout"/> is supported by the device.
        /// </summary>
        /// <param name="layout">Channel layout to validate support for.</param>
        /// <returns>Value indicating whether channel layout is supported.</returns>
        public bool SupportsLayout(SoundIOChannelLayout layout)
        {
            return NativeMethods.SoundIoDeviceSupportsLayout(handle, layout.Handle);
        }

        /// <summary>
        /// Whether specified sample rate is supported by the device.
        /// </summary>
        /// <param name="sampleRate">Sample rate to validate support for.</param>
        /// <returns>Value indicating whether sample rate is supported.</returns>
        public bool SupportsSampleRate(int sampleRate)
        {
            return NativeMethods.SoundIoDeviceSupportsSampleRate(handle, sampleRate);
        }

        /// <summary>
        /// Returns nearest available sample rate for the device, rounding up.
        /// </summary>
        /// <param name="sampleRate">Sample rate to validate availability for.</param>
        /// <returns>Nearest available sample rate, rounding up.</returns>
        public int NearestSampleRate(int sampleRate)
        {
            return NativeMethods.SoundIoDeviceNearestSampleRate(handle, sampleRate);
        }
    }
}

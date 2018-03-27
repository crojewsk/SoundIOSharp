//
// ExtensionMethods.cs
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

using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    /// <summary>
    /// Set of extension methods targetting SoundIo enumeration types.
    /// </summary>
    public static class SoundIoExtensionMethods
    {
        /// <summary>
        /// Retrieves string message for specified error.
        /// </summary>
        /// <param name="error">Error for which to retrieve message.</param>
        /// <returns>String message for specified error.</returns>
        public static string GetErrorMessage(this SoundIoError error)
        {
            return Marshal.PtrToStringAnsi(NativeMethods.SoundIoStrError(error));
        }

        /// <summary>
        /// Retrieves name string for specified backend.
        /// </summary>
        /// <param name="backend">Backend for which to retrieve value.</param>
        /// <returns>Name of specified backend.</returns>
        public static string GetBackendName(this SoundIoBackend backend)
        {
            return Marshal.PtrToStringAnsi(NativeMethods.SoundIoBackendName(backend));
        }

        /// <summary>
        /// Retrieves name string for specified channel id.
        /// </summary>
        /// <param name="id">Channel id for which to retrieve value.</param>
        /// <returns>Name of specified channel id.</returns>
        public static string GetChannelName(this SoundIoChannelId id)
        {
            return Marshal.PtrToStringAnsi(NativeMethods.SoundIoGetChannelName(id));
        }

        /// <summary>
        /// Retrieves number of bytes required per sample for specified format.
        /// </summary>
        /// <param name="format">Format for which to retrieve value.</param>
        /// <returns>Number of bytes per sample for specified format.</returns>
        public static int GetBytesPerSample(this SoundIoFormat format)
        {
            return NativeMethods.SoundIoGetBytesPerSample(format);
        }

        /// <summary>
        /// Retrieves number of bytes required per frame for specified format and channel count.
        /// </summary>
        /// <param name="format">Format for which to retrieve value.</param>
        /// <param name="channelCount">Number of channels.</param>
        /// <returns>Number of bytes per sample for specified format and channel count.</returns>
        public static int GetBytesPerFrame(this SoundIoFormat format, int channelCount)
        {
            return GetBytesPerSample(format) * channelCount;
        }

        /// <summary>
        /// Retrieves number of bytes required per second for specified format, channel count and sample rate.
        /// </summary>
        /// <param name="format">Format for which to retrieve value.</param>
        /// <param name="channelCount">Number of channels.</param>
        /// <param name="sampleRate">Sample rate.</param>
        /// <returns>Number of bytes per sample for specified format, channel count and sample rate.</returns>
        public static int GetBytesPerSecond(this SoundIoFormat format, int channelCount, int sampleRate)
        {
            return GetBytesPerFrame(format, channelCount) * sampleRate;
        }

        /// <summary>
        /// Retrieves format string for specified sound format.
        /// </summary>
        /// <param name="format">Format for which to retrieve value.</param>
        /// <returns>Format string for specified sound format.</returns>
        public static string GetFormatString(this SoundIoFormat format)
        {
            return Marshal.PtrToStringAnsi(NativeMethods.SoundIoFormatString(format));
        }
    }
}

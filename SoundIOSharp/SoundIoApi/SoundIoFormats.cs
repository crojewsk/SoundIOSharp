//
// SoundIoFormats.cs
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
    /// Collection of convenient <see cref="SoundIoFormat"/> enumeration constants.
    /// </summary>
    public static class SoundIoFormats
    {
        public static readonly SoundIoFormat S16NE;
        public static readonly SoundIoFormat U16NE;
        public static readonly SoundIoFormat S24NE;
        public static readonly SoundIoFormat U24NE;
        public static readonly SoundIoFormat S32NE;
        public static readonly SoundIoFormat U32NE;
        public static readonly SoundIoFormat Float32NE;
        public static readonly SoundIoFormat Float64NE;

        public static readonly SoundIoFormat S16FE;
        public static readonly SoundIoFormat U16FE;
        public static readonly SoundIoFormat S24FE;
        public static readonly SoundIoFormat U24FE;
        public static readonly SoundIoFormat S32FE;
        public static readonly SoundIoFormat U32FE;
        public static readonly SoundIoFormat Float32FE;
        public static readonly SoundIoFormat Float64FE;

        static SoundIoFormats()
        {
            if (BitConverter.IsLittleEndian)
            {
                S16NE = SoundIoFormat.S16LE;
                U16NE = SoundIoFormat.U16LE;
                S24NE = SoundIoFormat.S24LE;
                U24NE = SoundIoFormat.U24LE;
                S32NE = SoundIoFormat.S32LE;
                U32NE = SoundIoFormat.U32LE;
                Float32NE = SoundIoFormat.Float32LE;
                Float64NE = SoundIoFormat.Float64LE;

                S16FE = SoundIoFormat.S16BE;
                U16FE = SoundIoFormat.U16BE;
                S24FE = SoundIoFormat.S24BE;
                U24FE = SoundIoFormat.U24BE;
                S32FE = SoundIoFormat.S32BE;
                U32FE = SoundIoFormat.U32BE;
                Float32FE = SoundIoFormat.Float32BE;
                Float64FE = SoundIoFormat.Float64BE;
            }
            else
            {
                S16NE = SoundIoFormat.S16BE;
                U16NE = SoundIoFormat.U16BE;
                S24NE = SoundIoFormat.S24BE;
                U24NE = SoundIoFormat.U24BE;
                S32NE = SoundIoFormat.S32BE;
                U32NE = SoundIoFormat.U32BE;
                Float32NE = SoundIoFormat.Float32BE;
                Float64NE = SoundIoFormat.Float64BE;

                S16FE = SoundIoFormat.S16LE;
                U16FE = SoundIoFormat.U16LE;
                S24FE = SoundIoFormat.S24LE;
                U24FE = SoundIoFormat.U24LE;
                S32FE = SoundIoFormat.S32LE;
                U32FE = SoundIoFormat.U32LE;
                Float32FE = SoundIoFormat.Float32LE;
                Float64FE = SoundIoFormat.Float64LE;
            }
        }
    }
}

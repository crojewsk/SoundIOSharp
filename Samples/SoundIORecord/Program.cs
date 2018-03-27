//
// SoundIORecord sample
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

using SoundIOSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace SoundIORecord
{
    class Program
    {
        static Dictionary<IntPtr, SoundIORingBuffer> ringBuffers = new Dictionary<IntPtr, SoundIORingBuffer>();

        static SoundIoFormat[] prioritizedFormats =
        {
            SoundIoFormats.Float32NE,
            SoundIoFormats.Float32FE,
            SoundIoFormats.S32NE,
            SoundIoFormats.S32FE,
            SoundIoFormats.S24NE,
            SoundIoFormats.S24FE,
            SoundIoFormats.S16NE,
            SoundIoFormats.S16FE,
            SoundIoFormats.Float64NE,
            SoundIoFormats.Float64FE,
            SoundIoFormats.U32NE,
            SoundIoFormats.U32FE,
            SoundIoFormats.U24NE,
            SoundIoFormats.U24FE,
            SoundIoFormats.U16NE,
            SoundIoFormats.U16FE,
            SoundIoFormat.S8,
            SoundIoFormat.U8,
            SoundIoFormat.Invalid,
        };

        static readonly int[] prioritizedSampleRates =
        {
            48000,
            44100,
            96000,
            24000,
            0,
        };

        static void ReadCallback(SoundIOInStream instream, int frameCountMin, int frameCountMax)
        {
            SoundIORingBuffer ringBuffer = ringBuffers[instream.UserData];
            SoundIOChannelAreas areas;
            SoundIoError err;

            IntPtr writePtr = ringBuffer.WritePointer;
            int freeBytes = ringBuffer.FreeCount;
            int freeCount = freeBytes / instream.BytesPerFrame;

            if (frameCountMin > freeCount)
                throw new SoundIOException("Ring buffer overflow");

            int writeFrames = Math.Min(freeCount, frameCountMax);
            int framesLeft = writeFrames;

            while (true)
            {
                int frameCount = framesLeft;

                err = instream.BeginRead(out areas, ref frameCount);
                if (err != SoundIoError.None)
                    throw new SoundIOException(string.Format("Begin read error: {0}.", err.GetErrorMessage()));

                if (frameCount == 0)
                    break;

                if (areas == null)
                {
                    // Due to an overflow there is a hole. Fill the ring buffer with
                    // silence for the size of the hole.
                    int count = frameCount * instream.BytesPerFrame;
                    for (int i = 0; i < count; i++)
                        Marshal.WriteByte(writePtr + i, 0);
                }
                else
                {
                    int channelCount = instream.Layout.ChannelCount;
                    int bytesPerSample = instream.BytesPerSample;
                    for (int frame = 0; frame < frameCount; frame++)
                    {
                        for (int ch = 0; ch < channelCount; ch++)
                        {
                            unsafe
                            {
                                Buffer.MemoryCopy((void*)areas[ch].Pointer, (void*)writePtr, bytesPerSample, bytesPerSample);
                            }
                            areas[ch].AdvancePointer();
                            writePtr += bytesPerSample;
                        }
                    }
                }

                err = instream.EndRead();
                if (err != SoundIoError.None)
                    throw new SoundIOException(string.Format("End read error: {0}.", err.GetErrorMessage()));

                framesLeft -= frameCount;
                if (framesLeft <= 0)
                    break;
            }

            int advanceBytes = writeFrames * instream.BytesPerFrame;
            ringBuffer.AdvanceWritePointer(advanceBytes);
        }

        static int overflowCount = 0;

        static void OverflowCallback(SoundIOInStream instream)
        {
            Console.Error.WriteLine("Overflow {0}.", overflowCount++);
        }

        static int Usage(string exe)
        {
            Console.Error.WriteLine("Usage: {0} [options]\n" +
                "Options:\n" +
                "\t[--backend dummy|alsa|pulseaudio|jack|coreaudio|wasapi]\n" +
                "\t[--device id]\n" +
                "\t[--raw]\n",
                exe);
            return 1;
        }

        public static int Main(string[] args)
        {
            string exe = "SoundIORecord";
            SoundIoBackend backend = SoundIoBackend.None;
            string deviceId = null;
            bool isRaw = false;
            string outfile = null;
            SoundIoError err;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--"))
                {
                    if (arg.CompareTo("--raw") == 0)
                        isRaw = true;
                    else if (++i > args.Length)
                        return Usage(exe);
                    else if (arg.CompareTo("--backend") == 0)
                        backend = (SoundIoBackend)Enum.Parse(typeof(SoundIoBackend), args[i]);
                    else if (arg.CompareTo("--device") == 0)
                        deviceId = args[i];
                    else
                        return Usage(exe);
                }
                else if (outfile == null)
                    outfile = arg;
                else
                    return Usage(exe);
            }

            if (outfile == null)
                return Usage(exe);

            SoundIO soundIo = new SoundIO();

            err = (backend == SoundIoBackend.None) ?
                soundIo.Connect() : soundIo.ConnectBackend(backend);
            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("Error connecting: {0}.", err.GetErrorMessage());
                return 1;
            }

            soundIo.FlushEvents();

            SoundIODevice selectedDevice = null;
            if (deviceId != null)
            {
                foreach (var dev in soundIo)
                {
                    if (dev.Aim == SoundIoDeviceAim.Input && dev.Id.Equals(deviceId) && dev.IsRaw == isRaw)
                    {
                        selectedDevice = dev;
                        break;
                    }
                }

                if (selectedDevice == null)
                {
                    Console.Error.WriteLine("Invalid device id: {0}.", deviceId);
                    return 1;
                }

                selectedDevice.AddRef(); // Enumerator cleans up itself on dispose
            }
            else
            {
                selectedDevice = soundIo.GetDefaultInputDevice();
                if (selectedDevice == null)
                {
                    Console.Error.WriteLine("No input devices available.");
                    return 1;
                }
            }

            Console.WriteLine("Device: {0}.", selectedDevice.Name);

            if (selectedDevice.ProbeError != 0)
            {
                Console.Error.WriteLine("Unable to probe device: {0}.", selectedDevice.ProbeError.GetErrorMessage());
                return 1;
            }

            selectedDevice.SortChannelLayouts();

            int sampleRate = prioritizedSampleRates.FirstOrDefault(sr => selectedDevice.SupportsSampleRate(sr));
            if (sampleRate == 0)
                sampleRate = selectedDevice.SampleRates[0].Max;

            SoundIoFormat fmt = prioritizedFormats.FirstOrDefault(f => selectedDevice.SupportsFormat(f));
            if (fmt == SoundIoFormat.Invalid)
                fmt = selectedDevice.Formats[0];

            var instream = new SoundIOInStream(selectedDevice);
            instream.Format = fmt;
            instream.SampleRate = sampleRate;
            instream.OnReadCallback = ReadCallback;
            instream.OnOverflowCallback = OverflowCallback;

            err = instream.Open();
            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("Unable to open input stream: {0}.", err.GetErrorMessage());
                return 1;
            }

            Console.WriteLine("{0} {1}Hz {2} interleaved",
                instream.Layout.Name, sampleRate, fmt.GetFormatString());

            const int ringBufferDurationSeconds = 30;
            int capacity = ringBufferDurationSeconds * instream.SampleRate * instream.BytesPerFrame;
            SoundIORingBuffer ringBuffer = new SoundIORingBuffer(soundIo, capacity);
            instream.UserData = ringBuffer.Handle;
            ringBuffers.Add(ringBuffer.Handle, ringBuffer);

            err = instream.Start();
            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("Unable to start input device: {0}.", err.GetErrorMessage());
                return 1;
            }

            Console.WriteLine("Recording data for 10 seconds.");
            int timeout = 10;
            using (var fs = File.OpenWrite(outfile))
            {
                byte[] buffer = new byte[capacity];
                while (true) // No memory allocations allowed
                {
                    soundIo.FlushEvents();
                    Thread.Sleep(1000);
                    int fillBytes = ringBuffer.FillCount;
                    IntPtr readBuf = ringBuffer.ReadPointer;
                    Marshal.Copy(readBuf, buffer, 0, fillBytes);
                    fs.Write(buffer, 0, fillBytes);

                    ringBuffer.AdvanceReadPointer(fillBytes);

                    if (--timeout <= 0)
                        break;
                }
            }

            instream.Dispose();
            selectedDevice.Release();
            soundIo.Dispose();
            return 0;
        }
    }
}

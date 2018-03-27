//
// SoundIOSine sample
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

namespace SoundIOSine
{
    class Program
    {
        static int Usage(string exe)
        {
            Console.Error.WriteLine("Usage: {0} [options]\n" +
                "Options:\n" +
                "\t[--backend dummy|alsa|pulseaudio|jack|coreaudio|wasapi]\n" +
                "\t[--device id]\n" +
                "\t[--raw]\n" +
                "\t[--name stream_name]\n" +
                "\t[--latency seconds]\n" +
                "\t[--sample-rate hz]",
                exe);
            return 1;
        }

        static unsafe void WriteSampleS16NE(IntPtr ptr, double sample)
        {
            short* buffer = (short*)ptr;
            double range = (double)short.MaxValue - (double)short.MinValue;
            double val = sample * range / 2.0;
            *buffer = (short)val;
        }

        static unsafe void WriteSampleS32NE(IntPtr ptr, double sample)
        {
            int* buffer = (int*)ptr;
            double range = (double)int.MaxValue - (double)int.MinValue;
            double val = sample * range / 2.0;
            *buffer = (int)val;
        }

        static unsafe void WriteSampleFloat32NE(IntPtr ptr, double sample)
        {
            float* buffer = (float*)ptr;
            *buffer = (float)sample;
        }

        static unsafe void WriteSampleFloat64NE(IntPtr ptr, double sample)
        {
            double* buffer = (double*)ptr;
            *buffer = sample;
        }

        static Action<IntPtr, double> writeSample;
        static double secondsOffset = 0.0;
        static bool wantPause = false;

        static void WriteCallback(SoundIOOutStream stream, int frameCountMin, int frameCountMax)
        {
            double floatSampleRate = stream.SampleRate;
            double secondsPerFrame = 1.0 / floatSampleRate;
            SoundIOChannelAreas areas;
            SoundIoError err;
            int framesLeft = frameCountMax;

            while (true)
            {
                int frameCount = framesLeft;
                err = stream.BeginWrite(out areas, ref frameCount);
                if (err != SoundIoError.None)
                    throw new SoundIOException(string.Format("Unrecoverable stream error: {0}.", err.GetErrorMessage()));

                if (areas == null || frameCount == 0)
                    break;

                SoundIOChannelLayout layout = stream.Layout;

                double pitch = 440.0;
                double radiansPerSecond = pitch * 2.0 * Math.PI;
                for (int frame = 0; frame < frameCount; frame++)
                {
                    double sample = Math.Sin((secondsOffset + frame * secondsPerFrame) * radiansPerSecond);
                    for (int channel = 0; channel < layout.ChannelCount; channel += 1)
                    {
                        writeSample(areas[channel].Pointer, sample);
                        areas[channel].AdvancePointer();
                    }
                    
                }
                secondsOffset = (secondsOffset + secondsPerFrame * frameCount) % 1.0;

                err = stream.EndWrite();
                if (err != SoundIoError.None)
                {
                    Console.WriteLine("EndWrite failed with error: {0}.", err);
                    if (err == SoundIoError.Underflow)
                        return;

                    throw new SoundIOException(string.Format("Unrecoverable stream error: {0}.", err.GetErrorMessage()));
                }

                framesLeft -= frameCount;
                if (framesLeft <= 0)
                    break;
            }

            stream.Pause(wantPause);
        }

        static int underflowCount = 0;

        static void UnderflowCallback(SoundIOOutStream o)
        {
            Console.Error.WriteLine("Underflow {0}.", underflowCount++);
        }

        public static int Main(string[] args)
        {
            string exe = "SoundIOSine";
            SoundIoBackend backend = SoundIoBackend.None;
            string deviceId = null;
            bool raw = false;
            string streamName = null;
            double latency = 0.0;
            int sampleRate = 0;
            SoundIoError err;
            SoundIODevice device = null;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--"))
                {
                    if (arg.CompareTo("--raw") == 0)
                        raw = true;
                    else
                    {
                        i++;
                        if (i >= args.Length)
                            return Usage(exe);
                        else if (arg.CompareTo("--backend") == 0)
                            backend = (SoundIoBackend)Enum.Parse(typeof(SoundIoBackend), args[i]);
                        else if (arg.CompareTo("--device") == 0)
                            deviceId = args[i];
                        else if (arg.CompareTo("--name") == 0)
                            streamName = args[i];
                        else if (arg.CompareTo("--latency") == 0)
                            latency = double.Parse(args[i]);
                        else if (arg.CompareTo("--sample-rate") == 0)
                            sampleRate = int.Parse(args[i]);
                        else
                            return Usage(exe);
                    }
                }
                else
                    return Usage(exe);
            }

            SoundIO soundIo = new SoundIO();

            err = (backend == SoundIoBackend.None) ?
                soundIo.Connect() : soundIo.ConnectBackend(backend);

            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("Unable to connect to backend: {0}.", err.GetErrorMessage());
                return 1;
            }

            Console.WriteLine("Backend: {0}.", soundIo.CurrentBackend);

            soundIo.FlushEvents();

            if (deviceId != null)
            {
                foreach (var dev in soundIo)
                {
                    if (dev.Aim == SoundIoDeviceAim.Output && dev.Id.Equals(deviceId) && dev.IsRaw == raw)
                    {
                        device = dev;
                        break;
                    }
                }

                if (device == null)
                {
                    Console.Error.WriteLine("Output device not found.");
                    return 1;
                }

                device.AddRef(); // Enumerator cleans up itself on dispose
            }
            else
            {
                device = soundIo.GetDefaultOutputDevice();
            }

            Console.WriteLine("Output device: {0}.", device.Name);

            if (device.ProbeError != SoundIoError.None)
            {
                Console.Error.WriteLine("Cannot probe device: {0}.", device.ProbeError.GetErrorMessage());
                return 1;
            }

            SoundIOOutStream outstream = new SoundIOOutStream(device);

            outstream.OnWriteCallback = WriteCallback;
            outstream.OnUnderflowCallback = UnderflowCallback;
            if (streamName != null)
                outstream.Name = streamName;
            outstream.SoftwareLatency = latency;
            if (sampleRate != 0)
                outstream.SampleRate = sampleRate;

            if (device.SupportsFormat(SoundIoFormats.Float32NE))
            {
                outstream.Format = SoundIoFormats.Float32NE;
                writeSample = WriteSampleFloat32NE;
            }
            else if (device.SupportsFormat(SoundIoFormats.Float64NE))
            {
                outstream.Format = SoundIoFormats.Float64NE;
                writeSample = WriteSampleFloat64NE;
            }
            else if (device.SupportsFormat(SoundIoFormats.S32NE))
            {
                outstream.Format = SoundIoFormats.S32NE;
                writeSample = WriteSampleS32NE;
            }
            else if (device.SupportsFormat(SoundIoFormats.S16NE))
            {
                outstream.Format = SoundIoFormats.S16NE;
                writeSample = WriteSampleS16NE;
            }
            else
            {
                Console.Error.WriteLine("No suitable format available.");
                return 1;
            }

            err = outstream.Open();
            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("Unable to open device: {0}.", err.GetErrorMessage());
                return 1;
            }

            Console.WriteLine("Software latency: {0:N6}.", outstream.SoftwareLatency);
            Console.WriteLine(
                "\t'p' - pause\n" +
                "\t'u' - unpause\n" +
                "\t'P' - pause from within callback\n" +
                "\t'c' - clear buffer\n" +
                "\t'q' - quit\n");

            if (outstream.LayoutError != SoundIoError.None)
                Console.Error.WriteLine("Unable to set channel layout: {0}.", outstream.LayoutError.GetErrorMessage());

            err = outstream.Start();
            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("Unable to start device {0}.", err.GetErrorMessage());
                return 1;
            }

            while (true)
            {
                soundIo.FlushEvents();

                int c = Console.Read();
                if (c == 'p')
                {
                    err = outstream.Pause(true);
                    Console.Error.WriteLine("Pausing result: {0}.", err.GetErrorMessage());
                }
                else if (c == 'P')
                {
                    wantPause = true;
                }
                else if (c == 'u')
                {
                    wantPause = false;
                    err = outstream.Pause(false);
                    Console.Error.WriteLine("Unpausing result: {0}.", err.GetErrorMessage());
                }
                else if (c == 'c')
                {
                    err = outstream.ClearBuffer();
                    Console.Error.WriteLine("Clear buffer result: {0}.", err.GetErrorMessage());
                }
                else if (c == 'q')
                {
                    break;
                }
                else if (c == '\r' || c == '\n')
                {
                    // ignore
                }
                else
                {
                    Console.Error.WriteLine("Unrecognized command: {0}.", (char)c);
                }
            }

            outstream.Dispose();
            device.Release();
            soundIo.Dispose();
            return 0;
        }
    }
}

//
// SoundIOListDevices sample
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

namespace SoundIOListDevices
{
    class Program
    {
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

        static void PrintChannelLayout(SoundIOChannelLayout layout)
        {
            if (layout.Name != null)
            {
                Console.Write("{0}", layout.Name);
            }
            else
            {
                Console.WriteLine("{0}", layout.Channels[0].GetChannelName());
                for (int i = 1; i < layout.ChannelCount; i++)
                {
                    Console.WriteLine("{0}", layout.Channels[i].GetChannelName());
                }
            }
        }

        static bool shortOutput = false;

        static void PrintDevice(SoundIODevice device, bool isDefault)
        {
            string defaultStr = isDefault ? " (default)" : string.Empty;
            string rawStr = device.IsRaw ? " (raw)" : string.Empty;
            Console.WriteLine("{0}{1}{2}", device.Name, defaultStr, rawStr);
            if (shortOutput)
                return;
            Console.WriteLine("  Id: {0}.", device.Id);

            if (device.ProbeError != SoundIoError.None)
            {
                Console.Error.WriteLine("  Probe error: {0}.", device.ProbeError.GetErrorMessage());
            }
            else
            {
                Console.WriteLine("  Channel layouts:");
                for (int i = 0; i < device.LayoutCount; i++)
                {
                    Console.Write("    ");
                    PrintChannelLayout(device.Layouts[i]);
                    Console.WriteLine();
                }
                if (device.CurrentLayout.ChannelCount > 0)
                {
                    Console.Write("  Current layout: ");
                    PrintChannelLayout(device.CurrentLayout);
                    Console.WriteLine();
                }

                Console.WriteLine("  Sample rates:");
                for (int i = 0; i < device.SamleRateCount; i++)
                {
                    SoundIoSampleRateRange range = device.SampleRates[i];
                    Console.WriteLine("    {0} - {1}", range.Min, range.Max);
                }
                if (device.CurrentSampleRate != 0)
                    Console.WriteLine("  Current sample rate: {0}.", device.CurrentSampleRate);
                Console.Write("  Formats: ");
                for (int i = 0; i < device.FormatCount; i++)
                {
                    string comma = (i == device.FormatCount - 1) ? string.Empty : ", ";
                    Console.Write("{0}{1}", device.Formats[i].GetFormatString(), comma);
                }
                Console.WriteLine();
                if (device.CurrentFormat != SoundIoFormat.Invalid)
                    Console.WriteLine("  Current format: {0}.", device.CurrentFormat.GetFormatString());

                Console.WriteLine("  Min software latency: {0:N8} sec.", device.SoftwareLatencyMin);
                Console.WriteLine("  Max software latency: {0:N8} sec.", device.SoftwareLatencyMax);
                if (device.SoftwareLatencyCurrent != 0)
                    Console.WriteLine("  Current software latency: {0:N8} sec.", device.SoftwareLatencyCurrent);
            }
            Console.WriteLine();
        }

        static int ListDevices(SoundIO soundIo)
        {
            int outputCount = soundIo.GetOutputDeviceCount();
            int inputCount = soundIo.GetInputDeviceCount();

            int defaultOutput = soundIo.GetDefaultOutputDeviceIndex();
            int defaultInput = soundIo.GetDefaultInputDeviceIndex();

            Console.WriteLine("--------Input Devices--------");
            for (int i = 0; i < inputCount; i++)
            {
                SoundIODevice device = soundIo.GetInputDevice(i);
                PrintDevice(device, defaultInput == i);
                device.Release();
            }
            Console.WriteLine("\n--------Output Devices--------\n");
            for (int i = 0; i < outputCount; i++)
            {
                SoundIODevice device = soundIo.GetOutputDevice(i);
                PrintDevice(device, defaultOutput == i);
                device.Release();
            }

            Console.WriteLine("\n{0} devices found.", inputCount + outputCount);
            return 0;
        }

        static void OnDevicesChange(SoundIO soundIo)
        {
            Console.WriteLine("Devices changed.");

        }

        static int Main(string[] args)
        {
            string exe = "SoundIOListDevices";
            bool watch = false;
            SoundIoBackend backend = SoundIoBackend.None;
            SoundIoError err;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.CompareTo("--watch") == 0)
                    watch = true;
                else if (arg.CompareTo("--short") == 0)
                    shortOutput = true;
                else if (arg.StartsWith("--"))
                {
                    i++;
                    if (i >= args.Length)
                        return Usage(exe);
                    else if (arg.CompareTo("--backend") == 0)
                        backend = (SoundIoBackend)Enum.Parse(typeof(SoundIoBackend), args[i]);
                    else
                        return Usage(exe);
                }
                else
                    return Usage(exe);
            }

            SoundIO soundIo = new SoundIO();

            err = (backend == SoundIoBackend.None) ?
                soundIo.Connect() : soundIo.ConnectBackend(backend);

            if (err != SoundIoError.None)
            {
                Console.Error.WriteLine("{0}.", err.GetErrorMessage());
                return 1;
            }

            if (watch)
            {
                soundIo.OnDevicesChange = OnDevicesChange;
                while (true)
                {
                    soundIo.WaitEvents();
                }
            }
            else
            {
                soundIo.FlushEvents();
                err = (SoundIoError)ListDevices(soundIo);
                soundIo.Dispose();
                return (int)err;
            }
        }
    }
}

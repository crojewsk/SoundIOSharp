# SoundIOSharp

C# .NET cross-platform library for audio input and output.
Wrapper for [libsoundio](https://github.com/andrewrk/libsoundio) by andrewrk.
Provides high level objective API over said library as well as exposing all functions and types available for export.

 * Fully C# Windows and Mono compatible
 * Buildable both in VisualStudio and MonoDevelop
 * No unsafe code found within main SoundIOSharp project
 * Several convenient wrappers, IEnumerable and IEquatable among them
 * Code is well documented

## Interface and naming convention

 * Standard C# Microsoft naming rules
 * Each and every native soundio structure, field or function starts with 'SoundIo'
 * Almost all high level soundio classes, fields or methods starts with 'SoundIO'
 * Functions taking enum constants as arguments are found within ExtensionMethods static class
 * SoundIoFormats static class contains convenient format constants
 * Checkout samples for more info

## Limitations

 * libsoundio limitations apply here
 * Supported operating systems:
   - Windows, .NET 4.0+
   - MacOS , [Mono](http://www.mono-project.com/)
   - Linux, [Mono](http://www.mono-project.com/)
 * Supported backends:
   - [JACK](http://jackaudio.org/)
   - [PulseAudio](http://www.freedesktop.org/wiki/Software/PulseAudio/)
   - [ALSA](http://www.alsa-project.org/)
   - [CoreAudio](https://developer.apple.com/library/mac/documentation/MusicAudio/Conceptual/CoreAudioOverview/Introduction/Introduction.html)
   - [WASAPI](https://msdn.microsoft.com/en-us/library/windows/desktop/dd371455%28v=vs.85%29.aspx)
   - Dummy (silence)

## Attached libraries in .\libs

 * libsoundio.dylib, MacOS, CoreAudio
 * libsoundio.so, Linux x64 ALSA
 * soundio.dll, Windows x64 WASAPI

With SoundIOSharp, sample projects are provided that are based on original samples by andrewrk.
Examples cover subjects of sine wave output, simple recording and device enumeration.
 
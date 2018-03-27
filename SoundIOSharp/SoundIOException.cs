//
// SoundIOException.cs
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
    /// The exception that is thrown when a method call to libsoundio fails.
    /// </summary>
    [Serializable]
    public class SoundIOException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoundIOException"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SoundIOException(string message)
            : base(string.Format(message))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundIOException"/> class with an error message
        /// adequate to specified error code.
        /// </summary>
        /// <param name="error">Error code to retrieve error message for.</param>
        public SoundIOException(SoundIoError error)
            : base(error.GetErrorMessage())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundIOException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SoundIOException(string message, Exception innerException)
            : base(string.Format(message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoundIOException"/> class with an error message
        /// adequate to specified error code and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="error">Error code to retrieve error message for.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SoundIOException(SoundIoError error, Exception innerException)
            : base(error.GetErrorMessage(), innerException)
        {
        }
    }
}

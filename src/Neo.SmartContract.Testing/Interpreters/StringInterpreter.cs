// Copyright (C) 2015-2025 The Neo Project.
//
// StringInterpreter.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;
using System.Text;

namespace Neo.SmartContract.Testing.Interpreters
{
    public class StringInterpreter : IStringInterpreter
    {
        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// A strict UTF8 encoding used in NEO system.
        /// </summary>
        public static readonly StringInterpreter StrictUTF8 = new(Encoding.UTF8, true);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strict">Strict</param>
        public StringInterpreter(Encoding encoding, bool strict)
        {
            if (strict)
            {
                Encoding = (Encoding)Encoding.UTF8.Clone();
                Encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
                Encoding.EncoderFallback = EncoderFallback.ExceptionFallback;
            }
            else
            {
                Encoding = encoding;
            }
        }

        /// <summary>
        /// Get string from bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Value</returns>
        public virtual string GetString(ReadOnlySpan<byte> bytes)
        {
            return Encoding.GetString(bytes);
        }
    }
}

// Copyright (C) 2015-2025 The Neo Project.
//
// UInt160.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace Neo.SmartContract.Framework
{
    public abstract class UInt160 : ByteString
    {
        public static extern UInt160 Zero { [OpCode(OpCode.PUSHDATA1, "140000000000000000000000000000000000000000")] get; }

        public extern bool IsZero
        {
            [OpCode(OpCode.PUSH0)]
            [OpCode(OpCode.NUMEQUAL)]
            get;
        }

        public extern bool IsValid
        {
            [OpCode(OpCode.DUP)]
            [OpCode(OpCode.ISTYPE, "0x28")] //ByteString
            [OpCode(OpCode.JMPIF, "06")]  // to SIZE
            [OpCode(OpCode.DROP)]
            [OpCode(OpCode.PUSHF)]
            [OpCode(OpCode.JMP, "06")]    // to the end
            [OpCode(OpCode.SIZE)]
            [OpCode(OpCode.PUSHINT8, "14")] // 0x14 == 20 bytes expected array size
            [OpCode(OpCode.NUMEQUAL)]
            get;
        }

        public bool IsValidAndNotZero => IsValid && !IsZero;

        [OpCode(OpCode.CONVERT, StackItemType.ByteString)]
        [OpCode(OpCode.DUP)]
        [OpCode(OpCode.ISNULL)]
        [OpCode(OpCode.JMPIF, "09")]
        [OpCode(OpCode.DUP)]
        [OpCode(OpCode.SIZE)]
        [OpCode(OpCode.PUSHINT8, "14")] // 0x14 == 20 bytes expected array size
        [OpCode(OpCode.JMPEQ, "03")]
        [OpCode(OpCode.THROW)]
        public static extern explicit operator UInt160(byte[] value);

        [OpCode(OpCode.CONVERT, StackItemType.Buffer)]
        public static extern explicit operator byte[](UInt160 value);

        /// <summary>
        /// Converts the specified script hash to an address, using the current blockchain AddressVersion value.
        /// </summary>
        /// <returns>The converted address.</returns>
        public string ToAddress()
        {
            return ToAddress(Runtime.AddressVersion);
        }

        /// <summary>
        /// Converts the specified script hash to an address.
        /// </summary>
        /// <param name="version">The address version.</param>
        /// <returns>The converted address.</returns>
        public string ToAddress(byte version)
        {
            byte[] data = { version };
            data = Helper.Concat(data, this);
            return StdLib.Base58CheckEncode((ByteString)data);
        }

        /// <summary>
        /// Implicitly converts a hexadecimal string to a UInt160 object.
        /// This can be a 20 bytes hex string or a neo address.
        /// <example>
        /// 20 bytes hex string: "01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4" (no prefix)
        ///             Address: "NZNosnRn6FpRjwGKx8VdXv5Sn7BvzrjZVb"
        /// </example>
        /// <remarks>
        /// This is a compile time conversion, only work with constant string.
        /// If you want to convert a runtime string, convert it to byte[] first.
        /// </remarks>
        /// </summary>
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
        public static extern implicit operator UInt160(string value);
#pragma warning restore CS0626 // Method, operator, or accessor is marked external and has no attributes on it
    }
}

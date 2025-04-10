// Copyright (C) 2015-2025 The Neo Project.
//
// Contract_Storage.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System;
using Contract = Neo.SmartContract.Framework.Services.Contract;

namespace Neo.Compiler.CSharp.TestContracts
{
    [ContractPermission("*", "*")]
    public class Contract_Storage : SmartContract.Framework.SmartContract
    {
        #region A

        public static bool MainA(UInt160 callee, bool throwInB)
        {
            Storage.Put(new byte[] { 0xA0 }, 1);
            Storage.Put(new byte[] { 0xA1 }, 1);
            try
            {
                Storage.Put(new byte[] { 0xA1 }, 2);  // valid
                Contract.Call(callee, "mainB", CallFlags.All, new object[] { Runtime.ExecutingScriptHash, throwInB });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void WriteA()
        {
            Storage.Put(new byte[] { 0xA0 }, 2);
        }

        #endregion

        #region B

        public static bool MainB(UInt160 callerA, bool throw_)
        {
            Storage.Put(new byte[] { 0xB0 }, 1);
            WriteB();
            try
            {
                Contract.Call(callerA, "writeA", CallFlags.All);
                throw new Exception("B");
            }
            catch { }

            if (throw_)
                throw new Exception("B");

            return true;
        }

        public static void WriteB()
        {
            Storage.Put(new byte[] { 0xB1 }, 2);
        }

        #endregion
    }
}

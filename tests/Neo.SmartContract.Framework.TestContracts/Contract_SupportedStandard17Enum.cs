// Copyright (C) 2015-2025 The Neo Project.
//
// Contract_SupportedStandard17Enum.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract.Framework.Attributes;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework.Interfaces;

namespace Neo.SmartContract.Framework.UnitTests.TestClasses
{
    [DisplayName(nameof(Contract_SupportedStandard17Enum))]
    [ManifestExtra("Author", "<Your Name Or Company Here>")]
    [ContractDescription("<Description Here>")]
    [ManifestExtra("Email", "<Your Public Email Here>")]
    [ManifestExtra("Version", "<Version String Here>")]
    [ContractSourceCode("https://github.com/neo-project/neo-devpack-dotnet/tree/master/src/Neo.SmartContract.Template")]
    [ContractPermission(Permission.Any, Method.Any)]
    [SupportedStandards(NepStandard.Nep17)]
    public class Contract_SupportedStandard17Enum : Nep17Token, INEP27
    {
        public override string Symbol { [Safe] get; } = "EXAMPLE";
        public override byte Decimals { [Safe] get; } = 0;

        public void OnNEP17Payment(UInt160 from, BigInteger amount, object? data = null)
        {
        }
    }
}

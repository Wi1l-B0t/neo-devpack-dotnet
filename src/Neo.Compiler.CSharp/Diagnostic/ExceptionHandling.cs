// Copyright (C) 2015-2025 The Neo Project.
//
// ExceptionHandling.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.CodeAnalysis;
using Neo.VM;
using System.Collections.Generic;

namespace Neo.Compiler
{
    class ExceptionHandling
    {
        public ExceptionHandlingState State;
        public HashSet<ILabelSymbol> Labels = new(SymbolEqualityComparer.Default);
        public List<Instruction> PendingGotoStatments = new();
        public int SwitchCount = 0;
        public int ContinueTargetCount = 0;
        public int BreakTargetCount = 0;
    }
}

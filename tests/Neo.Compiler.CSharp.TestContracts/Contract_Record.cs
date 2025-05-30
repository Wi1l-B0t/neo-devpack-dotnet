// Copyright (C) 2015-2025 The Neo Project.
//
// Contract_Record.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;

namespace Neo.Compiler.CSharp.TestContracts
{
    record Student(string name, int age);

    public record StudentR
    {
        public string Name { get; set; }
        public int Age { get; init; }

        public StudentR(string n)
        {
            Name = n;
        }
    }

    public class Contract_Record : SmartContract.Framework.SmartContract
    {
        public static object Test_CreateRecord(string n, int a)
        {
            var p = new Student(n, a);
            return p;
        }

        public static object Test_CreateRecord2(string n, int a)
        {
            var p = new StudentR(n) { Age = a };
            return p;
        }

        public static object Test_UpdateRecord(string n, int a)
        {
            var p = new Student(n, a);
            var p2 = p with { age = a + 1 };
            return p;
        }

        public static object Test_UpdateRecord2(string n, int a)
        {
            var p = new Student(n, a);
            var p2 = p with { age = a + 1, name = "0" + n };
            return p2;
        }

        public static string Test_DeconstructRecord(string n, int a)
        {
            var p = new Student(n, a);
            var (name, age) = p;
            return name;
        }
    }
}

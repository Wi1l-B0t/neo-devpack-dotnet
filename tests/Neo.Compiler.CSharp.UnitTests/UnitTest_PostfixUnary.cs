// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_PostfixUnary.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.SmartContract.Testing;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_PostfixUnary : DebugAndTestBase<Contract_PostfixUnary>
    {
        [TestMethod]
        public void Test()
        {
            Assert.AreEqual("John", Contract.Test());
        }

        [TestMethod]
        public void TestUndefinedCase()
        {
            Contract.TestUndefinedCase();
        }

        [TestMethod]
        public void TestInvert()
        {
            Contract.TestInvert();
        }
    }
}

// Copyright (C) 2015-2025 The Neo Project.
//
// UnitTest_ByteArrayAssignment.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.SmartContract.Testing;
using Neo.SmartContract.Testing.Exceptions;
using System;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_ByteArrayAssignment : DebugAndTestBase<Contract_ByteArrayAssignment>
    {
        [TestMethod]
        public void Test_ByteArrayAssignment()
        {
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x04 }, Contract.TestAssignment());
            AssertGasConsumed(1724190);
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentOutOfBounds()
        {
            Assert.ThrowsException<TestException>(Contract.TestAssignmentOutOfBounds);
            AssertGasConsumed(1724070);
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentOverflow()
        {
            CollectionAssert.AreEqual(new byte[] { 0xff, 0x02, 0x03 }, Contract.TestAssignmentOverflow());
            AssertGasConsumed(1478820);
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentWrongCasting()
        {
            var exception = Assert.ThrowsException<TestException>(Contract.TestAssignmentWrongCasting);
            AssertGasConsumed(1478340);
            Assert.IsInstanceOfType<InvalidOperationException>(exception.InnerException);
            AssertGasConsumed(1478340);
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentDynamic()
        {
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x0a }, Contract.TestAssignmentDynamic(10));
            AssertGasConsumed(1546590);
        }
    }
}

// Copyright (C) 2015-2025 The Neo Project.
//
// TriggerType.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

namespace Neo.SmartContract.Framework.Services
{
    public enum TriggerType : byte
    {
        /// <summary>
        /// Indicate that the contract is triggered by the system to execute the OnPersist method of the native contracts.
        /// </summary>
        OnPersist = 0x01,

        /// <summary>
        /// Indicate that the contract is triggered by the system to execute the PostPersist method of the native contracts.
        /// </summary>
        PostPersist = 0x02,

        /// <summary>
        /// Indicates that the contract is triggered by the verification of a <see cref="IVerifiable"/>.
        /// </summary>
        Verification = 0x20,

        /// <summary>
        /// Indicates that the contract is triggered by the execution of transactions.
        /// </summary>
        Application = 0x40,

        /// <summary>
        /// The combination of all system triggers.
        /// </summary>
        System = OnPersist | PostPersist,

        /// <summary>
        /// The combination of all triggers.
        /// </summary>
        All = OnPersist | PostPersist | Verification | Application
    }
}

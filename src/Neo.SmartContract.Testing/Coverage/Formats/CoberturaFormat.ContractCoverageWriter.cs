// Copyright (C) 2015-2025 The Neo Project.
//
// CoberturaFormat.ContractCoverageWriter.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Neo.SmartContract.Testing.Coverage.Formats
{
    public partial class CoberturaFormat
    {
        internal class ContractCoverageWriter
        {
            readonly CoveredContract Contract;
            readonly NeoDebugInfo DebugInfo;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="contract">Contract</param>
            /// <param name="debugInfo">Debug info</param>
            public ContractCoverageWriter(CoveredContract contract, NeoDebugInfo debugInfo)
            {
                Contract = contract;
                DebugInfo = debugInfo;
            }

            public void WritePackage(XmlWriter writer)
            {
                var allMethods = DebugInfo.Methods.SelectMany(m => m.SequencePoints).ToArray();
                var (lineCount, hitCount) = GetLineRate(Contract, allMethods);
                var lineRate = CoverageBase.CalculateHitRate(lineCount, hitCount);
                var (branchCount, branchHit) = GetBranchRate(Contract, allMethods);
                var branchRate = CoverageBase.CalculateHitRate(branchCount, branchHit);

                writer.WriteStartElement("package");
                // TODO: complexity
                writer.WriteAttributeString("name", DebugInfo.Hash.ToString());
                writer.WriteAttributeString("scripthash", $"{DebugInfo.Hash}");
                writer.WriteAttributeString("line-rate", $"{lineRate:N4}");
                writer.WriteAttributeString("branch-rate", $"{branchRate:N4}");
                writer.WriteStartElement("classes");
                {
                    foreach (var group in DebugInfo.Methods.GroupBy(NamespaceAndFilename))
                    {
                        foreach (var entry in group.Key)
                        {
                            WriteClass(writer, entry.@namespace, entry.filename, group);
                        }
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndElement();

                IEnumerable<(string @namespace, string filename)> NamespaceAndFilename(NeoDebugInfo.Method method)
                {
                    var indexes = method.SequencePoints
                        .Select(sp => sp.Document)
                        .Distinct()
                        .ToList();

                    foreach (var index in indexes)
                    {
                        if (index >= 0 && index < DebugInfo.Documents.Count)
                        {
                            yield return (method.Namespace, DebugInfo.Documents[index]);
                        }
                    }
                }
            }

            internal void WriteClass(XmlWriter writer, string name, string filename, IEnumerable<NeoDebugInfo.Method> methods)
            {
                var allMethods = methods.SelectMany(m => m.SequencePoints).ToArray();
                var (lineCount, hitCount) = GetLineRate(Contract, allMethods);
                var lineRate = CoverageBase.CalculateHitRate(lineCount, hitCount);
                var (branchCount, branchHit) = GetBranchRate(Contract, allMethods);
                var branchRate = CoverageBase.CalculateHitRate(branchCount, branchHit);

                writer.WriteStartElement("class");
                // TODO: complexity
                writer.WriteAttributeString("name", name);
                if (filename.Length > 0)
                {
                    writer.WriteAttributeString("filename", filename);
                }
                writer.WriteAttributeString("line-rate", $"{lineRate:N4}");
                writer.WriteAttributeString("branch-rate", $"{branchRate:N4}");

                writer.WriteStartElement("methods");
                foreach (var method in methods)
                {
                    WriteMethod(writer, method);
                }
                writer.WriteEndElement();

                writer.WriteStartElement("lines");
                foreach (var method in methods)
                {
                    foreach (var sp in method.SequencePoints)
                    {
                        WriteLine(writer, method, sp);
                    }
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            internal void WriteMethod(XmlWriter writer, NeoDebugInfo.Method method)
            {
                var signature = string.Join(", ", method.Parameters.Select(p => p.Type));
                var (lineCount, hitCount) = GetLineRate(Contract, method.SequencePoints);
                var lineRate = CoverageBase.CalculateHitRate(lineCount, hitCount);
                var (branchCount, branchHit) = GetBranchRate(Contract, method.SequencePoints);
                var branchRate = CoverageBase.CalculateHitRate(branchCount, branchHit);

                writer.WriteStartElement("method");
                writer.WriteAttributeString("name", method.Name);
                writer.WriteAttributeString("signature", $"({signature})");
                writer.WriteAttributeString("line-rate", $"{lineRate:N4}");
                writer.WriteAttributeString("branch-rate", $"{branchRate:N4}");
                writer.WriteStartElement("lines");
                foreach (var sp in method.SequencePoints)
                {
                    WriteLine(writer, method, sp);
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            internal void WriteLine(XmlWriter writer, NeoDebugInfo.Method method, NeoDebugInfo.SequencePoint sp)
            {
                var hits = Contract.TryGetLine(sp.Address, out var value) ? value.Hits : 0;

                writer.WriteStartElement("line");
                writer.WriteAttributeString("number", $"{sp.Start.Line}");
                writer.WriteAttributeString("address", $"{sp.Address}");
                writer.WriteAttributeString("hits", $"{hits}");

                if (!Contract.TryGetBranch(sp.Address, out var branch))
                {
                    writer.WriteAttributeString("branch", $"{false}");
                }
                else
                {
                    int branchCount = branch.Count;
                    int branchHit = branch.Hits;
                    var branchRate = CoverageBase.CalculateHitRate(branchCount, branchHit);

                    writer.WriteAttributeString("branch", $"{true}");
                    writer.WriteAttributeString("condition-coverage", $"{branchRate * 100:N}% ({branchHit}/{branchCount})");
                    writer.WriteStartElement("conditions");

                    foreach ((var address, var branchI) in GetBranchInstructions(Contract, method, sp))
                    {
                        var (condBranchCount, condContinueCount) = (branchI.Count, branchI.Hits);
                        var coverage = condBranchCount == 0 ? 0m : 1m;
                        coverage += condContinueCount == 0 ? 0m : 1m;

                        writer.WriteStartElement("condition");
                        writer.WriteAttributeString("number", $"{address}");
                        writer.WriteAttributeString("type", $"{branchI.Offset}"); // TODO: Opcode?
                        writer.WriteAttributeString("coverage", $"{coverage / 2m * 100m}%");
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
    }
}

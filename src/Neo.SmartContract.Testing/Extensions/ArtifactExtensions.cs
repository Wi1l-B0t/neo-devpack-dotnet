// Copyright (C) 2015-2025 The Neo Project.
//
// ArtifactExtensions.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Disassembler.CSharp;
using Neo.Extensions;
using Neo.Json;
using Neo.SmartContract.Manifest;
using Neo.SmartContract.Testing.TestingStandards;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Neo.SmartContract.Testing.Extensions
{
    public static class ArtifactExtensions
    {
        static readonly string[] _protectedWords =
        [
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while"
        ];

        /// <summary>
        /// Get source code from contract Manifest
        /// </summary>
        /// <param name="manifest">Manifest</param>
        /// <param name="name">Class name, by default is manifest.Name</param>
        /// <param name="nef">Nef file</param>
        /// <param name="generateProperties">Generate properties</param>
        /// <param name="debugInfo">NEP-19 debug information</param>
        /// <param name="traceRemarks">Trace remarks</param>
        /// <returns>Source</returns>
        public static string GetArtifactsSource(this ContractManifest manifest, string? name = null, NefFile? nef = null, bool generateProperties = true, JToken? debugInfo = null, bool traceRemarks = false)
        {
            name ??= manifest.Name;
            var builder = new StringBuilder();
            using var sourceCode = new StringWriter(builder)
            {
                NewLine = "\n"
            };

            var inheritance = new List<Type>
            {
                typeof(SmartContract)
            };

            if (manifest.IsNep17()) inheritance.Add(typeof(INep17Standard));
            if (manifest.IsOwnable()) inheritance.Add(typeof(IOwnable));
            if (manifest.IsVerificable()) inheritance.Add(typeof(IVerificable));

            sourceCode.WriteLine("using Neo.Cryptography.ECC;");
            sourceCode.WriteLine("using Neo.Extensions;");
            sourceCode.WriteLine("using System;");
            sourceCode.WriteLine("using System.Collections.Generic;");
            sourceCode.WriteLine("using System.ComponentModel;");
            sourceCode.WriteLine("using System.Numerics;");
            sourceCode.WriteLine("");
            sourceCode.WriteLine("namespace Neo.SmartContract.Testing;");
            sourceCode.WriteLine("");
            sourceCode.WriteLine($"public abstract class {name}({typeof(SmartContractInitialize).FullName} initialize) : " + FormatInheritance(inheritance, "initialize") + ", IContractInfo");
            sourceCode.WriteLine("{");

            // Write compiled data

            sourceCode.WriteLine("    #region Compiled data");
            sourceCode.WriteLine();

            var value = manifest.ToJson().ToString(false).Replace("\"", "\"\"");
            sourceCode.WriteLine($"    public static {typeof(ContractManifest).FullName} Manifest => {typeof(ContractManifest).FullName}.Parse(@\"{value}\");");
            sourceCode.WriteLine();

            if (nef is not null)
            {
                value = Convert.ToBase64String(nef.ToArray()).Replace("\"", "\"\"");

                var optimization = manifest.Extra?["nef"]?["optimization"]?.ToString();

                if (optimization != null)
                {
                    sourceCode.WriteLine($"    /// <summary>");
                    sourceCode.WriteLine($"    /// Optimization: {optimization}");
                    sourceCode.WriteLine($"    /// </summary>");
                }
                sourceCode.WriteLine($"    public static {typeof(NefFile).FullName} Nef => Convert.FromBase64String(@\"{value}\").AsSerializable<{typeof(NefFile).FullName}>();");
                sourceCode.WriteLine();
            }

            sourceCode.WriteLine("    #endregion");
            sourceCode.WriteLine();

            // Crete events

            if (manifest.Abi.Events.Any())
            {
                sourceCode.WriteLine("    #region Events");
                sourceCode.WriteLine();

                foreach (var ev in manifest.Abi.Events.OrderBy(u => u.Name))
                {
                    sourceCode.Write(CreateSourceEventFromManifest(ev, inheritance));
                    sourceCode.WriteLine();
                }

                sourceCode.WriteLine("    #endregion");
                sourceCode.WriteLine();
            }

            // Create methods

            var methods = manifest.Abi.Methods;

            if (generateProperties)
            {
                (methods, var properties) = ProcessAbiMethods(manifest.Abi.Methods);

                if (properties.Any())
                {
                    sourceCode.WriteLine("    #region Properties");
                    sourceCode.WriteLine();

                    foreach (var property in properties.OrderBy(u => u.getter.Name))
                    {
                        sourceCode.Write(CreateSourcePropertyFromManifest(property.getter, property.setter));
                        sourceCode.WriteLine();
                    }

                    sourceCode.WriteLine("    #endregion");
                    sourceCode.WriteLine();
                }
            }

            if (methods.Any(u => u.Safe))
            {
                sourceCode.WriteLine("    #region Safe methods");
                sourceCode.WriteLine();

                foreach (var method in methods.Where(u => u.Safe).OrderBy(u => u.Name))
                {
                    // This method can't be called, so avoid them

                    if (method.Name.StartsWith('_')) continue;

                    sourceCode.Write(CreateSourceMethodFromManifest(method, nef, debugInfo, traceRemarks));
                    sourceCode.WriteLine();
                }

                sourceCode.WriteLine("    #endregion");
                sourceCode.WriteLine();
            }

            if (methods.Any(u => !u.Safe))
            {
                sourceCode.WriteLine("    #region Unsafe methods");
                sourceCode.WriteLine();

                foreach (var method in methods.Where(u => !u.Safe).OrderBy(u => u.Name))
                {
                    // This method can't be called, so avoid them

                    if (method.Name.StartsWith('_')) continue;

                    sourceCode.Write(CreateSourceMethodFromManifest(method, nef, debugInfo, traceRemarks));
                    sourceCode.WriteLine();
                }
                sourceCode.WriteLine("    #endregion");
            }

            sourceCode.WriteLine("}");

            return sourceCode.ToString();
        }

        private static (ContractMethodDescriptor[] methods, (ContractMethodDescriptor getter, ContractMethodDescriptor? setter)[] properties)
            ProcessAbiMethods(ContractMethodDescriptor[] methods)
        {
            HashSet<string> propertyNames = new();
            List<ContractMethodDescriptor> methodList = new(methods);
            List<(ContractMethodDescriptor, ContractMethodDescriptor?)> properties = new();

            // Detect and extract properties, first find Safe && 0 args && return != void

            foreach (ContractMethodDescriptor getter in methods.Where(u => u.Safe && u.Parameters.Length == 0 && u.ReturnType != ContractParameterType.Void).ToArray())
            {
                // Find setter: setXXX && one arg && not safe && parameter = getter.return && return == void

                var setter = getter.Name.StartsWith("get") ? // Only find setter if start with get
                    methodList.FirstOrDefault(
                    u =>
                        u.Name == "set" + getter.Name[3..] &&
                        !u.Safe &&
                        u.Parameters.Length == 1 &&
                        u.Parameters[0].Type == getter.ReturnType &&
                        u.ReturnType == ContractParameterType.Void
                        ) : null;

                // Avoid property repetition

                var propertyName = GetPropertyName(getter);
                if (!propertyNames.Add(propertyName)) continue;

                // Add property and remove method

                properties.Add((getter, setter));
                methodList.Remove(getter);

                if (setter != null)
                {
                    methodList.Remove(setter);
                }
            }

            return (methodList.ToArray(), properties.ToArray());
        }

        private static string GetPropertyName(ContractMethodDescriptor getter)
        {
            return TongleLowercase(EscapeName(getter.Name.StartsWith("get") ? getter.Name[3..] : getter.Name));
        }

        /// <summary>
        /// Create source code from event
        /// </summary>
        /// <param name="ev">Event</param>
        /// <param name="inheritance">Inheritance</param>
        /// <returns>Source</returns>
        private static string CreateSourceEventFromManifest(ContractEventDescriptor ev, IList<Type> inheritance)
        {
            var builder = new StringBuilder();
            using var sourceCode = new StringWriter(builder)
            {
                NewLine = "\n"
            };

            switch (ev.Name)
            {
                case "Transfer":
                    {
                        if (inheritance.Contains(typeof(INep17Standard)) && ev.Parameters.Length == 3 &&
                            ev.Parameters[0].Type == ContractParameterType.Hash160 &&
                            ev.Parameters[1].Type == ContractParameterType.Hash160 &&
                            ev.Parameters[2].Type == ContractParameterType.Integer)
                        {
                            sourceCode.WriteLine($"    [DisplayName(\"{ev.Name}\")]");
                            sourceCode.WriteLine($"    public event {typeof(INep17Standard).FullName}.delTransfer? OnTransfer;");
                            return builder.ToString();
                        }

                        break;
                    }
                case "SetOwner":
                    {
                        if (inheritance.Contains(typeof(IOwnable)) && ev.Parameters.Length == 2 &&
                            ev.Parameters[0].Type == ContractParameterType.Hash160 &&
                            ev.Parameters[1].Type == ContractParameterType.Hash160)
                        {
                            sourceCode.WriteLine($"    [DisplayName(\"{ev.Name}\")]");
                            sourceCode.WriteLine($"    public event {typeof(IOwnable).FullName}.delSetOwner? OnSetOwner;");
                            return builder.ToString();
                        }

                        break;
                    }
            }

            // Add On prefix

            var evName = TongleLowercase(EscapeName(ev.Name));
            if (!evName.StartsWith("On")) evName = "On" + evName;

            // Compose delegate

            sourceCode.Write($"    public delegate void del{ev.Name}(");

            var isFirst = true;
            foreach (var arg in ev.Parameters)
            {
                if (!isFirst) sourceCode.Write(", ");
                else isFirst = false;

                sourceCode.Write($"{TypeToSource(arg.Type)} {EscapeName(arg.Name)}");
            }

            sourceCode.WriteLine(");");
            sourceCode.WriteLine();

            // Compose event

            if (ev.Name != evName)
            {
                sourceCode.WriteLine($"    [DisplayName(\"{ev.Name}\")]");
            }
            sourceCode.WriteLine($"    public event del{ev.Name}? {evName};");

            return builder.ToString();
        }

        /// <summary>
        /// Create source code from manifest property
        /// </summary>
        /// <param name="getter">Getter</param>
        /// <param name="setter">Setter</param>
        /// <returns>Source</returns>
        private static string CreateSourcePropertyFromManifest(ContractMethodDescriptor getter, ContractMethodDescriptor? setter)
        {
            var propertyName = GetPropertyName(getter);
            var getset = setter is not null ? $"{{ [DisplayName(\"{getter.Name}\")] get; [DisplayName(\"{setter.Name}\")] set; }}" : $"{{ [DisplayName(\"{getter.Name}\")] get; }}";

            var builder = new StringBuilder();
            using var sourceCode = new StringWriter(builder)
            {
                NewLine = "\n"
            };

            sourceCode.WriteLine($"    /// <summary>");
            sourceCode.WriteLine($"    /// {(getter.Safe ? "Safe property" : "Unsafe property")}");
            sourceCode.WriteLine($"    /// </summary>");
            sourceCode.WriteLine($"    public abstract {TypeToSource(getter.ReturnType)} {propertyName} {getset}");

            return builder.ToString();
        }

        /// <summary>
        /// Create source code from manifest method
        /// </summary>
        /// <param name="method">Contract method</param>
        /// <param name="nefFile">Nef File</param>
        /// <param name="debugInfo">Debug info</param>
        /// <param name="traceRemarks">Trace remarks</param>
        /// <returns>Source</returns>
        private static string CreateSourceMethodFromManifest(ContractMethodDescriptor method, NefFile? nefFile = null, JToken? debugInfo = null, bool traceRemarks = false)
        {
            var methodName = TongleLowercase(EscapeName(method.Name));

            var builder = new StringBuilder();
            using var sourceCode = new StringWriter(builder)
            {
                NewLine = "\n"
            };

            sourceCode.WriteLine($"    /// <summary>");
            sourceCode.WriteLine($"    /// {(method.Safe ? "Safe method" : "Unsafe method")}");
            sourceCode.WriteLine($"    /// </summary>");

            // Add the opcodes
            if (debugInfo != null && nefFile != null)
            {
                var debugMethod = Disassembler.CSharp.Disassembler.GetMethod(method, debugInfo);
                if (debugMethod != null)
                {
                    var (start, end) = Disassembler.CSharp.Disassembler.GetMethodStartEndAddress(debugMethod);
                    var instructions = Disassembler.CSharp.Disassembler.ConvertMethodToInstructions(nefFile, start, end);

                    if (instructions is not null && instructions.Count > 0)
                    {
                        Script script = nefFile.Script;
                        var actualEnd = end + script.GetInstruction(end).Size;
                        var scripts = nefFile.Script[start..actualEnd].ToArray();

                        var maxAddressLength = instructions.Max(instruction => instruction.address.ToString("X").Length);
                        var addressFormat = $"X{(maxAddressLength + 1) / 2 * 2}";

                        sourceCode.WriteLine("    /// <remarks>");
                        sourceCode.WriteLine($"    /// Script: {Convert.ToBase64String(scripts)}");

                        var lastExtraComments = string.Empty;
                        foreach (var instruction in instructions)
                        {
                            var extraComments = string.Empty;

                            if (traceRemarks)
                            {
                                if (debugMethod["sequence-points-v2"] is JObject sequencePointsV2 &&
                                    sequencePointsV2[instruction.offset.ToString()] is JObject sequenceV2)
                                {
                                    extraComments = BuildExtraInformation(sequenceV2);
                                }

                                if (lastExtraComments != extraComments)
                                {
                                    lastExtraComments = extraComments;
                                }
                                else
                                {
                                    extraComments = string.Empty;
                                }
                            }

                            sourceCode.WriteLine($"    /// {instruction.instruction.InstructionToString()}" + extraComments);
                        }
                        sourceCode.WriteLine("    /// </remarks>");
                    }
                }
            }

            if (method.Name != methodName)
            {
                sourceCode.WriteLine($"    [DisplayName(\"{method.Name}\")]");
            }
            sourceCode.Write($"    public abstract {TypeToSource(method.ReturnType)} {methodName}(");

            var isFirst = true;
            for (int x = 0; x < method.Parameters.Length; x++)
            {
                if (!isFirst) sourceCode.Write(", ");
                else isFirst = false;

                var isLast = x == method.Parameters.Length - 1;
                var arg = method.Parameters[x];

                if (isLast && arg.Type == ContractParameterType.Any)
                {
                    // it will be `object X`, we can add a default value

                    sourceCode.Write($"{TypeToSource(arg.Type)} {EscapeName(arg.Name)} = null");
                }
                else
                {
                    sourceCode.Write($"{TypeToSource(arg.Type)} {EscapeName(arg.Name)}");
                }
            }

            sourceCode.WriteLine(");");

            return builder.ToString();
        }

        private static string BuildExtraInformation(JObject sequenceV2)
        {
            var builder = new StringBuilder();
            using var sourceCode = new StringWriter(builder)
            {
                NewLine = "\n"
            };

            builder.Append($" [optimization={sequenceV2["optimization"]?.AsString()},compiler=");

            if (sequenceV2["compiler"] is JObject compilerObj)
            {
                builder.Append($"{compilerObj["file"]?.AsString()}:{compilerObj["line"]}({compilerObj["method"]?.AsString()})");
            }
            else if (sequenceV2["compiler"] is JArray compilerArray)
            {
                var first = true;

                foreach (var entry in compilerArray)
                {
                    if (entry == null) continue;
                    if (first) first = false;
                    else builder.Append(',');
                    builder.Append($"{entry["file"]?.AsString()}:{entry["line"]}({entry["method"]?.AsString()})");
                }
            }

            builder.Append(']');

            return builder.ToString();
        }

        private static string TongleLowercase(string value)
        {
            if (value.Length == 0)
            {
                return value;
            }

            if (char.IsLower(value[0]))
            {
                return value[0].ToString().ToUpperInvariant() + value[1..];
            }

            return value;
        }

        /// <summary>
        /// Escape name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Escaped name</returns>
        private static string EscapeName(string name)
        {
            if (_protectedWords.Contains(name))
                return "@" + name;

            return name;
        }

        /// <summary>
        /// Type to source
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>c# type</returns>
        private static string TypeToSource(ContractParameterType type)
        {
            return type switch
            {
                ContractParameterType.Boolean => "bool?",
                ContractParameterType.Integer => "BigInteger?",
                ContractParameterType.String => "string?",
                ContractParameterType.Hash160 => "UInt160?",
                ContractParameterType.Hash256 => "UInt256?",
                ContractParameterType.PublicKey => "ECPoint?",
                ContractParameterType.ByteArray => "byte[]?",
                ContractParameterType.Signature => "byte[]?",
                ContractParameterType.Array => "IList<object>?",
                ContractParameterType.Map => "IDictionary<object, object>?",
                ContractParameterType.Void => "void",
                _ => "object?",
            };
        }

        private static string FormatInheritance(List<Type> inheritance, string initializeParam)
        {
            var formattedInheritance = inheritance.Select(type => (type == typeof(SmartContract)
                    ? $"{type.FullName}({initializeParam})"
                    : type.FullName)!)
                .ToList();
            return string.Join(", ", formattedInheritance);
        }
    }
}

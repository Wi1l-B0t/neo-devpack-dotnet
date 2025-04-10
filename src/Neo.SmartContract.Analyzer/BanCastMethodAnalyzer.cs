// Copyright (C) 2015-2025 The Neo Project.
//
// BanCastMethodAnalyzer.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Neo.SmartContract.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BanCastMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NC4017";
        private const string Title = "Do not use .Cast<T>() method";
        private const string MessageFormat = "Use of .Cast<T>() method is not allowed";
        private const string Description = "Replace .Cast<T>() with direct type casting.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            if (invocationExpr.Expression is MemberAccessExpressionSyntax memberAccessExpr &&
                memberAccessExpr.Name.Identifier.ValueText == "Cast" &&
                memberAccessExpr.Expression != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccessExpr.Name.GetLocation()));
            }
        }
    }
}

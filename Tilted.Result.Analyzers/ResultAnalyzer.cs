using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tilted.Result.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ResultAnalyzer : DiagnosticAnalyzer
{
    private const string EqualTypesId = "TRESULT001";
    private const string OverlappingTypesId = "TRESULT002";

    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor EqualRule = new(
        EqualTypesId,
        "Result type arguments must be distinct",
        "Result<TValue, TError> requires TValue and TError to be distinct types; '{0}' is used for both",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The union discriminates its cases by runtime type, so identical type arguments make success and error indistinguishable.");

    private static readonly DiagnosticDescriptor OverlapRule = new(
        OverlappingTypesId,
        "Result type arguments must not overlap",
        "Result<TValue, TError> requires TValue and TError to be non-overlapping types; '{0}' is convertible to '{1}'",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The union discriminates its cases by runtime type, so overlapping type arguments make success and error ambiguous.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(EqualRule, OverlapRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
    }

    private static void AnalyzeGenericName(SyntaxNodeAnalysisContext context)
    {
        var genericName = (GenericNameSyntax)context.Node;
        if (genericName.TypeArgumentList.Arguments.Count != 2)
        {
            return;
        }

        if (genericName.FirstAncestorOrSelf<CrefSyntax>() is not null)
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(genericName).Symbol;
        if (!IsTiltedResultSymbol(symbol))
        {
            return;
        }

        var t1 = context.SemanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[0]).Type;
        var t2 = context.SemanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[1]).Type;
        if (t1 is null || t2 is null)
        {
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(t1, t2))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                EqualRule,
                genericName.TypeArgumentList.GetLocation(),
                t1.ToDisplayString()));
            return;
        }

        if (t1 is ITypeParameterSymbol || t2 is ITypeParameterSymbol)
        {
            return;
        }

        if (IsImplicitReferenceConversion(context.Compilation, t1, t2))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                OverlapRule,
                genericName.TypeArgumentList.GetLocation(),
                t1.ToDisplayString(),
                t2.ToDisplayString()));
        }
        else if (IsImplicitReferenceConversion(context.Compilation, t2, t1))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                OverlapRule,
                genericName.TypeArgumentList.GetLocation(),
                t2.ToDisplayString(),
                t1.ToDisplayString()));
        }
    }

    private static bool IsTiltedResultSymbol(ISymbol? symbol)
    {
        return symbol switch
        {
            INamedTypeSymbol { Arity: 2 } named =>
                named.Name == "Result" && named.ContainingNamespace.ToDisplayString() == "Tilted.Result",
            IMethodSymbol { Arity: 2, ContainingType: { } container } method =>
                method.Name is "Ok" or "Fail" or "Try" or "TryAsync"
                && container.Name == "Result"
                && container.ContainingNamespace.ToDisplayString() == "Tilted.Result",
            _ => false
        };
    }

    private static bool IsImplicitReferenceConversion(Compilation compilation, ITypeSymbol from, ITypeSymbol to)
    {
        try
        {
            var conversion = compilation.ClassifyConversion(from, to);
            return conversion is { IsImplicit: true, IsReference: true, IsIdentity: false };
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}

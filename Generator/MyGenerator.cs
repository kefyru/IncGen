using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator;

[Generator]
public class MyGenerator : IIncrementalGenerator
{
  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    Logger.WriteLine($"{DateTime.Now:T} {Environment.CommandLine}");
    var values = context
                .SyntaxProvider
                .CreateSyntaxProvider(
                   (sn, ct) => sn is AttributeSyntax a && a.Name.ToString() == "My",
                   (c, ct) =>
                   {
                     if (c.Node.Parent?.Parent is not InterfaceDeclarationSyntax targetNode) return null;
                     var symbol = c.SemanticModel.GetDeclaredSymbol(targetNode);
                     if (symbol is not INamedTypeSymbol targetSymbol) return null;
                     return targetSymbol;
                   })
                .Where(t => t is not null).Select((t, ct) => t!).WithComparer(SymbolEqualityComparer.Default);

    context.RegisterImplementationSourceOutput(values, (spc, target) =>
    {
      Logger.WriteLine($"{DateTime.Now:T} {target} changed");
      spc.AddSource(
        target.Name + ".g.cs",
        $"namespace Generated {{ public class {target.Name}_Impl : {target} {{}} }}"
      );
    });
  }
}
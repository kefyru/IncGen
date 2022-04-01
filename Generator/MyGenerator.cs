using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generator;

public readonly struct MySymbolData : IEquatable<MySymbolData>
{
  public static readonly IEqualityComparer<MySymbolData> Comparer = EqualityComparer<MySymbolData>.Default;
  public readonly bool IsDefault = true;
  public readonly string Name;
  public readonly string FullName;
  public readonly IEnumerable<string> Members;
  public MySymbolData(INamedTypeSymbol symbol)
  {
    IsDefault = false;
    FullName = symbol.ToString();
    Name = symbol.Name.ToString();
    Members = symbol.MemberNames.ToArray();
  }
  public bool Equals(MySymbolData other) =>
    FullName == other.FullName
    && Name == other.Name
    && Members.SequenceEqual(other.Members);

  public override bool Equals(object? obj) => obj is MySymbolData other && Equals(other);
  public override int GetHashCode()
  {
    if (IsDefault) return 0;
    unchecked {
      var hashCode = Name.GetHashCode();
      hashCode = (hashCode * 397) ^ FullName.GetHashCode();
      hashCode = (hashCode * 397) ^ Members.Aggregate(0, (a, it) => a * 397 ^ it.GetHashCode());
      return hashCode;
    }
  }
}

[Generator]
public class MyGenerator : IIncrementalGenerator
{
  private ConcurrentDictionary<string, string> _myCache = new();

  public void Initialize(IncrementalGeneratorInitializationContext context)
  {
    Logger.WriteLine($"{DateTime.Now:T} {Environment.CommandLine}");

    var values = context
                .SyntaxProvider
                .CreateSyntaxProvider(
                   static (sn, ct) =>
                   {
                     ct.ThrowIfCancellationRequested();
                     return sn is AttributeSyntax a && a.Name.ToString() == "My";
                   },
                   static (c, ct) =>
                   {
                     ct.ThrowIfCancellationRequested();
                     if (c.Node.Parent?.Parent is not InterfaceDeclarationSyntax targetNode) return default;
                     var symbol = c.SemanticModel.GetDeclaredSymbol(targetNode);
                     if (symbol is not INamedTypeSymbol targetSymbol) return default;
                     return new MySymbolData(targetSymbol); 
                   })
                .WithComparer(MySymbolData.Comparer) // !!!!!!! 
                .Where(static t => !t.IsDefault);

    context.RegisterImplementationSourceOutput(values, Generator);
  }
  private static void Generator(SourceProductionContext spc, MySymbolData target)
  {
    var ct = spc.CancellationToken;
    ct.ThrowIfCancellationRequested();

    var b = new StringBuilder();
    b.AppendLine("namespace Generated {");
    b.AppendLine($"   public interface {target.Name}_Generated: {target.FullName} {{");
    foreach (var member in target.Members)
      b.AppendLine($"      // {member} implementation ...");
    b.AppendLine("   }"); // class
    b.AppendLine("}"); // namespace

    spc.AddSource($"{target.Name}.g.cs", b.ToString());
    Logger.WriteLine($"{DateTime.Now:T} {target.Name}");

  }
}
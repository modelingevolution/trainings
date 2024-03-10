using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class AggregateSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this example
            //System.Diagnostics.Debugger.Launch();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees)
            {
                var usingDirectives = syntaxTree.GetRoot().DescendantNodes()
                    .OfType<UsingDirectiveSyntax>()
                    .Select(u => u.ToString())
                    .Distinct()
                    .ToList();
                var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
                var classDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classDecl in classDeclarations)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
                    if (classSymbol == null) continue;

                    if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.Name == "AggregateAttribute" || ad.AttributeClass.Name == "Aggregate"))
                    {
                        var namespaceDecl = classDecl.Parent as NamespaceDeclarationSyntax;
                        var fileScopedNamespace = classDecl.SyntaxTree.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
                        var namespaceName = namespaceDecl?.Name.ToString() ?? fileScopedNamespace?.Name.ToString() ?? string.Empty;
                        var className = classDecl.Identifier.ValueText;
                        var methods = classDecl.Members.OfType<MethodDeclarationSyntax>()
                            .Where(m => m.Identifier.ValueText == "Given" &&
                                        m.ParameterList.Parameters.Count == 2 &&
                                        m.Modifiers.Any(SyntaxKind.PrivateKeyword))
                            .ToList();

                        var sb = new StringBuilder();

                        // Add using directives
                        foreach (var usingDirective in usingDirectives)
                        {
                            sb.AppendLine(usingDirective);
                        }
                        sb.AppendLine(); // Add a line break after using directives

                        if (!string.IsNullOrEmpty(namespaceName))
                        {
                            sb.AppendLine($"namespace {namespaceName};");
                        }

                        sb.AppendLine($"partial class {className} : IAggregate<{className}> ");
                        sb.AppendLine("{");
                        sb.AppendLine("    protected override void Apply(object ev)");
                        sb.AppendLine("    {");
                        sb.AppendLine("        _state = ev switch");
                        sb.AppendLine("        {");

                        foreach (var method in methods)
                        {
                            var eventType = method.ParameterList.Parameters[1].Type.ToString();
                            sb.AppendLine($"            {eventType} e => Given(_state, e),");
                        }

                        sb.AppendLine("            _ => _state");
                        
                        sb.AppendLine("        };");
                        sb.AppendLine("    }");

                        sb.AppendLine($"   public static {className} New(Guid id) => new {className}(id);");

                        sb.AppendLine("    private static readonly Dictionary<string, Type> _register = new()");
                        sb.AppendLine("    {");

                        foreach (var method in methods)
                        {
                            var eventType = method.ParameterList.Parameters[1].Type.ToString();
                            sb.AppendLine($"        {{ nameof({eventType}), typeof({eventType}) }}, ");
                        }
                        sb.AppendLine("    };");

                        sb.AppendLine($"    static IDictionary<string, Type> IAggregate<{className}>.TypeRegister => _register;");

                        sb.AppendLine("}");
                        context.AddSource($"{className}_Aggregate.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
                    }
                }
            }
        }
    }
}
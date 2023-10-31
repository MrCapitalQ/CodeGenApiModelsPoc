using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace ApiModelGenerator
{
    [Generator]
    public class SourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.ForAttributeWithMetadataName("PocAttributes.ModelGenerationAttribute",
                IsMatch,
                Transform);

            context.RegisterSourceOutput(provider, Generate);
        }
        private static bool IsMatch(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            return syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax;
        }

        private static INamedTypeSymbol Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            return (INamedTypeSymbol)context.TargetSymbol;
        }

        private static void Generate(SourceProductionContext context, INamedTypeSymbol typeSymbol)
        {
            var typeNamespace = typeSymbol.ContainingNamespace.ToDisplayString();

            var properties = typeSymbol
                .GetMembers()
                .Where(s => s.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .ToList();

            var propertiesSb = new StringBuilder();
            foreach (var property in properties)
            {
                propertiesSb.AppendLine($$"""        public {{property.Type.Name}} {{property.Name}} { get; set; }""");
            }

            var classTemplate = $$"""
                namespace {{typeNamespace}}
                {
                    public class {{typeSymbol.Name}}Post
                    {
                {{propertiesSb}}
                    }
                }
                """;
            context.AddSource($"{typeSymbol.Name}_ApiModels.g.cs", classTemplate);
        }
    }
}

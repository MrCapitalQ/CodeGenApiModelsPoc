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

        private static ClassInfo Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            var typeSymbol = (INamedTypeSymbol)context.TargetSymbol;

            return new ClassInfo
            {
                Name = typeSymbol.Name,
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                Properties = typeSymbol
                    .GetMembers()
                    .Where(s => s.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .Where(p => !p.GetAttributes().Any(x => "PocAttributes.PostIgnoreAttribute".Equals(x.AttributeClass?.ToDisplayString())))
                    .Select(p => new ClassPropertyInfo
                    {
                        Name = p.Name,
                        Type = p.Type.ToDisplayString()
                    })
                    .ToList()
            };
        }

        private static void Generate(SourceProductionContext context, ClassInfo classInfo)
        {
            var propertiesSb = new StringBuilder();
            foreach (var property in classInfo.Properties)
            {
                propertiesSb.AppendLine($$"""        public {{property.Type}} {{property.Name}} { get; set; }""");
            }

            var classTemplate = $$"""
                namespace {{classInfo.Namespace}}
                {
                    public class {{classInfo.Name}}Post
                    {
                {{propertiesSb}}
                    }
                }
                """;
            context.AddSource($"{classInfo.Name}_ApiModels.g.cs", classTemplate);
        }

        private record ClassInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Namespace { get; set; } = string.Empty;
            public IEnumerable<ClassPropertyInfo> Properties { get; set; } = Enumerable.Empty<ClassPropertyInfo>();
        }

        private record ClassPropertyInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
        }
    }
}

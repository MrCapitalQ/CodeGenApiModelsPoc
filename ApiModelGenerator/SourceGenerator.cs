using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Xml;

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
                        Type = p.Type.ToDisplayString(),
                        Comments = p.GetDocumentationCommentXml(cancellationToken: cancellationToken),
                        Attributes = p.GetAttributes()
                    })
                    .ToList()
            };
        }

        private static void Generate(SourceProductionContext context, ClassInfo classInfo)
        {
            var propertiesSb = new StringBuilder();
            foreach (var property in classInfo.Properties)
            {
                if (!string.IsNullOrEmpty(property.Comments))
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(property.Comments);
                    var defaultSummary = doc.GetElementsByTagName("summary").OfType<XmlNode>().FirstOrDefault();
                    var postSummary = doc.GetElementsByTagName("post").OfType<XmlNode>().FirstOrDefault();
                    AppendSummaryComment(propertiesSb, postSummary ?? defaultSummary);
                }

                foreach (var attribute in property.Attributes)
                {
                    propertiesSb.Append($"        [{attribute.AttributeClass?.ToDisplayString()}(");

                    var arguments = attribute.ConstructorArguments
                        .Select(x => x.ToCSharpString())
                        .Concat(attribute.NamedArguments
                            .Select(x => $"{x.Key} = {x.Value.ToCSharpString()}"));
                    propertiesSb.Append(string.Join(", ", arguments));

                    propertiesSb.AppendLine(")]");
                }

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

        private static void AppendSummaryComment(StringBuilder sb, XmlNode? node)
        {
            if (node is null)
                return;

            sb.AppendLine("        /// <summary>");
            foreach (var line in node.InnerXml.TrimEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.Append("        /// ");
                sb.AppendLine(string.Concat(line.Skip(4)));
            }
            sb.AppendLine("        /// </summary>");
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
            public string? Comments { get; set; }
            public IEnumerable<AttributeData> Attributes { get; set; } = Enumerable.Empty<AttributeData>();
        }
    }
}

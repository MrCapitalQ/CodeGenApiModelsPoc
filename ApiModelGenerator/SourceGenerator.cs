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
        private const string PostIgnoreAttributeName = "PocAttributes.PostIgnoreAttribute";
        private const string PutIgnoreAttributeName = "PocAttributes.PutIgnoreAttribute";

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
                IsRecord = typeSymbol.IsRecord,
                Namespace = typeSymbol.ContainingNamespace.ToDisplayString(),
                Properties = typeSymbol
                    .GetMembers()
                    .Where(s => s.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .Select(p => new ClassPropertyInfo
                    {
                        Name = p.Name,
                        Type = p.Type.ToDisplayString(),
                        NullableAnnotation = p.NullableAnnotation,
                        IsReferenceType = p.Type.IsReferenceType,
                        Comments = p.GetDocumentationCommentXml(cancellationToken: cancellationToken),
                        Attributes = p.GetAttributes()
                    })
                    .ToList()
            };
        }

        private static void Generate(SourceProductionContext context, ClassInfo classInfo)
        {
            var postPropertiesSb = new StringBuilder();
            var putPropertiesSb = new StringBuilder();
            var isNullableEnabled = classInfo.Properties.Any(x => x.NullableAnnotation != NullableAnnotation.None && x.IsReferenceType);
            foreach (var property in classInfo.Properties)
            {
                if (classInfo.IsRecord && property.Name == "EqualityContract")
                    continue;

                var isPostIgnored = property.Attributes.Any(x => PostIgnoreAttributeName.Equals(x.AttributeClass?.ToDisplayString()));
                var isPutIgnored = property.Attributes.Any(x => PutIgnoreAttributeName.Equals(x.AttributeClass?.ToDisplayString()));

                if (!string.IsNullOrEmpty(property.Comments))
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(property.Comments);
                    var defaultSummary = doc.GetElementsByTagName("summary").OfType<XmlNode>().FirstOrDefault();

                    if (!isPostIgnored)
                    {
                        var postSummary = doc.GetElementsByTagName("post").OfType<XmlNode>().FirstOrDefault();
                        AppendSummaryComment(postPropertiesSb, postSummary ?? defaultSummary);
                    }

                    if (!isPutIgnored)
                    {
                        var putSummary = doc.GetElementsByTagName("put").OfType<XmlNode>().FirstOrDefault();
                        AppendSummaryComment(putPropertiesSb, putSummary ?? defaultSummary);
                    }
                }

                foreach (var attribute in property.Attributes)
                {
                    if (!isPostIgnored)
                        AppendAttribute(postPropertiesSb, attribute);

                    if (!isPutIgnored)
                        AppendAttribute(putPropertiesSb, attribute);
                }

                if (!isPostIgnored)
                    AppendProperty(postPropertiesSb, isNullableEnabled, property);

                if (!isPutIgnored)
                    AppendProperty(putPropertiesSb, isNullableEnabled, property);
            }

            var fileTemplate = $$"""
                {{(isNullableEnabled ? "#nullable enable" : string.Empty)}}
                namespace {{classInfo.Namespace}}
                {
                    public {{(classInfo.IsRecord ? "record" : "class")}} {{classInfo.Name}}Post
                    {
                {{postPropertiesSb}}
                    }
                
                    public {{(classInfo.IsRecord ? "record" : "class")}} {{classInfo.Name}}Put
                    {
                {{putPropertiesSb}}
                    }
                }
                """;
            context.AddSource($"{classInfo.Name}_ApiModels.g.cs", fileTemplate);
        }

        private static void AppendProperty(StringBuilder sb, bool isNullableEnabled, ClassPropertyInfo property)
        {
            sb.Append("        public ");
            sb.Append(property.Type);

            if (isNullableEnabled && property.NullableAnnotation != NullableAnnotation.Annotated)
                sb.Append("?");

            sb.Append(" ");
            sb.Append(property.Name);
            sb.AppendLine(" { get; set; }");
        }

        private static void AppendAttribute(StringBuilder sb, AttributeData attribute)
        {
            if (attribute.AttributeClass is null
                || PostIgnoreAttributeName.Equals(attribute.AttributeClass.ToDisplayString())
                || PutIgnoreAttributeName.Equals(attribute.AttributeClass.ToDisplayString()))
                return;

            sb.Append("        [");
            sb.Append(attribute.AttributeClass.ToDisplayString());
            sb.Append("(");

            var argCount = 0;
            foreach (var arg in attribute.ConstructorArguments)
            {
                if (argCount > 0)
                    sb.Append(", ");
                sb.Append(arg.ToCSharpString());
                argCount++;
            }

            foreach (var arg in attribute.NamedArguments)
            {
                if (argCount > 0)
                    sb.Append(", ");
                sb.Append(arg.Key);
                sb.Append(" = ");
                sb.Append(arg.Value.ToCSharpString());
                argCount++;
            }

            sb.AppendLine(")]");
        }

        private static void AppendSummaryComment(StringBuilder sb, XmlNode? node)
        {
            if (node is null)
                return;

            sb.AppendLine("        /// <summary>");

            var xmlDocs = $"    {node.InnerXml.Trim()}";
            foreach (var line in xmlDocs.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.Append("        /// ");
                sb.AppendLine(string.Concat(line.Skip(4)));
            }

            sb.AppendLine("        /// </summary>");
        }

        private record ClassInfo
        {
            public string Name { get; set; } = string.Empty;
            public bool IsRecord { get; set; }
            public string Namespace { get; set; } = string.Empty;
            public IEnumerable<ClassPropertyInfo> Properties { get; set; } = Enumerable.Empty<ClassPropertyInfo>();
        }

        private record ClassPropertyInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string? Comments { get; set; }
            public IEnumerable<AttributeData> Attributes { get; set; } = Enumerable.Empty<AttributeData>();
            public NullableAnnotation NullableAnnotation { get; set; }
            public bool IsReferenceType { get; set; }
        }
    }
}

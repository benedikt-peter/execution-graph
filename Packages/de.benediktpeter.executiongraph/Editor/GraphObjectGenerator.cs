using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExecutionGraph.Editor
{
  public class GraphObjectGenerator
  {
    private readonly BaseGraph _graph;

    private readonly List<NodeTypeMetadata> _nodes = new();

    private ClassDeclarationSyntax _scriptableObject;
    private readonly string _className;

    public GraphObjectGenerator(BaseGraph graph)
    {
      _graph = graph;
      _className = $"Graph_{_graph.name}_Impl";

      var types = new Dictionary<Type, NodeTypeMetadata>();
      foreach (var node in _graph.Nodes)
      {
        if (!types.TryGetValue(node.ImplType, out var metadata))
        {
          types[node.ImplType] = metadata = new NodeTypeMetadata(node.ImplType);
        }

        _nodes.Add(metadata);
      }

      _scriptableObject = ClassDeclaration(_className);
    }

    public void WriteToDisk()
    {
      CreateNodeInstances();

      var folder = "Assets/Generated";

      Directory.CreateDirectory(folder);
      folder = $"{folder}/{_className}.cs";

      const string classNamespace = "Internal.Generated";

      var compilationUnit = CompilationUnit()
        .AddMembers(
          NamespaceDeclaration(IdentifierName(classNamespace))
            .WithLeadingTrivia(Comment("//ReSharper disable All"))
            .AddMembers(_scriptableObject)
        );
      File.WriteAllText(folder, compilationUnit.NormalizeWhitespace().ToFullString());
    }

    private void CreateNodeInstances()
    {
      _scriptableObject = _scriptableObject
        .AddMembers(
          _nodes.Select((node, index) =>
              FieldDeclaration(
                  VariableDeclaration(
                      IdentifierName(node.Type.FullName!))
                    .WithVariables(
                      SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier($"_node{index}{node.Type.Name}"))
                          .WithInitializer(
                            EqualsValueClause(
                              ImplicitObjectCreationExpression()
                                .WithArgumentList(
                                  ArgumentList(
                                    SeparatedList<ArgumentSyntax>(
                                      CreateNodeConstructorArgs(index)
                                    ))))))))
                .WithModifiers(
                  TokenList(
                    Token(SyntaxKind.PrivateKeyword))))
            .ToArray<MemberDeclarationSyntax>()
        );
    }

    private IEnumerable<SyntaxNodeOrToken> CreateNodeConstructorArgs(int index)
    {
      var fieldToConstructorArg = _nodes[index].CreatePropertyConstructorArgs();

      var arguments = new List<SyntaxNodeOrToken>();
      var first = true;
      foreach (var field in fieldToConstructorArg)
      {
        if (first)
        {
          first = false;
        }
        else
        {
          arguments.Add(Token(SyntaxKind.CommaToken));
        }

        var argument = field.Item2.FieldType switch
        {
          NodeTypeMetadata.FieldType.Property => Argument(IdentifierName(field.Item2.UnityFieldName)),
          NodeTypeMetadata.FieldType.Continuation => Argument(ObjectCreationExpression(
              IdentifierName("ExecutionGraph.Continuation"))
            .WithArgumentList(
              ArgumentList(
                SingletonSeparatedList(
                  Argument(
                    LiteralExpression(
                      SyntaxKind.NumericLiteralExpression,
                      Literal(1))))))
            .WithArgumentList(
              ArgumentList(
                SingletonSeparatedList(
                  Argument(
                    LiteralExpression(
                      SyntaxKind.StringLiteralExpression,
                      Literal(field.Item2.PropertyName))))))),
          _ => throw new ArgumentOutOfRangeException()
        };

        arguments.Add(argument);
      }

      return arguments.ToArray();
    }
  }
}
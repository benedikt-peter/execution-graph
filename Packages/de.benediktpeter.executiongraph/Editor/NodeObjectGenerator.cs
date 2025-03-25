using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExecutionGraph.UnityObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExecutionGraph.Editor
{
  internal class NodeObjectGenerator
  {
    private const string FallbackFolder = "Assets/Generated";

    private readonly NodeTypeMetadata _metadata;
    private ClassDeclarationSyntax _scriptableObject;

    public NodeObjectGenerator(Type type)
    {
      _metadata = new NodeTypeMetadata(type);

      _scriptableObject =
        ClassDeclaration(
          default,
          new SyntaxTokenList(Token(SyntaxKind.PublicKeyword)),
          Identifier(_metadata.Type.Name),
          null,
          parameterList: null,
          BaseList(Token(SyntaxKind.ColonToken),
            SeparatedList<BaseTypeSyntax>()
              .Add(SimpleBaseType(
                ParseTypeName(typeof(NodeObject).FullName!)))),
          default,
          default);
    }

    public IReadOnlyCollection<string> Validate()
    {
      return _metadata.Validate();
    }

    public void WriteToDisk()
    {
      AddUnityPropertyFields();
      AddPublicProperties();
      AddInputsProperty();
      AddOutputsProperty();
      AddContinuationsProperty();
      AddNodeImplementationType();
      AddInstantiation();
      AddExecution();

      var folder = $"{_metadata.GetAssemblyRoot() ?? FallbackFolder}/Generated";

      Directory.CreateDirectory(folder);

      var path = _metadata.Type.Namespace != null
        ? $"{folder}/{_metadata.Type.Namespace.Replace('.', '/')}"
        : folder;
      Directory.CreateDirectory(path);
      path = $"{path}/{_metadata.Type.Name}.cs";

      var classNamespace = _metadata.Type.Namespace != null
        ? $"Internal.Generated{_metadata.Type.Namespace.Replace(".", "")}"
        : "Internal.Generated";

      var compilationUnit = CompilationUnit()
        .AddMembers(
          NamespaceDeclaration(IdentifierName(classNamespace))
            .WithLeadingTrivia(Comment("//ReSharper disable All"))
            .AddMembers(_scriptableObject)
        );
      File.WriteAllText(path, compilationUnit.NormalizeWhitespace().ToFullString());
    }

    private void AddNodeImplementationType()
    {
      _scriptableObject = _scriptableObject.AddMembers(
        PropertyDeclaration(ParseTypeName(typeof(Type).FullName!), "ImplType")
          .AddModifiers(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.SealedKeyword),
            Token(SyntaxKind.OverrideKeyword)
          )
          .AddAccessorListAccessors(
            AccessorDeclaration(
              SyntaxKind.GetAccessorDeclaration,
              Block(
                List(new[]
                {
                  ReturnStatement(TypeOfExpression(
                    IdentifierName(_metadata.Type.FullName!)))
                })
              )
            ))
      );
    }

    private void AddPublicProperties()
    {
      _scriptableObject = _scriptableObject.AddMembers(
        _metadata.Properties.Select(p =>
          PropertyDeclaration(ParseTypeName(p.Type.FullName!), p.PropertyName)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
              AccessorDeclaration(
                SyntaxKind.GetAccessorDeclaration,
                Block(
                  List(new[]
                  {
                    ReturnStatement(IdentifierName(p.UnityFieldName))
                  })
                )
              ))
        ).ToArray<MemberDeclarationSyntax>()
      );
    }

    private void AddUnityPropertyFields()
    {
      _scriptableObject = _scriptableObject.AddMembers(
        _metadata.Inputs.Concat(_metadata.Properties).Select(p =>
          FieldDeclaration(
              default,
              new SyntaxTokenList(Token(SyntaxKind.PrivateKeyword)),
              VariableDeclaration(
                ParseTypeName(p.Type.FullName!),
                SeparatedList(new[]
                {
                  VariableDeclarator(Identifier(p.UnityFieldName), null, p.DefaultValue)
                })
              ))
            .AddAttributeLists(AttributeList(
              SingletonSeparatedList(
                Attribute(IdentifierName(typeof(SerializeField).FullName!),
                  null))))
        ).ToArray<MemberDeclarationSyntax>()
      );
    }

    private void AddInputsProperty()
    {
      _scriptableObject = _scriptableObject.AddMembers(
        CreateInputsProperty()
      );
    }

    private void AddOutputsProperty()
    {
      var portType = typeof(Value).FullName!;
      var propertyType = $"System.Collections.Generic.IReadOnlyList<{portType}>";
      _scriptableObject = _scriptableObject
        .AddMembers(
          PropertyDeclaration(ParseTypeName(propertyType), "Outputs")
            .AddModifiers(
              Token(SyntaxKind.PublicKeyword),
              Token(SyntaxKind.SealedKeyword),
              Token(SyntaxKind.OverrideKeyword)
            )
            .AddAccessorListAccessors(
              AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            )
            .WithInitializer(
              EqualsValueClause(
                ArrayCreationExpression(ArrayType(ParseTypeName(portType))
                    .WithRankSpecifiers(
                      SingletonList(
                        ArrayRankSpecifier(
                          SingletonSeparatedList<ExpressionSyntax>(
                            OmittedArraySizeExpression())))))
                  .WithInitializer(
                    InitializerExpression(
                      SyntaxKind.ArrayInitializerExpression,
                      SeparatedList<ExpressionSyntax>(
                        CreateOutputsPorts()
                      ))
                  )))
            .WithSemicolonToken(
              Token(SyntaxKind.SemicolonToken)));
    }

    private void AddContinuationsProperty()
    {
      var portType = typeof(string).FullName!;
      var propertyType = $"System.Collections.Generic.IReadOnlyList<{portType}>";
      _scriptableObject = _scriptableObject
        .AddMembers(
          PropertyDeclaration(ParseTypeName(propertyType), "Continuations")
            .AddModifiers(
              Token(SyntaxKind.PublicKeyword),
              Token(SyntaxKind.SealedKeyword),
              Token(SyntaxKind.OverrideKeyword)
            )
            .AddAccessorListAccessors(
              AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            )
            .WithInitializer(
              EqualsValueClause(
                ArrayCreationExpression(ArrayType(ParseTypeName(portType))
                    .WithRankSpecifiers(
                      SingletonList(
                        ArrayRankSpecifier(
                          SingletonSeparatedList<ExpressionSyntax>(
                            OmittedArraySizeExpression())))))
                  .WithInitializer(
                    InitializerExpression(
                      SyntaxKind.ArrayInitializerExpression,
                      SeparatedList<ExpressionSyntax>(
                        CreateContinuationsPorts()
                      ))
                  )))
            .WithSemicolonToken(
              Token(SyntaxKind.SemicolonToken)));
    }

    private void AddInstantiation()
    {
      _scriptableObject = _scriptableObject
        .AddMembers(
          MethodDeclaration(
              IdentifierName(typeof(object).FullName!), //IdentifierName(_type.FullName!),
              Identifier("Instantiate"))
            .WithModifiers(
              TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
              ParameterList(
                SingletonSeparatedList(
                  Parameter(
                      Identifier("context"))
                    .WithType(
                      IdentifierName("ExecutionGraph.Runners.InstantiationContext")))))
            .WithBody(
              Block(
                SingletonList<StatementSyntax>(
                  ReturnStatement(
                    ObjectCreationExpression(
                        IdentifierName(_metadata.Type.FullName!))
                      .WithArgumentList(
                        ArgumentList(
                          SeparatedList<ArgumentSyntax>(
                            CreatePropertyConstructorArgs()
                          )))))))
            .NormalizeWhitespace()
        );
    }

    private void AddExecution()
    {
      var executionMethod = _metadata.GraphNodeType == NodeType.Action
        ? _metadata.ExecuteMethods[0]
        : _metadata.ComputeMethods[0];

      var outputs = _metadata.Outputs.Select(output =>
        ExpressionStatement(
          InvocationExpression(
              MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("context"),
                IdentifierName("SetOutput")))
            .WithArgumentList(
              ArgumentList(
                SeparatedList<ArgumentSyntax>(
                  new SyntaxNodeOrToken[]
                  {
                    Argument(
                      LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(output.PropertyName))),
                    Token(SyntaxKind.CommaToken),
                    Argument(
                      output.FieldType == NodeTypeMetadata.FieldType.Output
                        ? MemberAccessExpression(
                          SyntaxKind.SimpleMemberAccessExpression,
                          IdentifierName("node"),
                          IdentifierName(output.PropertyName))
                        : IdentifierName("result")
                    )
                  }))))
      ).ToList<StatementSyntax>();

      List<StatementSyntax> body;
      if (executionMethod.ReturnType.IsGenericType &&
          executionMethod.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerator<>))
      {
        outputs.Add(YieldStatement(
          SyntaxKind.YieldReturnStatement,
          MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName("result"),
            IdentifierName("Current"))));
        body = new List<StatementSyntax>
        {
          LocalDeclarationStatement(
            VariableDeclaration(IdentifierName(_metadata.Type.FullName!))
              .WithVariables(
                SingletonSeparatedList(
                  VariableDeclarator(
                      Identifier("node"))
                    .WithInitializer(
                      EqualsValueClause(
                        CastExpression(
                          IdentifierName(_metadata.Type.FullName!),
                          IdentifierName("instance"))))))),
          LocalDeclarationStatement(
            VariableDeclaration(QualifiedName(
                QualifiedName(
                  QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Collections")),
                  IdentifierName("Generic")),
                GenericName(
                    Identifier("IEnumerator"))
                  .WithTypeArgumentList(
                    TypeArgumentList(
                      SingletonSeparatedList<TypeSyntax>(
                        IdentifierName(typeof(Continuation).FullName!))))))
              .WithVariables(
                SingletonSeparatedList(
                  VariableDeclarator(
                      Identifier("result"))
                    .WithInitializer(
                      EqualsValueClause(
                        InvocationExpression(
                            MemberAccessExpression(
                              SyntaxKind.SimpleMemberAccessExpression,
                              IdentifierName("node"),
                              IdentifierName(executionMethod.Name)))
                          .WithArgumentList(
                            ArgumentList(
                              SeparatedList(
                                _metadata.Inputs.Select(i => Argument(
                                  InvocationExpression(
                                      MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("context"),
                                        IdentifierName("GetInput")))
                                    .WithArgumentList(
                                      ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                          new SyntaxNodeOrToken[]
                                          {
                                            Argument(
                                              LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(i.PropertyName))),
                                            Token(SyntaxKind.CommaToken),
                                            Argument(
                                              IdentifierName(i.UnityFieldName))
                                          })))))
                              )))))))),
          LocalFunctionStatement(
              QualifiedName(
                QualifiedName(
                  QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Collections")),
                  IdentifierName("Generic")),
                GenericName(
                    Identifier("IEnumerator"))
                  .WithTypeArgumentList(
                    TypeArgumentList(
                      SingletonSeparatedList<TypeSyntax>(
                        IdentifierName(typeof(Continuation).FullName!))))),
              Identifier("Iterate"))
            .WithBody(
              Block(
                SingletonList<StatementSyntax>(
                  WhileStatement(
                    InvocationExpression(
                      MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("result"),
                        IdentifierName("MoveNext"))),
                    Block(new SyntaxList<StatementSyntax>(outputs)))))),
          ReturnStatement(
            InvocationExpression(
              IdentifierName("Iterate")))
        };
      }
      else
      {
        body = new List<StatementSyntax>
        {
          LocalDeclarationStatement(
            VariableDeclaration(
                IdentifierName(_metadata.Type.FullName!)
              )
              .WithVariables(
                SingletonSeparatedList(
                  VariableDeclarator(
                      Identifier("node"))
                    .WithInitializer(
                      EqualsValueClause(
                        CastExpression(
                          IdentifierName(_metadata.Type.FullName!),
                          IdentifierName("instance"))))))),
          LocalDeclarationStatement(
            VariableDeclaration(
                QualifiedName(
                  IdentifierName("System"),
                  IdentifierName("Object")))
              .WithVariables(
                SingletonSeparatedList(
                  VariableDeclarator(
                      Identifier("result"))
                    .WithInitializer(
                      EqualsValueClause(
                        InvocationExpression(
                            MemberAccessExpression(
                              SyntaxKind.SimpleMemberAccessExpression,
                              IdentifierName("node"),
                              IdentifierName(executionMethod.Name)))
                          .WithArgumentList(
                            ArgumentList(
                              SeparatedList(
                                _metadata.Inputs.Select(i => Argument(
                                  InvocationExpression(
                                      MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("context"),
                                        IdentifierName("GetInput")))
                                    .WithArgumentList(
                                      ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                          new SyntaxNodeOrToken[]
                                          {
                                            Argument(
                                              LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal(i.PropertyName))),
                                            Token(SyntaxKind.CommaToken),
                                            Argument(
                                              IdentifierName(i.UnityFieldName))
                                          })))))
                              )))
                      ))))),
        };

        body.AddRange(outputs);
        body.Add(ReturnStatement(
          IdentifierName("result")));
      }

      _scriptableObject = _scriptableObject
        .AddMembers(
          MethodDeclaration(
              IdentifierName(typeof(object).FullName!),
              Identifier("Execute"))
            .WithModifiers(
              TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
            .WithParameterList(
              ParameterList(
                SeparatedList(
                  new[]
                  {
                    Parameter(
                        Identifier("instance"))
                      .WithType(
                        IdentifierName(typeof(object).FullName!)),
                    Parameter(
                        Identifier("context"))
                      .WithType(
                        IdentifierName("ExecutionGraph.Runners.ExecutionContext"))
                  }
                )))
            .WithBody(Block(body))
        ).NormalizeWhitespace();
    }

    private IEnumerable<SyntaxNodeOrToken> CreatePropertyConstructorArgs()
    {
      var fieldToConstructorArg = _metadata.CreatePropertyConstructorArgs();
      
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
          NodeTypeMetadata.FieldType.Continuation => Argument(InvocationExpression(
              MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("context"),
                GenericName(
                    Identifier("GetContinuation"))
                  .WithTypeArgumentList(
                    TypeArgumentList(
                      SingletonSeparatedList<TypeSyntax>(
                        IdentifierName(field.Item2.Type.FullName!))))))
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

    private MemberDeclarationSyntax[] CreateInputsProperty()
    {
      var portType = typeof(Value).FullName!;
      var propertyType = $"System.Collections.Generic.IReadOnlyList<{portType}>";
      return new MemberDeclarationSyntax[]
      {
        PropertyDeclaration(ParseTypeName(propertyType), "Inputs")
          .AddModifiers(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.SealedKeyword),
            Token(SyntaxKind.OverrideKeyword)
          )
          .AddAccessorListAccessors(
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
              .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
          )
          .WithInitializer(
            EqualsValueClause(
              ArrayCreationExpression(ArrayType(ParseTypeName(portType))
                  .WithRankSpecifiers(
                    SingletonList(
                      ArrayRankSpecifier(
                        SingletonSeparatedList<ExpressionSyntax>(
                          OmittedArraySizeExpression())))))
                .WithInitializer(
                  InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SeparatedList<ExpressionSyntax>(
                      CreateInputsPorts()
                    ))
                )))
          .WithSemicolonToken(
            Token(SyntaxKind.SemicolonToken))
      };
    }

    private List<SyntaxNodeOrToken> CreateInputsPorts()
    {
      var entries = new List<SyntaxNodeOrToken>();
      foreach (var input in _metadata.ContinuationsIn.Concat(_metadata.Inputs))
      {
        entries.Add(ObjectCreationExpression(IdentifierName(typeof(Value).FullName!))
          .WithArgumentList(
            ArgumentList(
              SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]
                {
                  Argument(
                    LiteralExpression(
                      SyntaxKind.StringLiteralExpression,
                      Literal(input.PropertyName))),
                  Token(SyntaxKind.CommaToken),
                  Argument(
                    TypeOfExpression(
                      IdentifierName(input.Type.FullName!))),
                  Token(SyntaxKind.CommaToken),
                  Argument(
                    LiteralExpression(
                      SyntaxKind.StringLiteralExpression,
                      Literal(input.UnityFieldName)))
                })))
        );
        entries.Add(Token(SyntaxKind.CommaToken));
      }

      return entries;
    }

    private List<SyntaxNodeOrToken> CreateOutputsPorts()
    {
      var entries = new List<SyntaxNodeOrToken>();
      foreach (var output in _metadata.ContinuationsOut.Concat(_metadata.Outputs))
      {
        entries.Add(ObjectCreationExpression(IdentifierName(typeof(Value).FullName!))
          .WithArgumentList(
            ArgumentList(
              SeparatedList<ArgumentSyntax>(
                new SyntaxNodeOrToken[]
                {
                  Argument(
                    LiteralExpression(
                      SyntaxKind.StringLiteralExpression,
                      Literal(output.PropertyName))),
                  Token(SyntaxKind.CommaToken),
                  Argument(
                    TypeOfExpression(
                      IdentifierName(output.Type.FullName!)))
                })))
        );
        entries.Add(Token(SyntaxKind.CommaToken));
      }

      return entries;
    }

    private List<SyntaxNodeOrToken> CreateContinuationsPorts()
    {
      var entries = new List<SyntaxNodeOrToken>();
      foreach (var continuation in _metadata.ContinuationsOut)
      {
        entries.Add(
          LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            Literal(continuation.PropertyName)));
        entries.Add(Token(SyntaxKind.CommaToken));
      }

      return entries;
    }
  }
}
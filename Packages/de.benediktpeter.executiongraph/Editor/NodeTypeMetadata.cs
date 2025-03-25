using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExecutionGraph.Attributes;
using ExecutionGraph.UnityObjects;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEngine;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExecutionGraph.Editor
{
  internal class NodeTypeMetadata
  {
    public Type Type { get; }

    public NodeType? GraphNodeType { get; }
    
    public List<MethodInfo> ExecuteMethods { get; } = new();
    public List<MethodInfo> ComputeMethods { get; } = new();

    public List<ValueField> Inputs { get; } = new();
    public List<ValueField> Outputs { get; } = new();
    public List<ValueField> Properties { get; } = new();
    public List<ValueField> ContinuationsIn { get; } = new();
    public List<ValueField> ContinuationsOut { get; } = new();
    
    [CanBeNull]
    private readonly SyntaxTree _originalSource;

    [CanBeNull]
    private readonly string _originalFile;

    public NodeTypeMetadata(Type type)
    {
      Type = type;

      if (Type.GetCustomAttribute<ActionNodeAttribute>() != null)
      {
        GraphNodeType = NodeType.Action;
      }
      else if (Type.GetCustomAttribute<ValueNodeAttribute>() != null)
      {
        GraphNodeType = NodeType.Value;
      }

      string scriptLocation = null;
      var allScripts = Resources.FindObjectsOfTypeAll<MonoScript>();
      foreach (var monoScript in allScripts)
      {
        if (monoScript.GetClass() != Type)
        {
          continue;
        }

        scriptLocation = AssetDatabase.GetAssetPath(monoScript);
        break;
      }

      if (scriptLocation != null)
      {
        _originalFile = scriptLocation;
        _originalSource = CSharpSyntaxTree.ParseText(File.ReadAllText(scriptLocation));
      }

      var currentType = Type;
      while (currentType != null)
      {
        foreach (var field in currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
        {
          if (field.GetCustomAttribute<PropertyInAttribute>(true) != null)
          {
            Properties.Add(new ValueField(field.Name, field.FieldType, GetDefaultValueExpression(field.Name),
              FieldType.Property));
          }

          if (field.GetCustomAttribute<ContinuationOutAttribute>(true) != null &&
              field.FieldType == typeof(Continuation))
          {
            ContinuationsOut.Add(new ValueField(field.Name, field.FieldType, null, FieldType.Continuation));
          }

          if (field.GetCustomAttribute<ContinuationInAttribute>(true) != null &&
              field.FieldType == typeof(Continuation))
          {
            ContinuationsIn.Add(new ValueField(field.Name, field.FieldType, null, FieldType.Continuation));
          }
        }

        foreach (var property in currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
          if (property.GetCustomAttribute<ValueOutAttribute>(true) != null)
          {
            Outputs.Add(new ValueField(property.Name, property.PropertyType,
              EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
              FieldType.Output));
          }
        }

        foreach (var method in currentType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
          if (method.GetCustomAttribute<ExecuteAttribute>(true) != null)
          {
            ExecuteMethods.Add(method);

            foreach (var parameter in method.GetParameters())
            {
              //TODO: parameter.DefaultValue
              Inputs.Add(new ValueField(parameter.Name, parameter.ParameterType,
                EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
                FieldType.Input));
            }
          }

          var computeAttribute = method.GetCustomAttribute<ComputeAttribute>(true);
          if (computeAttribute != null)
          {
            ComputeMethods.Add(method);

            foreach (var parameter in method.GetParameters())
            {
              //TODO: parameter.DefaultValue
              Inputs.Add(new ValueField(parameter.Name, parameter.ParameterType,
                EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
                FieldType.Input));
            }

            Outputs.Add(new ValueField(computeAttribute.Output, method.ReturnType,
              EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
              FieldType.Result));
          }
        }

        currentType = currentType.BaseType;
      }
    }

    public IReadOnlyCollection<string> Validate()
    {
      var errors = new List<string>();

      if (_originalFile == null)
      {
        errors.Add(
          "Could not find script asset file. Type name and file name have to match in order for Unity to find them.");
      }

      if (!GraphNodeType.HasValue)
      {
        errors.Add("The type has to be annotated either with ActionNode or with ValueNode.");
      }

      if (GraphNodeType == NodeType.Action && (ExecuteMethods.Count != 1 || ComputeMethods.Count != 0))
      {
        errors.Add(
          "An ActionNode requires exactly one method annotated with Execute and must not contain methods annotated with Compute.");
      }

      if (GraphNodeType == NodeType.Value && (ExecuteMethods.Count != 0 || ComputeMethods.Count != 1))
      {
        errors.Add(
          "A ValueNode requires exactly one method annotated with Compute and must not contain methods annotated with Execute.");
      }

      return errors;
    }
    
    [CanBeNull]
    public string GetAssemblyRoot()
    {
      if (_originalFile == null)
      {
        return null;
      }

      var root = new DirectoryInfo(_originalFile).Parent;
      while (root != null)
      {
        if (Directory.EnumerateFiles(root.ToString(), "*.asmdef").Any())
        {
          break;
        }

        root = root.Parent;
      }

      return root?.ToString();
    }

    private EqualsValueClauseSyntax GetDefaultValueExpression(string fieldName)
    {
      var classDefinition = FindClass();
      if (classDefinition == null)
      {
        return EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression));
      }

      foreach (var member in classDefinition.Members)
      {
        if (member is not FieldDeclarationSyntax fieldSyntax)
        {
          continue;
        }

        foreach (var variable in fieldSyntax.Declaration.Variables)
        {
          if (variable.Identifier.Text != fieldName)
          {
            continue;
          }

          return variable.Initializer;
        }
      }

      return EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression));
    }

    [CanBeNull]
    private ClassDeclarationSyntax FindClass()
    {
      if (_originalSource == null)
      {
        return null;
      }

      var root = _originalSource.GetCompilationUnitRoot();

      if (Type.Namespace != null)
      {
        foreach (var globalMember in root.Members)
        {
          if (globalMember is not NamespaceDeclarationSyntax namespaceSyntax)
          {
            continue;
          }

          foreach (var namespaceMember in namespaceSyntax.Members)
          {
            if (namespaceMember is not ClassDeclarationSyntax classSyntax)
            {
              continue;
            }

            if (classSyntax.Identifier.Text == Type.Name)
            {
              return classSyntax;
            }
          }
        }

        return null;
      }

      foreach (var globalMember in root.Members)
      {
        if (globalMember is not ClassDeclarationSyntax classSyntax)
        {
          continue;
        }

        if (classSyntax.Identifier.Text == Type.Name)
        {
          return classSyntax;
        }
      }

      return null;
    }

    public IReadOnlyList<(ParameterInfo, ValueField)> CreatePropertyConstructorArgs()
    {
      var properties = Properties
        .ToDictionary(e => e.Name.ToLowerInvariant().Replace("_", ""), e => e);
      var continuationsOut = ContinuationsOut
        .ToDictionary(n => n.Name.ToLowerInvariant().Replace("_", ""), e => e);
      var allFieldsCount = properties.Count + continuationsOut.Count;

      foreach (var constructorInfo in Type.GetConstructors())
      {
        var constructorArgs = constructorInfo.GetParameters();
        if (constructorArgs.Length != allFieldsCount)
        {
          continue;
        }

        var fieldToConstructorArg = new List<(ParameterInfo, ValueField)>();

        foreach (var arg in constructorArgs)
        {
          var name = arg.Name.ToLowerInvariant().Replace("_", "");
          if (properties.TryGetValue(name, out var property))
          {
            fieldToConstructorArg.Add((arg, property));
          }
          else if (continuationsOut.TryGetValue(name, out var continuation))
          {
            fieldToConstructorArg.Add((arg, continuation));
          }
        }

        if (fieldToConstructorArg.Count != allFieldsCount)
        {
          continue;
        }

        return fieldToConstructorArg;
      }

      throw new ArgumentException($"Could not find a constructor that initializes all properties of type {Type.Name}");
    }

    internal enum FieldType
    {
      Input,
      Output,
      Property,
      Continuation,
      Result
    }

    private static string GetPropertyName(string name)
    {
      name = name.Trim('_');
      return char.ToUpper(name[0]) + name[1..];
    }

    internal readonly struct ValueField
    {
      public string Name { get; }
      public string UnityFieldName { get; }
      public string PropertyName { get; }
      public Type Type { get; }
      public EqualsValueClauseSyntax DefaultValue { get; }
      public FieldType FieldType { get; }

      public ValueField(string name, Type type, EqualsValueClauseSyntax defaultValue, FieldType fieldType)
      {
        if (name.Length == 0)
        {
          throw new ArgumentException("Field name may not be empty!");
        }

        Name = name;

        // Name for Unity serialized field
        // "_example"
        UnityFieldName = name.Trim('_');
        UnityFieldName = "_" + char.ToLower(UnityFieldName[0]) + UnityFieldName[1..];

        // Name for read-only property
        // "Example"
        PropertyName = GetPropertyName(name);

        Type = type;

        DefaultValue = defaultValue;

        FieldType = fieldType;
      }
    }
  }
}
using System;
using System.Collections.Generic;
using ExecutionGraph.Nodes;
using ExecutionGraph.Runners;
using UnityEditor;
using UnityEngine;

namespace ExecutionGraph.UnityObjects
{
  public enum NodeType
  {
    Action,
    Value
  }

  public abstract class NodeObject : ScriptableObject
  {
    [SerializeField] [HideInInspector]
    private string guid;

    [SerializeField] [HideInInspector]
    private Vector2 position;

    public string Guid => guid;

    public Vector2 Position
    {
      get => position;
      set => position = value;
    }

    public NodeType Type => Continuations.Count > 0 ? NodeType.Action : NodeType.Value;

    public abstract IReadOnlyList<Value> Inputs { get; }
    public abstract IReadOnlyList<Value> Outputs { get; }
    public abstract IReadOnlyList<string> Continuations { get; }
    
    public abstract Type ImplType { get; }

    public abstract object Instantiate(InstantiationContext context);

    public abstract object Execute(object instance, ExecutionContext context);

    private void OnEnable()
    {
      guid ??= GUID.Generate().ToString();
    }

#if UNITY_EDITOR
    public string Title
    {
      get
      {
        const string nodeSuffix = "Node";
        var className = GetType().Name;
        if (className.EndsWith(nodeSuffix))
        {
          className = className[..^nodeSuffix.Length];
        }

        return ObjectNames.NicifyVariableName(className);
      }
    }
    

#endif
  }
}
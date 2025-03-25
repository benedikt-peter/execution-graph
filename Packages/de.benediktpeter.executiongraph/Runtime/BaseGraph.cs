using System;
using System.Collections.Generic;
using ExecutionGraph.UnityObjects;
using Internal.GeneratedExecutionGraphNodes;
using JetBrains.Annotations;
using UnityEngine;

namespace ExecutionGraph
{
  public abstract class BaseGraph : ScriptableObject
  {
    [SerializeField]
    // [HideInInspector]
    private List<NodeObject> nodes = new();

    [SerializeField]
    // [HideInInspector]
    private List<Edge> edges = new();

    [SerializeField]
    [HideInInspector]
    private StartNode startNode;

    public StartNode StartNode => startNode;

    public IReadOnlyList<NodeObject> Nodes => nodes;

    public IReadOnlyList<Edge> Edges => edges;

    private readonly Dictionary<NodeObject, int> _idsByNode = new();
    private readonly Dictionary<int, List<Edge>> _incomingEdges = new();
    private readonly Dictionary<int, List<Edge>> _outgoingEdges = new();

    public int GetIdByNode(NodeObject node)
    {
      return _idsByNode[node];
    }

    public NodeObject GetNodeById(int nodeId)
    {
      return nodes[nodeId];
    }

    [CanBeNull]
    public Edge GetOutgoingEdge(int nodeId, string port)
    {
      if (!_outgoingEdges.TryGetValue(nodeId, out var outgoingEdges))
      {
        return null;
      }

      foreach (var edge in outgoingEdges)
      {
        if (edge.FromPort == port)
        {
          return edge;
        }
      }

      return null;
    }

    [CanBeNull]
    public Edge GetIncomingEdge(int nodeId, string port)
    {
      if (!_incomingEdges.TryGetValue(nodeId, out var incomingEdges))
      {
        return null;
      }

      foreach (var edge in incomingEdges)
      {
        if (edge.ToPort == port)
        {
          return edge;
        }
      }

      return null;
    }

    private void OnEnable()
    {
      RebuildData();
    }

    private void RebuildData()
    {
      _idsByNode.Clear();
      _outgoingEdges.Clear();
      _incomingEdges.Clear();

      for (var i = 0; i < nodes.Count; i++)
      {
        var node = nodes[i];
        _idsByNode[node] = i;
      }

      foreach (var edge in edges)
      {
        var outgoingNodeId = _idsByNode[edge.FromNode];
        if (!_outgoingEdges.TryGetValue(outgoingNodeId, out var outgoingEdges))
        {
          _outgoingEdges[outgoingNodeId] = outgoingEdges = new List<Edge>();
        }

        outgoingEdges.Add(edge);

        var incomingNodeId = _idsByNode[edge.ToNode];
        if (!_incomingEdges.TryGetValue(incomingNodeId, out var incomingEdges))
        {
          _incomingEdges[incomingNodeId] = incomingEdges = new List<Edge>();
        }

        incomingEdges.Add(edge);
      }
    }

#if UNITY_EDITOR
    internal NodeObject CreateNode(Type type)
    {
      if (type == typeof(StartNode) && startNode)
      {
        throw new InvalidOperationException($"Cannot add another {nameof(StartNode)} to graph '{name}'");
      }

      var node = (NodeObject) CreateInstance(type);
      node.name = type.Name;

      nodes.Add(node);

      if (type == typeof(StartNode))
      {
        startNode = (StartNode) node;
      }

      RebuildData();

      return node;
    }

    internal void DeleteNode(NodeObject node)
    {
      nodes.Remove(node);

      RebuildData();
    }

    internal void AddEdge(NodeObject fromNode, string fromPort, NodeObject toNode, string toPort)
    {
      DeleteEdge(fromNode, fromPort, toNode, toPort);

      edges.Add(new Edge(fromNode, fromPort, toNode, toPort));

      RebuildData();
    }

    internal void DeleteEdge(NodeObject fromNode, string fromPort, NodeObject toNode, string toPort)
    {
      edges.RemoveAll(e =>
        e.FromNode == fromNode &&
        e.FromPort == fromPort &&
        e.ToNode == toNode &&
        e.ToPort == toPort
      );

      RebuildData();
    }
#endif
  }
}
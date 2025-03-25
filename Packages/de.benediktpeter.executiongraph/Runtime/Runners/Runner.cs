using System.Collections.Generic;
using ExecutionGraph.UnityObjects;

namespace ExecutionGraph.Runners
{
  public class Runner
  {
    private readonly BaseGraph _graph;
    private readonly Node[] _nodes;

    public Runner(BaseGraph graph)
    {
      _graph = graph;
      _nodes = new Node[_graph.Nodes.Count];

      for (var i = 0; i < _graph.Nodes.Count; ++i)
      {
        _nodes[i] = new Node(i, Instantiate(i), _graph.Nodes[i]);
      }
    }

    public void RunGraph()
    {
      Execute(new Continuation(_graph.GetIdByNode(_graph.StartNode)));
    }

    private void Execute(Continuation continuation)
    {
      if (continuation.NodeId < 0)
      {
        return;
      }
      
      var node = _nodes[continuation.NodeId];

      var result = Execute(node);

      while (result.MoveNext())
      {
        Execute(result.Current);
      }
    }

    private object EvaluateOutput(NodeObject obj, string port)
    {
      var nodeId = _graph.GetIdByNode(obj);
      var node = _nodes[nodeId];

      if (obj.Type == NodeType.Value)
      {
        Invoke(node);
      }

      return node.Outputs.GetValueOrDefault(port);
    }

    private object Invoke(Node node)
    {
      var inputs = new Dictionary<string, object>();
      foreach (var input in node.Object.Inputs)
      {
        if (input.Type == typeof(Continuation))
        {
          continue;
        }

        var edge = _graph.GetIncomingEdge(node.NodeId, input.Name);
        inputs[input.Name] = edge != null ? EvaluateOutput(edge.FromNode, edge.FromPort) : null;
      }

      return node.Object.Execute(node.Instance, new ExecutionContext(inputs, node.Outputs));
    }

    private IEnumerator<Continuation> Execute(Node node)
    {
      var result = Invoke(node);

      switch (result)
      {
        case IEnumerator<Continuation> loop:
        {
          while (loop.MoveNext())
          {
            yield return loop.Current;
          }

          break;
        }
        case Continuation continuation:
          yield return continuation;
          break;
      }

      yield return Continuation.Empty;
    }

    private object Instantiate(int nodeId)
    {
      var node = _graph.Nodes[nodeId];

      var continuations = new Dictionary<string, object>();
      foreach (var continuation in node.Continuations)
      {
        var edge = _graph.GetOutgoingEdge(nodeId, continuation);
        continuations[continuation] =
          edge != null ? new Continuation(_graph.GetIdByNode(edge.ToNode)) : Continuation.Empty;
      }

      return node.Instantiate(new InstantiationContext(continuations));
    }

    private readonly struct Node
    {
      public int NodeId { get; }
      public object Instance { get; }
      public NodeObject Object { get; }
      public Dictionary<string, object> Outputs { get; }

      public Node(int nodeId, object instance, NodeObject obj)
      {
        NodeId = nodeId;
        Instance = instance;
        Object = obj;
        Outputs = new Dictionary<string, object>();
      }
    }
  }
}
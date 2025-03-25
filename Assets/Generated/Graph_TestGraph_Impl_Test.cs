//ReSharper disable All

using System;
using ExecutionGraph;
using ExecutionGraph.Nodes;
using ExecutionGraph.Runners;
using TestProject.NewNodes;

namespace Internal.Generated
{
  class Graph_TestGraph_Impl_Test : IRunnableGraph
  {
    private StartNode _node0StartNode = new(new Continuation(1));
    private LogNode _node1LogNode = new(Continuation.Empty, LogLevel.Error, "Test");
    private CustomValue _node2CustomValue = new(10);
    private CustomValue _node3CustomValue = new(25);

    private Continuation ExecuteNode0StartNode()
    {
      return _node0StartNode.Execute();
    }

    private Continuation ExecuteNode1LogNode()
    {
      var parameter = ComputeNode2CustomValue();
      return _node1LogNode.Execute(parameter);
    }

    private int ComputeNode2CustomValue()
    {
      var testIn = ComputeNode3CustomValue();
      return _node2CustomValue.Compute(testIn);
    }
    
    private int ComputeNode3CustomValue()
    {
      return _node3CustomValue.Compute(5);
    }

    public void Run()
    {
      var node = new Continuation(0);
      while (node.NodeId >= 0)
      {
        node = node.NodeId switch
        {
          0 => ExecuteNode0StartNode(),
          1 => ExecuteNode1LogNode(),
          _ => throw new ArgumentOutOfRangeException()
        };
      }
    }
  }
}
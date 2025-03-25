using System;
using ExecutionGraph.UnityObjects;
using UnityEngine;

namespace ExecutionGraph
{
  [Serializable]
  public class Edge
  {
    [SerializeField]
    private NodeObject fromNode;

    [SerializeField]
    private string fromPort;

    [SerializeField]
    private NodeObject toNode;

    [SerializeField]
    private string toPort;

    public NodeObject FromNode => fromNode;

    public string FromPort => fromPort;

    public NodeObject ToNode => toNode;

    public string ToPort => toPort;

    public Edge(NodeObject fromNode, string fromPort, NodeObject toNode, string toPort)
    {
      this.fromNode = fromNode;
      this.fromPort = fromPort;
      this.toNode = toNode;
      this.toPort = toPort;
    }
  }
}
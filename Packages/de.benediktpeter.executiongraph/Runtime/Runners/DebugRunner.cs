using System;
using ExecutionGraph.Nodes;
using UnityEditor;
using UnityEngine;

namespace ExecutionGraph.Runners
{
  public class DebugRunner : MonoBehaviour
  {
    [SerializeField]
    private BaseGraph graph;

    private void Awake()
    {
      var startNode = graph?.StartNode;
      if (!startNode)
      {
        return;
      }

      var runner = new Runner(graph);
      try
      {
        runner.RunGraph();
      }
      catch (Exception e)
      {
        Debug.LogError(e.Message);
        Debug.LogException(e);
      }

      EditorApplication.ExitPlaymode();
    }
  }
}
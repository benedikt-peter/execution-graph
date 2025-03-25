using System;
using System.Collections.Generic;
using System.Linq;
using ExecutionGraph.UnityObjects;
using Internal.GeneratedExecutionGraphNodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExecutionGraph.Editor
{
  [UxmlElement]
  public partial class GraphView : UnityEditor.Experimental.GraphView.GraphView
  {
    public Action<NodeObject> onNodeSelected;

    private BaseGraph _graph;

    private readonly Dictionary<UnityEditor.Experimental.GraphView.Edge, NodeView> _incoming = new();

    public GraphView()
    {
      style.flexGrow = 1;
      Insert(0, new GridBackground() { name = "grid_background" });
      this.AddManipulator(new ContentZoomer());
      this.AddManipulator(new ContentDragger());
      this.AddManipulator(new SelectionDragger());
      this.AddManipulator(new RectangleSelector());

      Undo.undoRedoPerformed += UndoRedoPerformed;
    }

    private void UndoRedoPerformed()
    {
      PopulateView(_graph);
      AssetDatabase.SaveAssets();
    }

    public void PopulateView(BaseGraph tree)
    {
      _graph = tree;

      graphViewChanged -= OnGraphViewChanged;
      DeleteElements(graphElements);
      graphViewChanged += OnGraphViewChanged;

      if (!_graph)
      {
        return;
      }

      foreach (var node in _graph.Nodes)
      {
        CreateNodeView(node);
      }

      var invalidEdges = new List<Edge>();
      foreach (var edge in _graph.Edges)
      {
        var fromView = GetNodeByGuid(edge.FromNode.Guid) as NodeView;
        var toView = GetNodeByGuid(edge.ToNode.Guid) as NodeView;
        if (fromView == null || toView == null)
        {
          continue;
        }

        var output = fromView.GetOutputPort(edge.FromPort);
        var input = toView.GetInputPort(edge.ToPort);

        if (output == null || input == null)
        {
          invalidEdges.Add(edge);
          continue;
        }

        var createdEdge = output.ConnectTo(input);

        AddElement(createdEdge);

        _incoming[createdEdge] = toView;
      }

      foreach (var edge in invalidEdges)
      {
        Debug.LogWarning($"Removing invalid edge from {edge.FromNode}:{edge.FromPort} to {edge.ToNode}:{edge.ToPort}");
        _graph.DeleteEdge(edge.FromNode, edge.FromPort, edge.ToNode, edge.ToPort);
      }
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
      var dirtyNodes = new List<NodeView>();

      if (graphViewChange.elementsToRemove != null)
      {
        foreach (var element in graphViewChange.elementsToRemove)
        {
          switch (element)
          {
            case NodeView nodeView:
              DeleteNode(nodeView.Node);
              break;
            case UnityEditor.Experimental.GraphView.Edge edge:
            {
              var input = edge.input.node ?? _incoming.GetValueOrDefault(edge);

              if (edge.output.node is not NodeView fromView || input is not NodeView toView)
              {
                continue;
              }

              _incoming.Remove(edge);

              _graph.DeleteEdge(fromView.Node, edge.output.name, toView.Node, edge.input.name);

              dirtyNodes.Add(fromView);
              dirtyNodes.Add(toView);

              break;
            }
          }
        }
      }

      if (graphViewChange.edgesToCreate != null && _graph)
      {
        foreach (var edge in graphViewChange.edgesToCreate)
        {
          if (edge.output.node is not NodeView fromView || edge.input.node is not NodeView toView)
          {
            continue;
          }

          _incoming[edge] = toView;

          _graph.AddEdge(fromView.Node, edge.output.name, toView.Node, edge.input.name);

          dirtyNodes.Add(fromView);
          dirtyNodes.Add(toView);
        }
      }

      foreach (var nodeView in dirtyNodes)
      {
        nodeView.UpdateInputPorts();
      }

      return graphViewChange;
    }

    private void CreateNodeView(NodeObject node)
    {
      var nodeView = new NodeView(_graph, node)
      {
        OnNodeSelected = onNodeSelected
      };

      AddElement(nodeView);
    }

    private void CreateNode(Type type)
    {
      if (!_graph)
      {
        return;
      }

      var node = _graph.CreateNode(type);
      CreateNodeView(node);
      Undo.RecordObject(_graph, "Graph Editor (Create Node)");

      if (Application.isPlaying) return;

      AssetDatabase.AddObjectToAsset(node, _graph);
      AssetDatabase.SaveAssets();

      Undo.RegisterCreatedObjectUndo(node, "Graph Editor (Create Node)");
      EditorUtility.SetDirty(node);
    }

    private void DeleteNode(NodeObject node)
    {
      if (!_graph)
      {
        return;
      }

      _graph.DeleteNode(node);
      Undo.RecordObject(_graph, "Graph Editor (Delete Node)");

      Undo.DestroyObjectImmediate(node);
      AssetDatabase.SaveAssets();
      EditorUtility.SetDirty(_graph);
    }

    public void UpdateNodeStates()
    {
      foreach (NodeView nodeView in nodes.OfType<NodeView>())
      {
        // nodeView.UpdateState();
      }
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
      if (!_graph)
      {
        return;
      }

      var types = TypeCache.GetTypesDerivedFrom<NodeObject>();
      foreach (var type in types)
      {
        if (type.IsAbstract || type.IsGenericType || (type == typeof(StartNode) && _graph.StartNode))
        {
          continue;
        }

        evt.menu.AppendAction($"{type.Name}",
          _ => CreateNode(type));
      }

      base.BuildContextualMenu(evt);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
      return ports.ToList()!.Where(endPort =>
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node &&
        endPort.portType.IsAssignableFrom(startPort.portType)).ToList();
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ExecutionGraph.UnityObjects;
using Internal.GeneratedExecutionGraphNodes;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;
using Port = UnityEditor.Experimental.GraphView.Port;

namespace ExecutionGraph.Editor
{
  public class NodeView : Node, IDisposable
  {
    public Action<NodeObject> OnNodeSelected;

    public NodeObject Node { get; }

    private readonly BaseGraph _graph;
    private readonly Dictionary<string, Port> _inputPorts = new();
    private readonly Dictionary<string, Port> _outputPorts = new();
    private readonly SerializedObject _serializedObject;

    private readonly VisualElement _properties;

    public NodeView(BaseGraph graph, NodeObject node) : base(
      AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("GraphNodeView")))
    {
      _graph = graph;
      Node = node;

      if (Node == null)
      {
        return;
      }

      _serializedObject = new SerializedObject(Node);

      base.title = Node.Title;

      viewDataKey = Node.Guid;

      style.left = Node.Position.x;
      style.top = Node.Position.y;

      UpdateInputPorts();
      CreateOutputPorts();
      SetupClasses();

      _properties = this.Q("properties");

      CreateProperties();
    }

    [CanBeNull]
    public Port GetInputPort(string portName)
    {
      return _inputPorts.GetValueOrDefault(portName);
    }

    [CanBeNull]
    public Port GetOutputPort(string portName)
    {
      return _outputPorts.GetValueOrDefault(portName);
    }

    public void Dispose()
    {
      _serializedObject?.Dispose();
    }

    private void SetupClasses()
    {
      switch (Node)
      {
        case StartNode:
          AddToClassList("start");
          break;
        case not null:
          AddToClassList(Node.Continuations.Count > 0 ? "action" : "value");
          break;
        default:
          AddToClassList("action");
          break;
      }
    }

    private bool IsInput(string fieldName)
    {
      return Node.Inputs.Any(v => v.FieldName == fieldName);
    }

    private void CreateProperties()
    {
      var iterator = _serializedObject.GetIterator();
      iterator.NextVisible(true);

      do
      {
        if (iterator.name == "m_Script" || IsInput(iterator.name))
        {
          continue;
        }

        var root = new VisualElement();
        root.AddToClassList("property-root");

        var label = new Label(ObjectNames.NicifyVariableName(iterator.name));
        label.AddToClassList("property-label");
        root.Add(label);

        var propertyField = new PropertyField();
        propertyField.AddToClassList("property-drawer");
        propertyField.label = "";
        propertyField.BindProperty(iterator);

        root.Add(propertyField);

        _properties.Add(root);
      } while (iterator.NextVisible(true));
    }

    public void UpdateInputPorts()
    {
      inputContainer.Clear();

      foreach (var input in Node.Inputs)
      {
        var showProperty = _graph.GetIncomingEdge(_graph.GetIdByNode(Node), input.Name) == null;

        var port = InstantiatePort(Orientation.Horizontal, Direction.Input,
          Port.Capacity.Single, input.Type);
        port.portName = "";
        port.name = input.Name;
        port.AddToClassList("input-port");

        _inputPorts[input.Name] = port;

        var root = new VisualElement();
        root.AddToClassList("input-root");
        root.Add(port);

        var label = new Label(ObjectNames.NicifyVariableName(input.Name));
        label.AddToClassList("input-label");
        root.Add(label);

        if (showProperty)
        {
          var property = FindProperty(input.FieldName);
          if (property != null)
          {
            var propertyField = new PropertyField();
            propertyField.AddToClassList("input-drawer");
            propertyField.label = "";
            propertyField.BindProperty(property);
            root.Add(propertyField);
          }
        }

        inputContainer.Add(root);
      }
    }

    private SerializedProperty FindProperty(string fieldName)
    {
      var iterator = _serializedObject.GetIterator();
      iterator.NextVisible(true);

      do
      {
        if (iterator.name == fieldName)
        {
          return iterator;
        }
      } while (iterator.NextVisible(true));

      return null;
    }

    private void CreateOutputPorts()
    {
      foreach (var output in Node.Outputs)
      {
        var port = InstantiatePort(Orientation.Horizontal, Direction.Output,
          Port.Capacity.Single, output.Type);
        port.portName = "";
        port.name = output.Name;
        port.AddToClassList("output-port");

        _outputPorts[output.Name] = port;

        var root = new VisualElement();
        root.AddToClassList("output-root");
        root.Add(port);

        var label = new Label(ObjectNames.NicifyVariableName(output.Name));
        label.AddToClassList("output-label");
        root.Add(label);

        outputContainer.Add(root);
      }
    }

    // public void UpdateState()
    // {
    //     RemoveFromClassList("running");
    //     RemoveFromClassList("success");
    //     RemoveFromClassList("failure");
    //
    //     if (m_description != null)
    //     {
    //         List<Node> nodes = m_node.GetChildren();
    //         int count = m_node.GetChildren().Count;
    //         string text = $"{count} children:";
    //
    //         text = nodes!.Aggregate(
    //             text,
    //             (current, child) =>
    //                 $"{current}\n{(child == null ? "" : child.name)}" +
    //                 $"{(child is DecoratorNode ? $": {(child.GetChildren()[0] != null ? child.GetChildren()[0].name : "")}" : "")}");
    //
    //         m_description.text = text;
    //     }
    //
    //     if (!Application.isPlaying) return;
    //
    //     switch (m_node.state)
    //     {
    //         case Node.State.Running:
    //             if (m_node.IsStarted)
    //             {
    //                 AddToClassList("running");
    //             }
    //
    //             break;
    //         case Node.State.Success:
    //             AddToClassList("success");
    //             break;
    //         case Node.State.Failure:
    //             AddToClassList("failure");
    //             break;
    //     }
    // }


    public override void SetPosition(Rect newPos)
    {
      base.SetPosition(newPos);
      Undo.RecordObject(Node, "Graph Editor (Set Position)");
      Node.Position = new Vector2(newPos.xMin, newPos.yMin);
      EditorUtility.SetDirty(Node);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
      base.BuildContextualMenu(evt);
    }

    public override void OnSelected()
    {
      OnNodeSelected?.Invoke(Node);
      base.OnSelected();
    }
  }
}
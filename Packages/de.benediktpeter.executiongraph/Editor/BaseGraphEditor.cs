using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ExecutionGraph.Editor
{
  public class BaseGraphEditor : EditorWindow
  {
    private static BaseGraphEditor _window;

    private GraphView _graphView;
    private UnityEditor.Editor _editor;
    private BaseGraph _graph;

    [MenuItem("Window/Graph/Editor")]
    public static void OpenTreeEditor()
    {
      _window = GetWindow<BaseGraphEditor>();
      _window.UpdateTitle();
    }

    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
      if (Selection.activeObject is not BaseGraph)
      {
        return false;
      }

      OpenTreeEditor();

      return true;
    }

    private void CreateGUI()
    {
      var visualTreeAsset = Resources.Load<VisualTreeAsset>("GraphEditor");
      visualTreeAsset.CloneTree(rootVisualElement);

      _graphView = rootVisualElement.Q<GraphView>();

      OnSelectionChange();
    }

    private void OnEnable()
    {
      EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
      EditorApplication.playModeStateChanged += OnplayModeStateChanged;
    }

    private void OnDisable()
    {
      EditorApplication.playModeStateChanged -= OnplayModeStateChanged;
    }

    private void OnInspectorUpdate()
    {
      _graphView?.UpdateNodeStates();
    }

    private void OnSelectionChange()
    {
      _graph = Selection.activeObject as BaseGraph;
      // if (tree == null)
      // {
      //     if (Selection.activeGameObject)
      //     {
      //         BehaviorTreeRunner treeRunner = Selection.activeGameObject.GetComponent<BehaviorTreeRunner>();
      //         if (treeRunner)
      //         {
      //             tree = treeRunner.tree;
      //         }
      //     }
      // }

      UpdateTitle();

      if (_graph != null)
      {
        if (Application.isPlaying || AssetDatabase.CanOpenAssetInEditor(_graph.GetInstanceID()))
        {
          var so = new SerializedObject(_graph);
          rootVisualElement.Bind(so);
          _graphView?.PopulateView(_graph);

          return;
        }
      }

      rootVisualElement.Unbind();

      var textField = rootVisualElement.Q<TextField>("GraphName");
      if (textField != null)
      {
        textField.value = string.Empty;
      }
    }

    private void UpdateTitle()
    {
      if (!_window)
      {
        return;
      }

      _window.titleContent = new GUIContent(_graph ? $"Graph Editor {_graph.name}" : "Graph Editor");
    }

    private void OnplayModeStateChanged(PlayModeStateChange obj)
    {
      switch (obj)
      {
        // Occurs during the next update of the Editor application if it is in edit mode and was previously in play mode.
        case PlayModeStateChange.EnteredEditMode:
          OnSelectionChange();
          break;
        // Occurs when exiting edit mode, before the Editor is in play mode.
        case PlayModeStateChange.ExitingEditMode:
          break;
        // Occurs during the next update of the Editor application if it is in play mode and was previously in edit mode.
        case PlayModeStateChange.EnteredPlayMode:
          OnSelectionChange();
          break;
        // Occurs when exiting play mode, before the Editor is in edit mode.
        case PlayModeStateChange.ExitingPlayMode:
          break;
      }
    }
  }
}
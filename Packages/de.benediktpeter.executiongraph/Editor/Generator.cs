using System.Linq;
using ExecutionGraph.Attributes;
using UnityEditor;
using UnityEngine;

namespace ExecutionGraph.Editor
{
  internal static class Generator
  {
    [MenuItem("Test/Generate ScriptableObjects")]
    private static void GenerateScriptableObjects()
    {
      GenerateNodeObjects();

      AssetDatabase.Refresh();
    }

    private static void GenerateNodeObjects()
    {
      var types = TypeCache.GetTypesWithAttribute<NodeAttribute>().ToArray();

      foreach (var type in types)
      {
        if (type.IsGenericType || type.IsAbstract)
        {
          continue;
        }

        var generator = new NodeObjectGenerator(type);

        var errors = generator.Validate();
        if (errors.Count > 0)
        {
          Debug.LogWarning($"Could not generate code for node type {type}, because one or more checks failed.");
          foreach (var error in errors)
          {
            Debug.LogWarning($" Error: {error}");
          }

          continue;
        }

        generator.WriteToDisk();
      }
    }
    
    [MenuItem("Test/Generate Graphs")]
    private static void GenerateGraphs()
    {
      var graphGuids = AssetDatabase.FindAssets($"t:{nameof(BaseGraph)}");
      foreach (var graphGuid in graphGuids)
      {
        var graph = AssetDatabase.LoadAssetAtPath<BaseGraph>(AssetDatabase.GUIDToAssetPath(graphGuid));

        var generator = new GraphObjectGenerator(graph);
        generator.WriteToDisk();
      }

      AssetDatabase.Refresh();
    }
  }
}
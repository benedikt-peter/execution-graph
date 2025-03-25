using ExecutionGraph;
using ExecutionGraph.Attributes;
using UnityEngine;

namespace TestProject.NewNodes
{
  [ActionNode]
  public class CustomAction
  {
    [ContinuationIn]
    private readonly Continuation _contIn;

    [ContinuationOut]
    private readonly Continuation _contOut;

    [PropertyIn]
    private readonly int _property = 10;
    
    [ValueOut]
    public bool Result { get; private set; }

    public CustomAction(Continuation contOut, int property)
    {
      _contOut = contOut;
      _property = property;
    }

    [Execute]
    public Continuation Execute(int testIn = 5)
    {
      Debug.LogError($"Test: {nameof(testIn)}={testIn}, {nameof(_property)}={_property}");

      Result = testIn == _property;
      
      return _contOut;
    }
  }
}
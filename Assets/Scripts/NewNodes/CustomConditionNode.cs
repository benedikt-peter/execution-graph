using ExecutionGraph.Attributes;

namespace TestProject.NewNodes
{
  [ValueNode]
  public class CustomConditionNode
  {
    [PropertyIn]
    private readonly int _property = 10;

    public CustomConditionNode(int property)
    {
      _property = property;
    }

    [Compute]
    public bool Compute(int testIn)
    {
      return testIn == _property;
    }
  }
}
using ExecutionGraph.Attributes;

namespace TestProject.NewNodes
{
  [ValueNode]
  public class CustomValue
  {
    [PropertyIn]
    private readonly int _property = 19;

    public CustomValue(int property)
    {
      _property = property;
    }

    [Compute]
    public int Compute(int testIn)
    {
      return testIn + _property;
    }
  }
}
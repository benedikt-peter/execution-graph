using ExecutionGraph.Attributes;

namespace ExecutionGraph.Nodes.Conditions
{
  [ValueNode]
  public class EqualsConditionNode
  {
    private readonly Variant _valueA;
    private readonly Variant _valueB;

    [Compute]
    public bool Calculate()
    {
      return Equals(_valueA, _valueB);
    }
  }
}
using ExecutionGraph.Attributes;

namespace ExecutionGraph.Nodes
{
  [ValueNode]
  public class ConditionalValueNode
  {
    [Compute]
    public Variant Compute(bool condition, Variant valueA, Variant valueB)
    {
      return condition ? valueA : valueB;
    }
  }
}
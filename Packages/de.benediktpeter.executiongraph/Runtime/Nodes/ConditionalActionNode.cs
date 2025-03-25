using ExecutionGraph.Attributes;

namespace ExecutionGraph.Nodes
{
  [ActionNode]
  public class ConditionalActionNode
  {
    [ContinuationIn]
    private readonly Continuation _contIn;

    [ContinuationOut]
    private readonly Continuation _contOutA;

    [ContinuationOut]
    private readonly Continuation _contOutB;

    public ConditionalActionNode(Continuation contOutA, Continuation contOutB)
    {
      _contOutA = contOutA;
      _contOutB = contOutB;
    }

    [Execute]
    public Continuation Execute(bool condition)
    {
      return condition ? _contOutA : _contOutB;
    }
  }
}
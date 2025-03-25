using ExecutionGraph.Attributes;

namespace ExecutionGraph.Nodes
{
  [ActionNode]
  public class StartNode
  {
    [ContinuationOut]
    private readonly Continuation _contOut;

    public StartNode(Continuation contOut)
    {
      _contOut = contOut;
    }

    [Execute]
    public Continuation Execute()
    {
      return _contOut;
    }
  }
}
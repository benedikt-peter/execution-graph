namespace ExecutionGraph
{
  public readonly struct Continuation
  {
    public static Continuation Empty = new(-1);

    public int NodeId { get; }

    public Continuation(int nodeId)
    {
      NodeId = nodeId;
    }
  }
}
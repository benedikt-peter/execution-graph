using System.Collections.Generic;
using ExecutionGraph.Attributes;

namespace ExecutionGraph.Nodes
{
  [ActionNode]
  public class ForRangeNode
  {
    [ContinuationIn]
    private readonly Continuation _contIn;

    [ContinuationOut]
    private readonly Continuation _loopCont;

    [ContinuationOut]
    private readonly Continuation _finishCont;
    
    [ValueOut]
    public int Iteration { get; private set; }

    public ForRangeNode(Continuation loopCont, Continuation finishCont)
    {
      _loopCont = loopCont;
      _finishCont = finishCont;
    }

    [Execute]
    public IEnumerator<Continuation> Execute(int loopStart, int loopEnd)
    {
      for (var i = loopStart; i < loopEnd; ++i)
      {
        Iteration = i;
        yield return _loopCont;
      }

      yield return _finishCont;
    }
  }
}
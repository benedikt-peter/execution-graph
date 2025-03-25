//ReSharper disable All
namespace Internal.GeneratedExecutionGraphNodes
{
    public class ForRangeNode : ExecutionGraph.UnityObjects.NodeObject
    {
        [UnityEngine.SerializeField]
        private System.Int32 _loopStart = default;
        [UnityEngine.SerializeField]
        private System.Int32 _loopEnd = default;
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContIn", typeof(ExecutionGraph.Continuation), "_contIn"),
            new ExecutionGraph.Value("LoopStart", typeof(System.Int32), "_loopStart"),
            new ExecutionGraph.Value("LoopEnd", typeof(System.Int32), "_loopEnd"),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("LoopCont", typeof(ExecutionGraph.Continuation)),
            new ExecutionGraph.Value("FinishCont", typeof(ExecutionGraph.Continuation)),
            new ExecutionGraph.Value("Iteration", typeof(System.Int32)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
            "LoopCont",
            "FinishCont",
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(ExecutionGraph.Nodes.ForRangeNode);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new ExecutionGraph.Nodes.ForRangeNode(context.GetContinuation<ExecutionGraph.Continuation>("LoopCont"), context.GetContinuation<ExecutionGraph.Continuation>("FinishCont"));
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            ExecutionGraph.Nodes.ForRangeNode node = (ExecutionGraph.Nodes.ForRangeNode)instance;
            System.Collections.Generic.IEnumerator<ExecutionGraph.Continuation> result = node.Execute(context.GetInput("LoopStart", _loopStart), context.GetInput("LoopEnd", _loopEnd));
            System.Collections.Generic.IEnumerator<ExecutionGraph.Continuation> Iterate()
            {
                while (result.MoveNext())
                {
                    context.SetOutput("Iteration", node.Iteration);
                    yield return result.Current;
                }
            }

            return Iterate();
        }
    }
}
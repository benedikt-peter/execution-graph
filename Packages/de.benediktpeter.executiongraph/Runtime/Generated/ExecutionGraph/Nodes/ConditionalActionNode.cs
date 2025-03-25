//ReSharper disable All
namespace Internal.GeneratedExecutionGraphNodes
{
    public class ConditionalActionNode : ExecutionGraph.UnityObjects.NodeObject
    {
        [UnityEngine.SerializeField]
        private System.Boolean _condition = default;
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContIn", typeof(ExecutionGraph.Continuation), "_contIn"),
            new ExecutionGraph.Value("Condition", typeof(System.Boolean), "_condition"),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContOutA", typeof(ExecutionGraph.Continuation)),
            new ExecutionGraph.Value("ContOutB", typeof(ExecutionGraph.Continuation)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
            "ContOutA",
            "ContOutB",
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(ExecutionGraph.Nodes.ConditionalActionNode);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new ExecutionGraph.Nodes.ConditionalActionNode(context.GetContinuation<ExecutionGraph.Continuation>("ContOutA"), context.GetContinuation<ExecutionGraph.Continuation>("ContOutB"));
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            ExecutionGraph.Nodes.ConditionalActionNode node = (ExecutionGraph.Nodes.ConditionalActionNode)instance;
            System.Object result = node.Execute(context.GetInput("Condition", _condition));
            return result;
        }
    }
}
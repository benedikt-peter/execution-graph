//ReSharper disable All
namespace Internal.GeneratedExecutionGraphNodes
{
    public class StartNode : ExecutionGraph.UnityObjects.NodeObject
    {
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContOut", typeof(ExecutionGraph.Continuation)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
            "ContOut",
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(ExecutionGraph.Nodes.StartNode);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new ExecutionGraph.Nodes.StartNode(context.GetContinuation<ExecutionGraph.Continuation>("ContOut"));
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            ExecutionGraph.Nodes.StartNode node = (ExecutionGraph.Nodes.StartNode)instance;
            System.Object result = node.Execute();
            return result;
        }
    }
}
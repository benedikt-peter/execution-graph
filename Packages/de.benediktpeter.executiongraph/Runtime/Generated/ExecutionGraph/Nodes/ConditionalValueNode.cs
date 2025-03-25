//ReSharper disable All
namespace Internal.GeneratedExecutionGraphNodes
{
    public class ConditionalValueNode : ExecutionGraph.UnityObjects.NodeObject
    {
        [UnityEngine.SerializeField]
        private System.Boolean _condition = default;
        [UnityEngine.SerializeField]
        private ExecutionGraph.Variant _valueA = default;
        [UnityEngine.SerializeField]
        private ExecutionGraph.Variant _valueB = default;
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("Condition", typeof(System.Boolean), "_condition"),
            new ExecutionGraph.Value("ValueA", typeof(ExecutionGraph.Variant), "_valueA"),
            new ExecutionGraph.Value("ValueB", typeof(ExecutionGraph.Variant), "_valueB"),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("Result", typeof(ExecutionGraph.Variant)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(ExecutionGraph.Nodes.ConditionalValueNode);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new ExecutionGraph.Nodes.ConditionalValueNode();
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            ExecutionGraph.Nodes.ConditionalValueNode node = (ExecutionGraph.Nodes.ConditionalValueNode)instance;
            System.Object result = node.Compute(context.GetInput("Condition", _condition), context.GetInput("ValueA", _valueA), context.GetInput("ValueB", _valueB));
            context.SetOutput("Result", result);
            return result;
        }
    }
}
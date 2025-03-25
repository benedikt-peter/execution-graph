//ReSharper disable All
namespace Internal.GeneratedExecutionGraphNodesConditions
{
    public class EqualsConditionNode : ExecutionGraph.UnityObjects.NodeObject
    {
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("Result", typeof(System.Boolean)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(ExecutionGraph.Nodes.Conditions.EqualsConditionNode);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new ExecutionGraph.Nodes.Conditions.EqualsConditionNode();
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            ExecutionGraph.Nodes.Conditions.EqualsConditionNode node = (ExecutionGraph.Nodes.Conditions.EqualsConditionNode)instance;
            System.Object result = node.Calculate();
            context.SetOutput("Result", result);
            return result;
        }
    }
}
//ReSharper disable All
namespace Internal.GeneratedExecutionGraphNodes
{
    public class LogNode : ExecutionGraph.UnityObjects.NodeObject
    {
        [UnityEngine.SerializeField]
        private System.Object _parameter = default;
        [UnityEngine.SerializeField]
        private ExecutionGraph.Nodes.LogLevel _level;
        [UnityEngine.SerializeField]
        private System.String _message;
        public ExecutionGraph.Nodes.LogLevel Level
        {
            get
            {
                return _level;
            }
        }

        public System.String Message
        {
            get
            {
                return _message;
            }
        }

        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContIn", typeof(ExecutionGraph.Continuation), "_contIn"),
            new ExecutionGraph.Value("Parameter", typeof(System.Object), "_parameter"),
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
                return typeof(ExecutionGraph.Nodes.LogNode);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new ExecutionGraph.Nodes.LogNode(context.GetContinuation<ExecutionGraph.Continuation>("ContOut"), _level, _message);
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            ExecutionGraph.Nodes.LogNode node = (ExecutionGraph.Nodes.LogNode)instance;
            System.Object result = node.Execute(context.GetInput("Parameter", _parameter));
            return result;
        }
    }
}
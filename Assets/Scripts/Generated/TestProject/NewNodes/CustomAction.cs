//ReSharper disable All
namespace Internal.GeneratedTestProjectNewNodes
{
    public class CustomAction : ExecutionGraph.UnityObjects.NodeObject
    {
        [UnityEngine.SerializeField]
        private System.Int32 _testIn = default;
        [UnityEngine.SerializeField]
        private System.Int32 _property = 10;
        public System.Int32 Property
        {
            get
            {
                return _property;
            }
        }

        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContIn", typeof(ExecutionGraph.Continuation), "_contIn"),
            new ExecutionGraph.Value("TestIn", typeof(System.Int32), "_testIn"),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("ContOut", typeof(ExecutionGraph.Continuation)),
            new ExecutionGraph.Value("Result", typeof(System.Boolean)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
            "ContOut",
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(TestProject.NewNodes.CustomAction);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new TestProject.NewNodes.CustomAction(context.GetContinuation<ExecutionGraph.Continuation>("ContOut"), _property);
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            TestProject.NewNodes.CustomAction node = (TestProject.NewNodes.CustomAction)instance;
            System.Object result = node.Execute(context.GetInput("TestIn", _testIn));
            context.SetOutput("Result", node.Result);
            return result;
        }
    }
}
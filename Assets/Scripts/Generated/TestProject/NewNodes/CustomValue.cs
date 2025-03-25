//ReSharper disable All
namespace Internal.GeneratedTestProjectNewNodes
{
    public class CustomValue : ExecutionGraph.UnityObjects.NodeObject
    {
        [UnityEngine.SerializeField]
        private System.Int32 _testIn = default;
        [UnityEngine.SerializeField]
        private System.Int32 _property = 19;
        public System.Int32 Property
        {
            get
            {
                return _property;
            }
        }

        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Inputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("TestIn", typeof(System.Int32), "_testIn"),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<ExecutionGraph.Value> Outputs { get; } = new ExecutionGraph.Value[]
        {
            new ExecutionGraph.Value("Result", typeof(System.Int32)),
        };
        public sealed override System.Collections.Generic.IReadOnlyList<System.String> Continuations { get; } = new System.String[]
        {
        };

        public sealed override System.Type ImplType
        {
            get
            {
                return typeof(TestProject.NewNodes.CustomValue);
            }
        }

        public override System.Object Instantiate(ExecutionGraph.Runners.InstantiationContext context)
        {
            return new TestProject.NewNodes.CustomValue(_property);
        }

        public override System.Object Execute(System.Object instance, ExecutionGraph.Runners.ExecutionContext context)
        {
            TestProject.NewNodes.CustomValue node = (TestProject.NewNodes.CustomValue)instance;
            System.Object result = node.Compute(context.GetInput("TestIn", _testIn));
            context.SetOutput("Result", result);
            return result;
        }
    }
}
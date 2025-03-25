using System;

namespace ExecutionGraph.Attributes
{
  [AttributeUsage(AttributeTargets.Method)]
  public class ComputeAttribute : Attribute
  {
    public string Output { get; }

    public ComputeAttribute(string output = "Result")
    {
      Output = output;
    }
  }
}
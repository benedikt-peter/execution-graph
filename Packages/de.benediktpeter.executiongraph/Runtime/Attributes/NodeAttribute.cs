using System;

namespace ExecutionGraph.Attributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
  public abstract class NodeAttribute : Attribute
  {
  }
}
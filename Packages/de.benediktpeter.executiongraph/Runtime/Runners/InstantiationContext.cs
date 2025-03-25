using System;
using System.Collections.Generic;

namespace ExecutionGraph.Runners
{
  public readonly struct InstantiationContext
  {
    private readonly IReadOnlyDictionary<string, object> _continuations;

    public InstantiationContext(IReadOnlyDictionary<string, object> continuations)
    {
      _continuations = continuations;
    }

    public T GetContinuation<T>(string name)
    {
      if (!_continuations.TryGetValue(name, out var value))
      {
        throw new ArgumentException($"Could not find continuation {name}!");
      }

      if (!typeof(T).IsAssignableFrom(value.GetType()))
      {
        throw new ArgumentException(
          $"Incompatible types for continuation {name}: Expected {typeof(T).Name}, found {value.GetType().Name}!");
      }

      return (T) value;
    }
  }
}
using System;
using System.Collections.Generic;

namespace ExecutionGraph.Runners
{
  public readonly struct ExecutionContext
  {
    private readonly IReadOnlyDictionary<string, object> _inputs;
    private readonly Dictionary<string, object> _outputs;

    public ExecutionContext(IReadOnlyDictionary<string, object> inputs, Dictionary<string, object> outputs)
    {
      _inputs = inputs;
      _outputs = outputs;
    }

    public T GetInput<T>(string name, T defaultValue)
    {
      if (!_inputs.TryGetValue(name, out var value))
      {
        throw new ArgumentException($"Could not find input {name}!");
      }

      if (value == null)
      {
        return defaultValue;
      }

      if (!typeof(T).IsAssignableFrom(value.GetType()))
      {
        throw new ArgumentException(
          $"Incompatible types for input {name}: Expected {typeof(T).Name}, found {value.GetType().Name}!");
      }

      return (T) value;
    }

    public void SetOutput(string name, object value)
    {
      _outputs[name] = value;
    }
  }
}
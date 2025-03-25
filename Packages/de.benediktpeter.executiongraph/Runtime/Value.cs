using System;

namespace ExecutionGraph
{
  public readonly struct Value
  {
    public string Name { get; }
    public Type Type { get; }
    public string FieldName { get; }

    public Value(string name, Type type)
    {
      Name = name;
      Type = type;
      FieldName = null;
    }

    public Value(string name, Type type, string fieldName)
    {
      Name = name;
      Type = type;
      FieldName = fieldName;
    }
  }
}
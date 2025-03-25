using System;
using ExecutionGraph.Attributes;
using UnityEngine;

namespace ExecutionGraph.Nodes
{
  public enum LogLevel
  {
    Info,
    Warning,
    Error
  }

  [ActionNode]
  public class LogNode
  {
    [ContinuationIn]
    private readonly Continuation _contIn;

    [ContinuationOut]
    private readonly Continuation _contOut;

    [PropertyIn]
    private readonly LogLevel _level;

    [PropertyIn]
    private readonly string _message;

    public LogNode(Continuation contOut, LogLevel level, string message)
    {
      _contOut = contOut;
      _level = level;
      _message = message;
    }

    [Execute]
    public Continuation Execute(object parameter)
    {
      switch (_level)
      {
        case LogLevel.Info:
          Debug.LogFormat(_message, parameter);
          break;
        case LogLevel.Warning:
          Debug.LogWarningFormat(_message, parameter);
          break;
        case LogLevel.Error:
          Debug.LogErrorFormat(_message, parameter);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return _contOut;
    }
  }
}
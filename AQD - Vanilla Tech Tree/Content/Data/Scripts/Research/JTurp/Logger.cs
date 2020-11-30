using System;
using System.IO;
using System.Text;

using Sandbox.ModAPI;

namespace AQDResearch
{
  public class Logger
  {
    StringBuilder _builder;
    TextWriter _writer;
    bool _isClosed;

    public Logger(string filename, bool isDedicated)
    {
      _writer = isDedicated ? MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(Logger)) : MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(Logger));
      _builder = new StringBuilder(1024);
      _isClosed = false;
    }

    string DateTimeNow => DateTime.Now.ToString("[HH:mm:ss]");

    public void AddDebug(string text)
    {
      _builder.Append(text);
    }

    public void Log(MessageType msgType = MessageType.DEBUG)
    {
      if (_isClosed || _builder.Length == 0)
        return;

      Log(string.Empty, msgType);
    }

    public void Log(string text, MessageType msgType = MessageType.DEBUG)
    {
      if (_isClosed)
        return;

      if (string.IsNullOrEmpty(text))
      {
        if (_builder.Length == 0)
          return;

        text = _builder.ToString();
        _builder.Clear();
      }

      _writer?.WriteLine($"{DateTimeNow} {msgType} | {text}");

      if (_builder.Length > 0)
      {
        _writer?.WriteLine(_builder.ToString());
        _builder.Clear();
      }

      _writer?.Flush();
    }

    public void Log(StringBuilder text, MessageType msgType = MessageType.DEBUG)
    {
      if (!_isClosed)
        Log(text.ToString(), msgType);
    }

    public void Close()
    {
      if (!_isClosed)
      {
        _writer?.Flush();
        _writer?.Close();
        _isClosed = true;
      }
    }

    public static void WriteData(StringBuilder data, string filename)
    {
      using (TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(Logger)))
      {
        writer.Write(data);
        writer.Flush();
      }
    }
  }

  public enum MessageType { ERROR, WARNING, DEBUG }
}

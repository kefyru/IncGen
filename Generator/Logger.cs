using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Generator;

public static class Logger
{
  private static async void Send(string message)
  {
    try {
      await Task.Yield();
      using var cli = new TcpClient();
      await cli.ConnectAsync("localhost", 17171);
      using var stream = cli.GetStream();
      using var writer = new StreamWriter(stream);
      await writer.WriteAsync(message);
      await writer.FlushAsync();
    } catch {
      // ignore
    }
  }
  

  public static void WriteLine(string s)
  {
    Send(s);
  }

  public static long TotalUs(this TimeSpan span) => (long)(span.TotalMilliseconds * 1000);
}
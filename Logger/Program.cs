using System.Net.Sockets;
using System.Runtime.CompilerServices;

var listener = TcpListener.Create(17171);
listener.Start();

Console.WriteLine($"Listen on {listener.LocalEndpoint}");

while (true) {
  var client = await listener.AcceptTcpClientAsync();
  Receive(client);
}

async void Receive(TcpClient client)
{
  var stream = client.GetStream();
  var reader = new StreamReader(stream);
  var str = await reader.ReadToEndAsync();
  Print(str);
}

[MethodImpl(MethodImplOptions.Synchronized)]
void Print(string s) => Console.WriteLine(s);
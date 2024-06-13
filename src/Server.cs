using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

var okResponse = "HTTP/1.1 200 OK\r\n\r\n";
var notFoundResponse = "HTTP/1.1 404 Not Found\r\n\r\n";


// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

byte[] buffer = new byte[1024];

var socket = server.AcceptSocket();
socket.Receive(buffer);
string requestMessage = Encoding.UTF8.GetString(buffer);

string[] result = requestMessage.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

Console.WriteLine($"Result : {result[0]}");

string[] cutResult = result[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
if (cutResult[1].Equals("/"))
{
    socket.Send(Encoding.ASCII.GetBytes(okResponse));
}
else
{
    socket.Send(Encoding.ASCII.GetBytes(notFoundResponse));
}
